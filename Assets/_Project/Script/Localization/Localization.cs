using System;
using System.Collections.Generic;
using UnityEngine;
using YG;

namespace Treasures.Localization
{
    /// <summary>
    /// Static access point for runtime text translation. Loads a <see cref="LocalizationTable"/>
    /// from Resources and exposes <see cref="Get"/> for lookups plus a change event for live UI
    /// updates.
    ///
    /// The active language always follows the language offered by the SDK platform (PluginYG2
    /// "Localization" module): <c>YG2.lang</c> is resolved from the platform on every launch
    /// (SetLangMod.EveryGameLaunch for Yandex Games) and pushed to this class through the
    /// <see cref="YG2.onCorrectLang"/> / <see cref="YG2.onSwitchLang"/> events. Nothing is
    /// persisted by this class - no PlayerPrefs, no saves. In-game switches call
    /// <see cref="YG2.SwitchLanguage(string)"/> so the SDK stays in sync (which itself never
    /// overrides the platform language on the next launch under EveryGameLaunch).
    /// </summary>
    public static class Localization
    {
        private const string ResourcePath = "LocalizationTable";

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
        /// Index of the active language. Setting it (used by the in-game language selector)
        /// clamps to range, raises <see cref="LanguageChanged"/> and calls
        /// <see cref="YG2.SwitchLanguage(string)"/> so the SDK follows. No PlayerPrefs/saves.
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
                LanguageChanged?.Invoke();

                string code = _table.languageCodes[clamped];
                if (YG2.lang != code)
                    YG2.SwitchLanguage(code); // Syncs YG2.lang and fires onSwitchLang (no save here).
            }
        }

        /// <summary>Initialize on game start so the first scene already shows the right language.</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInit() => EnsureInitialized();

        /// <summary>Idempotent. Loads the table and wires up the YG2 localisation events.</summary>
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

            // Initial language = what the SDK platform already resolved for the current launch.
            // The PluginYG2 Localization module resolves YG2.lang from the platform (for Yandex
            // Games this happens every launch) and delivers it through onCorrectLang/onSwitchLang,
            // so we also subscribe to those events to catch a late or synchronous delivery.
            _languageIndex = IndexFromCode(YG2.lang);
            if (_languageIndex < 0) _languageIndex = 0;

            YG2.onCorrectLang += OnYgCorrectLang;
            YG2.onSwitchLang += OnYgSwitchLang;
        }

        /// <summary>
        /// Fired by PluginYG2 when the language is read from the SDK platform at launch
        /// (<c>GetLanguage()</c>). Maps the platform country code to this game's table and applies it.
        /// </summary>
        private static void OnYgCorrectLang(string lang)
        {
            if (_table == null) return;
            int idx = IndexFromCode(lang);
            if (idx >= 0 && idx != _languageIndex)
            {
                _languageIndex = idx;
                LanguageChanged?.Invoke();
            }
        }

        /// <summary>
        /// Fired by PluginYG2 after the language changes (startup and <c>SwitchLanguage</c>).
        /// </summary>
        private static void OnYgSwitchLang(string lang)
        {
            OnYgCorrectLang(lang);
        }

        /// <summary>
        /// Maps a PluginYG2 country code (the value of <c>YG2.lang</c>, e.g. "ru", "en", "de",
        /// "tr") to an index in the game's <see cref="LocalizationTable"/>.
        ///
        /// Supported codes (en, ru, es, pt, fr, de, it) are used directly. CIS Russian-speaking
        /// codes reported by Yandex (be, kk, uk, az, ky, tg, tk, uz) map to the Russian column,
        /// since the game ships with Russian text for those regions. Any other unsupported code
        /// (e.g. tr) falls back to English. Returns -1 when <paramref name="code"/> is invalid
        /// or the table is not loaded.
        /// </summary>
        private static int IndexFromCode(string code)
        {
            if (_table == null || string.IsNullOrEmpty(code)) return -1;

            string c = code.Trim().ToLowerInvariant();

            // Direct match against any supported column (e.g. "de" -> Deutsch).
            int idx = Array.IndexOf(_table.languageCodes, c);
            if (idx >= 0) return idx;

            // CIS Russian-speaking locales share the Russian translation.
            switch (c)
            {
                case "be":
                case "kk":
                case "uk":
                case "az":
                case "ky":
                case "tg":
                case "tk":
                case "uz":
                case "mo":
                    return Array.IndexOf(_table.languageCodes, "ru");
            }

            // Unsupported language -> English fallback. Never Russian unless Russian/CIS.
            return Array.IndexOf(_table.languageCodes, "en");
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
