using TMPro;
using UnityEngine;

namespace Treasures.Localization
{
    /// <summary>
    /// Attach to any TextMeshPro text to make it translatable. The component looks up
    /// <see cref="key"/> in <see cref="Localization"/> and refreshes automatically whenever the
    /// active language changes. Dynamic texts (numbers, names) should NOT use this component.
    /// </summary>
    [DisallowMultipleComponent]
    public class LocalizedText : MonoBehaviour
    {
        [Tooltip("Translation key, e.g. settings.music")]
        [SerializeField] private string key;

        private TMP_Text _text;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            Localization.LanguageChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            Localization.LanguageChanged -= Refresh;
        }

        /// <summary>Assign a key at runtime and refresh immediately.</summary>
        public void SetKey(string newKey)
        {
            key = newKey;
            Refresh();
        }

        /// <summary>Re-read the translation for the current key/language.</summary>
        public void Refresh()
        {
            if (_text == null) _text = GetComponent<TMP_Text>();
            if (_text != null && !string.IsNullOrEmpty(key))
                _text.text = Localization.Get(key);
        }
    }
}
