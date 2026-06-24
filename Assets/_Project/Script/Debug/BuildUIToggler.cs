using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

namespace Treasures.BuildUtils
{
    /// <summary>
    /// Utility script to toggle visibility of UI and Text in the game.
    /// Only works in Unity Editor and Standalone (PC) builds.
    /// Key T: Toggle all Text visibility.
    /// Key Y: Toggle all UI visibility.
    /// </summary>
    public class BuildUIToggler : MonoBehaviour
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        private bool _textVisible = true;
        private bool _uiVisible = true;

        private void Awake()
        {
            // Persist across scenes
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
            
            // Toggle all TextMeshPro components in all loaded scenes
            var tmpTexts = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var text in tmpTexts)
            {
                text.enabled = _textVisible;
            }

            // Toggle all legacy UI Text components
            var legacyTexts = Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var text in legacyTexts)
            {
                text.enabled = _textVisible;
            }
        }

        private void ToggleUI()
        {
            _uiVisible = !_uiVisible;
            
            // Toggle all Canvas components to hide/show UI
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var canvas in canvases)
            {
                canvas.enabled = _uiVisible;
            }
        }
#endif
    }
}
