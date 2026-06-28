using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

namespace Treasures.BuildUtils
{
    public class BuildUIToggler : MonoBehaviour
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        private bool _textVisible = true;
        private bool _uiVisible = true;

        private void Awake()
        {
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.tKey.wasPressedThisFrame)
            {
                ToggleText();
            }

            if (keyboard.yKey.wasPressedThisFrame)
            {
                ToggleUI();
            }
        }

        private void ToggleText()
        {
            _textVisible = !_textVisible;
            
            var tmpTexts = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var text in tmpTexts)
            {
                text.enabled = _textVisible;
            }

            var legacyTexts = Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var text in legacyTexts)
            {
                text.enabled = _textVisible;
            }
        }

        private void ToggleUI()
        {
            _uiVisible = !_uiVisible;
            
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var canvas in canvases)
            {
                canvas.enabled = _uiVisible;
            }
        }
#endif
    }
}
