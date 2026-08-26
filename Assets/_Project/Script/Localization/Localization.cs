using System;
using System.Collections.Generic;
using UnityEngine;
using YG;
using PlayerPrefs = RedefineYG.PlayerPrefs;

namespace Treasures.Localization
{
    /// <summary>
    /// Static access point for runtime text translation. Loads a <see cref="LocalizationTable"/>
    /// from Resources, picks the initial language (saved choice -> OS language -> English),
    /// and exposes <see cref="Get"/> for lookups plus a change event for live UI updates.
    ///
    /// Language persistence: the selected language code is stored in PlayerPrefs under
    /// <c>settings.language</c> and restored on the next session.
    /// </summary>
    public static class Localization
    {
        private const string ResourcePath = "LocalizationTable";
        private const string PrefsKey = "settings.language";

        private static LocalizationTable _table;
        private static Dictionary<string, string[]> _lookup;
        private static int _languageIndex;
        private static bool _initialized;

        /// <summary>Raised whenever the active language changes (so UI can refresh).</summary>
        public static event Action LanguageChanged;

        /// <summary>True once the table has been loaded successfully.</summary>
        public static bool IsReady => _table != null;

        /// <summary>Number of supported languages.</summary>
        public static int LanguageCount => _table != null ? _table.languageCodes.Length : 0;

        /// <summary>Native display names of all supported languages.</summary>
        public static string[] LanguageNames =>
            _table != null ? _table.languageNames : Array.Empty<string>();

        /// <summary>Active language code (e.g. "ru"). Empty when not initialized.</summary>
        public static string CurrentLanguageCode =>
            (_table != null && _languageIndex >= 0 && _languageIndex < _table.languageCodes.Length)
                ? _table.languageCodes[_languageIndex]
                : string.Empty;

        /// <summary>Native name of the active language (e.g. "Русский").</summary>
        public static string CurrentLanguageName =>
            (_table != null && _languageIndex >= 0 && _languageIndex < _table.languageNames.Length)
                ? _table.languageNames[_languageIndex]
                : string.Empty;

        /// <summary>
        /// Index of the active language. Setting it clamps to range, persists the choice,
        /// and raises <see cref="LanguageChanged"/>.
        /// </summary>
        public static int CurrentLanguageIndex
        {
            get => _languageIndex;
            set
            {
                EnsureInitialized();
                if (_table == null || _table.languageCodes.Length == 0) return;

                int clamped = Mathf.Clamp(value, 0, _table.languageCodes.Length - 1);
                if (clamped == _languageIndex) return;

                _languageIndex = clamped;
                PlayerPrefs.SetString(PrefsKey, _table.languageCodes[_languageIndex]);
                PlayerPrefs.Save();
                LanguageChanged?.Invoke();
            }
        }

        /// <summary>Initialize on game start so the first scene already shows the right language.</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInit() => EnsureInitialized();

        /// <summary>Idempotent. Loads the table and resolves the initial language exactly once.</summary>
        public static void EnsureInitialized()
        {
            if (_initialized) return;
            _initialized = true;

            _table = Resources.Load<LocalizationTable>(ResourcePath);
            if (_table == null)
            {
                Debug.LogError($"[Localization] Could not load table at Resources/{ResourcePath}. Text will not be translated.");
                return;
            }

            _lookup = new Dictionary<string, string[]>(_table.entries.Count);
            foreach (var e in _table.entries)
            {
                if (e != null && !string.IsNullOrEmpty(e.key) && !_lookup.ContainsKey(e.key))
                    _lookup[e.key] = e.values;
            }

            _languageIndex = ResolveInitialLanguage();

            // With YG2 Storage, cloud/local saves finish loading asynchronously and REPLACE
            // everything that was written before that point. Once the SDK data arrives,
            // re-apply the persisted language choice so it survives the save-load swap.
            YG2.onGetSDKData += OnSdkDataLoaded;
        }

