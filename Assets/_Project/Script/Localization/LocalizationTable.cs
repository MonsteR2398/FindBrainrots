using System.Collections.Generic;
using UnityEngine;

namespace Treasures.Localization
{
    /// <summary>
    /// Lightweight translation database. Holds the list of supported languages and a flat list
    /// of key -> per-language value entries. One asset of this type lives in a Resources folder
    /// and is loaded at runtime by <see cref="Localization"/>.
    ///
    /// The <c>values</c> array on each entry is index-aligned with <see cref="languageCodes"/>
    /// (and <see cref="languageNames"/>): values[i] is the translation for languageCodes[i].
    /// </summary>
    [CreateAssetMenu(fileName = "LocalizationTable", menuName = "Treasures/Localization Table")]
    public class LocalizationTable : ScriptableObject
    {
        [System.Serializable]
        public class Entry
        {
            public string key;
            [TextArea] public string[] values;
        }

        [Tooltip("ISO-ish language codes, e.g. en, ru, es. Order defines column order.")]
        public string[] languageCodes;

        [Tooltip("Native display names shown in the language selector, aligned with languageCodes.")]
        public string[] languageNames;

        public List<Entry> entries = new List<Entry>();
    }
}
