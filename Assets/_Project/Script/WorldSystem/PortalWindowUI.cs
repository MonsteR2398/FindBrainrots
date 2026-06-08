using System.Collections.Generic;
using UnityEngine;

namespace Treasures.WorldSystem
{
    /// <summary>
    /// Singleton window (in the core Canvas) that lists every world from the catalog
    /// as a button. Selecting one tells the GameModeManager to switch.
    /// </summary>
    public class PortalWindowUI : MonoBehaviour
    {
        public static PortalWindowUI Instance { get; private set; }

        [Header("References")]
        [Tooltip("Root object toggled on/off when opening/closing the window.")]
        [SerializeField] private GameObject root;
        [Tooltip("Parent (with a Layout Group) that the world buttons are spawned under.")]
        [SerializeField] private Transform buttonContainer;
        [Tooltip("Disabled button used as a template, cloned per world.")]
        [SerializeField] private GameObject buttonTemplate;
        [SerializeField] private GameModeCatalog catalog;

        [Header("Behaviour")]
        [Tooltip("Free/show the cursor while the window is open, then re-lock on close.")]
        [SerializeField] private bool manageCursor = true;

        private readonly List<GameObject> _spawned = new List<GameObject>();
        private bool _built;
        private PlayerController _player;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (buttonTemplate != null) buttonTemplate.SetActive(false);
            if (root != null) root.SetActive(false);
        }

        public bool IsOpen => root != null && root.activeSelf;

        public void Open()
        {
            BuildIfNeeded();
            if (root != null) root.SetActive(true);

            if (_player == null) _player = FindFirstObjectByType<PlayerController>();
            if (_player != null) _player.enabled = false;

            if (manageCursor)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public void Close()
        {
            if (root != null) root.SetActive(false);
            if (_player != null) _player.enabled = true;

            if (manageCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void BuildIfNeeded()
        {
            if (_built) return;
            if (catalog == null || catalog.Modes == null || buttonTemplate == null || buttonContainer == null)
            {
                Debug.LogWarning("[PortalWindowUI] Missing references - cannot build buttons.");
                return;
            }

            foreach (GameModeDefinition mode in catalog.Modes)
            {
                if (mode == null) continue;

                GameObject go = Instantiate(buttonTemplate, buttonContainer);
                go.SetActive(true);
                go.name = "PortalButton_" + mode.DisplayName;

                TMPro.TMP_Text label = go.GetComponentInChildren<TMPro.TMP_Text>(true);
                if (label != null) label.text = mode.DisplayName;

                UnityEngine.UI.Image bg = go.GetComponent<UnityEngine.UI.Image>();
                if (bg != null) bg.color = mode.AccentColor;

                // Optional icon: a child Image named "Icon".
                Transform iconTf = go.transform.Find("Icon");
                if (iconTf != null)
                {
                    UnityEngine.UI.Image iconImg = iconTf.GetComponent<UnityEngine.UI.Image>();
                    if (iconImg != null)
                    {
                        if (mode.Icon != null) { iconImg.sprite = mode.Icon; iconImg.enabled = true; }
                        else iconImg.enabled = false;
                    }
                }

                GameModeDefinition captured = mode;
                UnityEngine.UI.Button btn = go.GetComponent<UnityEngine.UI.Button>();
                if (btn != null) btn.onClick.AddListener(() => OnSelect(captured));

                _spawned.Add(go);
            }

            _built = true;
        }

        private void OnSelect(GameModeDefinition mode)
        {
            Close();
            if (GameModeManager.Instance != null) GameModeManager.Instance.SwitchTo(mode);
            else Debug.LogWarning("[PortalWindowUI] No GameModeManager in the scene.");
        }
    }
}