        private static void OnSdkDataLoaded()
        {
            // 1) Explicit player choice (persisted) always wins.
            string saved;
            try
            {
                saved = PlayerPrefs.GetString(PrefsKey, string.Empty);
            }
            catch
            {
                return;
            }

            if (!string.IsNullOrEmpty(saved))
            {
                if (_table == null) return;

                int savedIdx = Array.IndexOf(_table.languageCodes, saved);
                if (savedIdx >= 0 && savedIdx != _languageIndex)
                    CurrentLanguageIndex = savedIdx; // Fires LanguageChanged so all UI refreshes.
                return;
            }

            // 2) No saved choice yet -> ask the platform (Yandex Games) for the player's language,
            //    falling back to the browser/OS language.
            int suggested = GetSuggestedLanguageIndex();
            if (suggested >= 0 && suggested != _languageIndex && _table != null)
                CurrentLanguageIndex = suggested; // Also persists the choice for next sessions.
        }

        /// <summary>
        /// Suggested language index from the environment:
        /// YG2 player language when the EnvirData module is installed, otherwise the OS/browser language.
        /// Returns -1 when nothing supported was detected.
        /// </summary>
        private static int GetSuggestedLanguageIndex()
        {
#if EnvirData_yg
            try
            {
                string ygCode = YG2.envirData.language;
                if (!string.IsNullOrEmpty(ygCode))
                {
                    // CIS variants reported by Yandex map to the Russian table entry.
                    if (ygCode == "be" || ygCode == "kk" || ygCode == "uk") ygCode = "ru";

                    int ygIdx = Array.IndexOf(_table.languageCodes, ygCode.ToLowerInvariant());
                    if (ygIdx >= 0) return ygIdx;
                }
            }
            catch
            {
                // envirData not ready - fall through to OS language.
            }
#endif
            string osCode = SystemLanguageToCode(Application.systemLanguage);
            int osIdx = _table != null ? Array.IndexOf(_table.languageCodes, osCode) : -1;
            return osIdx;
        }

        /// <summary>
        /// Returns the saved language if valid; otherwise maps the device OS language to a
        /// supported language; otherwise falls back to the first language (English).
        /// </summary>
        private static int ResolveInitialLanguage()
        {
            // PlayerPrefs (RedefineYG) touches the YG2 platform object which may not exist yet
            // if this static initializer runs before YG2.Initialize - degrade gracefully.
            string saved = string.Empty;
            try
            {
                saved = PlayerPrefs.GetString(PrefsKey, string.Empty);
            }
            catch
            {
                // Platform not ready; OnSdkDataLoaded will re-apply the choice after init.
            }

            if (!string.IsNullOrEmpty(saved))
            {
                int idx = Array.IndexOf(_table.languageCodes, saved);
                if (idx >= 0) return idx;
            }

            string osCode = SystemLanguageToCode(Application.systemLanguage);
            int osIdx = Array.IndexOf(_table.languageCodes, osCode);
            return osIdx >= 0 ? osIdx : 0;
        }

        private static string SystemLanguageToCode(SystemLanguage lang)
        {
            switch (lang)
            {
                case SystemLanguage.Russian: return "ru";
                case SystemLanguage.Spanish: return "es";
                case SystemLanguage.Portuguese: return "pt";
                case SystemLanguage.French: return "fr";
                case SystemLanguage.German: return "de";
                case SystemLanguage.Italian: return "it";
                case SystemLanguage.English: return "en";
                default: return "en";
            }
        }

        /// <summary>
        /// Returns the translation for <paramref name="key"/> in the active language.
        /// Falls back to the first language if the active value is empty, and to the raw key
        /// if the key is unknown (so missing translations are visible during development).
        /// </summary>
        public static string Get(string key)
        {
            EnsureInitialized();
            if (string.IsNullOrEmpty(key) || _lookup == null) return key;

            if (_lookup.TryGetValue(key, out var values) && values != null && values.Length > 0)
            {
                if (_languageIndex < values.Length && !string.IsNullOrEmpty(values[_languageIndex]))
                    return values[_languageIndex];
                if (!string.IsNullOrEmpty(values[0]))
                    return values[0];
            }
            return key;
        }
    }
}
