using System.Collections.Generic;
using UnityEngine;
using Treasures.Localization;

namespace Treasures.WorldSystem
{
    /// <summary>
    /// Singleton window (in the core Canvas) that lists every world from the catalog
    /// as a button. Selecting one tells the GameModeManager to switch.
    /// </summary>
    public class PortalWindowUI : MonoBehaviour, IUIOpenable
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
        [SerializeField] private PlayerController _player;

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

            UIRegistry.Register("PortalWindow", this);
        }

        private void OnDestroy()
        {
            UIRegistry.Unregister("PortalWindow");
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

        private bool IsWorldPurchased(GameModeDefinition mode)
        {
            if (mode == null) return true;
            if (mode.price == null || mode.price.Value <= 0) return true;

            string key = "WorldPurchased_" + mode.Id;
            return PlayerPrefs.GetInt(key, 0) == 1;
        }

        private void PurchaseWorld(GameModeDefinition mode, GameObject lockGo)
        {
            if (mode == null || mode.price == null) return;

            if (Treasures.CurrencySystem.CurrencyService.Instance != null)
            {
                if (Treasures.CurrencySystem.CurrencyService.Instance.TrySpend(mode.price.Type, mode.price.Value))
                {
                    string key = "WorldPurchased_" + mode.Id;
                    PlayerPrefs.SetInt(key, 1);
                    PlayerPrefs.Save();

                    if (lockGo != null)
                    {
                        lockGo.SetActive(false);
                    }

                    Debug.Log($"[PortalWindowUI] Successfully purchased world: {mode.DisplayName}");
                }
                else
                {
                    Debug.LogWarning($"[PortalWindowUI] Not enough {mode.price.Type} to purchase {mode.DisplayName}. Required: {mode.price.Value}");
                }
            }
            else
            {
                Debug.LogError("[PortalWindowUI] CurrencyService instance is missing!");
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

            // Cleanup any existing buttons that might be there (e.g. from editor or previous builds)
            foreach (var go in _spawned)
            {
                if (go != null)
                {
                    if (Application.isPlaying) Destroy(go);
                    else DestroyImmediate(go);
                }
            }
            _spawned.Clear();

            // Also cleanup any stray children in the container that are not the template
            for (int i = buttonContainer.childCount - 1; i >= 0; i--)
            {
                Transform child = buttonContainer.GetChild(i);
                if (child.gameObject != buttonTemplate && child.name.StartsWith("PortalButton_"))
                {
                    if (Application.isPlaying) Destroy(child.gameObject);
                    else DestroyImmediate(child.gameObject);
                }
            }

            foreach (GameModeDefinition mode in catalog.Modes)
            {
                if (mode == null) continue;

                GameObject go = Instantiate(buttonTemplate, buttonContainer);
                go.SetActive(true);
                go.name = "PortalButton_" + mode.DisplayName;

                Transform labelTf = go.transform.Find("Label");
                TMPro.TMP_Text label = labelTf != null ? labelTf.GetComponent<TMPro.TMP_Text>() : null;
                if (label != null)
                {
                    label.text = Treasures.Localization.Localization.Get(mode.DisplayName);
                }

                UnityEngine.UI.Image bg = go.GetComponent<UnityEngine.UI.Image>();
                if (bg != null)
                {
                    bg.color = mode.AccentColor;
                    bg.sprite = mode.Icon;
                }    
                

                Transform lockTf = go.transform.Find("Lock");
                if (lockTf != null)
                {
                    bool isPurchased = IsWorldPurchased(mode);
                    lockTf.gameObject.SetActive(!isPurchased);

                    if (!isPurchased)
                    {
                        Transform buyBtnTf = lockTf.Find("BuyButton");
                        if (buyBtnTf != null)
                        {
                            UnityEngine.UI.Button buyBtn = buyBtnTf.GetComponent<UnityEngine.UI.Button>();
                            if (buyBtn != null)
                            {
                                buyBtn.onClick.RemoveAllListeners();
                                GameObject lockGo = lockTf.gameObject;
                                GameModeDefinition capturedMode = mode;
                                buyBtn.onClick.AddListener(() => PurchaseWorld(capturedMode, lockGo));
                            }

                            TMPro.TMP_Text priceLabel = buyBtnTf.GetComponentInChildren<TMPro.TMP_Text>(true);
                            if (priceLabel != null && mode.price != null)
                            {
                                priceLabel.text = mode.price.Value.ToString();
                            }

                            Transform currencyIconTf = buyBtnTf.Find("Icon");
                            if (currencyIconTf != null && mode.price != null)
                            {
                                UnityEngine.UI.Image currencyImg = currencyIconTf.GetComponent<UnityEngine.UI.Image>();
                                if (currencyImg != null && Treasures.CurrencySystem.CurrencyService.Instance != null)
                                {
                                    var database = Treasures.CurrencySystem.CurrencyService.Instance.Database;
                                    if (database != null)
                                    {
                                        var currencyDef = database.GetDefinition(mode.price.Type);
                                        if (currencyDef != null && currencyDef.Icon != null)
                                        {
                                            currencyImg.sprite = currencyDef.Icon;
                                            currencyImg.enabled = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                GameModeDefinition captured = mode;
                UnityEngine.UI.Button btn = go.GetComponent<UnityEngine.UI.Button>();
                if (btn != null) btn.onClick.AddListener(() => OnSelect(captured));

                _spawned.Add(go);
            }

            _built = true;
        }

        private void OnEnable()
        {
            Treasures.Localization.Localization.LanguageChanged += Rebuild;
        }

        private void OnDisable()
        {
            Treasures.Localization.Localization.LanguageChanged -= Rebuild;
        }

        private void Rebuild()
        {
            if (!_built) return;
            _built = false;
            BuildIfNeeded();
        }

        private void OnSelect(GameModeDefinition mode)
        {
            if (!IsWorldPurchased(mode))
            {
                Debug.LogWarning($"[PortalWindowUI] World '{mode.DisplayName}' is locked and not purchased.");
                return;
            }

            Close();
            if (GameModeManager.Instance != null) GameModeManager.Instance.SwitchTo(mode);
            else Debug.LogWarning("[PortalWindowUI] No GameModeManager in the scene.");
        }
    }
}
