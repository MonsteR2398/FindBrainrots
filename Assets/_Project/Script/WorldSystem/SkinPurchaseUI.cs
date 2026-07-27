using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ModularSkinShop.Data;
using Treasures.CurrencySystem;
using Treasures.WorldSystem;
using ModularSkinShop.Core;

namespace Treasures.WorldSystem
{
    /// <summary>
    /// UI window for purchasing a skin on the map (not in the shop).
    /// Shows skin icon, name, price, and Buy/Select buttons.
    /// Supports multiple currencies (Gold, Diamond, etc.)
    /// Similar to SkinShopItemUI but for map-based purchases.
    /// </summary>
    public class SkinPurchaseUI : MonoBehaviour, IUIOpenable
    {
        [Header("UI References")]
        [SerializeField] private GameObject root;
        [SerializeField] private Image skinIcon;
        [SerializeField] private TMP_Text skinNameText;
        [SerializeField] private TMP_Text interactionText;
        [SerializeField] private Button interactionButton;
        [SerializeField] private Transform buyButtonContainer;
        [SerializeField] private GameObject buyButtonPrefab;

        [Header("Visual")]
        [SerializeField] private Sprite buySprite;
        [SerializeField] private Sprite noEnoughCurrencySprite;
        [SerializeField] private Sprite unlockedSprite;
        [SerializeField] private Sprite selectedSprite;

        [Header("3D Preview")]
        [SerializeField] private RawImage modelPreview;
        [SerializeField] private SkinShopRender skinShopRender;

        [Header("References")]
        [SerializeField] private SkinShopManager skinShopManager;

        private SkinSO _currentSkin;
        private List<Button> _buyButtons = new List<Button>();

        private void Awake()
        {
            UIRegistry.Register("SkinPurchase", this);

            // Setup button listeners
            if (interactionButton != null)
                interactionButton.onClick.AddListener(OnInteractionClicked);

            // Find SkinShopRender if not assigned
            if (skinShopRender == null)
                skinShopRender = FindFirstObjectByType<SkinShopRender>();

            // Hide all buttons by default
            if (interactionButton != null) interactionButton.gameObject.SetActive(false);
            if (buyButtonContainer != null) buyButtonContainer.gameObject.SetActive(false);

            // Hide by default
            if (root != null) root.SetActive(false);
        }

        private void OnDestroy()
        {
            UIRegistry.Unregister("SkinPurchase");
        }

        /// <summary>
        /// Show the purchase UI with the nearest skin data.
        /// </summary>
        public void Open()
        {
            if (root != null) root.SetActive(true);
            
            // Find SkinShopManager if not assigned
            if (skinShopManager == null)
            {
                skinShopManager = FindFirstObjectByType<SkinShopManager>();
                if (skinShopManager != null)
                {
                    Debug.Log("[SkinPurchaseUI] SkinShopManager found automatically");
                    
                    // Verify persistence is set up
                    var persistence = skinShopManager.GetComponent<ModularSkinShop.Interfaces.IShopPersistence>();
                    if (persistence == null)
                    {
                        Debug.LogWarning("[SkinPurchaseUI] SkinShopManager has no IShopPersistence component! Add MockShopSave or custom persistence.");
                    }
                }
                else
                {
                    Debug.LogError("[SkinPurchaseUI] SkinShopManager not found in scene! Make sure it exists.");
                }
            }
            
            UpdateUI();
        }

        /// <summary>
        /// Hide the purchase UI.
        /// </summary>
        public void Close()
        {
            if (root != null) root.SetActive(false);
            _currentSkin = null;
        }

        /// <summary>
        /// Set the skin data to display in the purchase UI.
        /// </summary>
        public void SetSkinData(SkinSO skin)
        {
            _currentSkin = skin;
            UpdateUI();
        }

        public void UpdateUI()
        {
            if (_currentSkin == null)
            {
                Debug.LogWarning("[SkinPurchaseUI] No skin data to display!");
                return;
            }

            // Update icon
            if (skinIcon != null)
                skinIcon.sprite = _currentSkin.Icon;

            // Update name
            if (skinNameText != null)
                skinNameText.text = _currentSkin.DisplayName;

            if (skinShopManager == null)
            {
                Debug.LogWarning("[SkinPurchaseUI] SkinShopManager not found!");
                return;
            }

            // Clear old buy buttons
            ClearBuyButtons();

            // Check skin state
            bool isActive = skinShopManager.IsSkinActive(_currentSkin);
            bool isUnlocked = skinShopManager.IsSkinUnlocked(_currentSkin);

            if (isActive)
            {
                // Skin is currently worn
                interactionText.text = "Active";
                interactionButton.gameObject.SetActive(true);
                interactionButton.interactable = false;
                if (unlockedSprite != null && interactionButton.TryGetComponent(out Image img))
                    img.sprite = unlockedSprite;
                
                buyButtonContainer.gameObject.SetActive(false);
            }
            else if (isUnlocked)
            {
                // Skin is unlocked but not worn
                interactionText.text = "Select";
                interactionButton.gameObject.SetActive(true);
                interactionButton.interactable = true;
                if (selectedSprite != null && interactionButton.TryGetComponent(out Image img))
                    img.sprite = selectedSprite;
                
                buyButtonContainer.gameObject.SetActive(false);
            }
            else
            {
                // Skin is not unlocked - show buy buttons
                interactionButton.gameObject.SetActive(false);
                buyButtonContainer.gameObject.SetActive(true);

                // Create buy buttons for each currency
                if (_currentSkin.CurrencyPrice != null && CurrencyService.Instance != null)
                {
                    for (int i = 0; i < _currentSkin.CurrencyPrice.Length; i++)
                    {
                        int capturedIndex = i;
                        var price = _currentSkin.CurrencyPrice[i];

                        GameObject buyBtnObj = Instantiate(buyButtonPrefab, buyButtonContainer);
                        buyBtnObj.SetActive(true);
                        _buyButtons.Add(buyBtnObj.GetComponent<Button>());

                        // Update button text
                        TMP_Text priceText = buyBtnObj.GetComponentInChildren<TMP_Text>();
                        if (priceText != null)
                            priceText.text = price.Value.ToString();

                        // Update currency icon
                        Image currencyIcon = buyBtnObj.transform.Find("Icon")?.GetComponent<Image>();
                        if (currencyIcon != null && CurrencyService.Instance.Database != null)
                        {
                            var currencyDef = CurrencyService.Instance.Database.GetDefinition(price.Type);
                            if (currencyDef != null && currencyDef.Icon != null)
                            {
                                currencyIcon.sprite = currencyDef.Icon;
                                currencyIcon.enabled = true;
                            }
                        }

                        // Check if player can afford
                        long balance = CurrencyService.Instance.GetBalance(price.Type);
                        bool canAfford = balance >= price.Value;

                        // Setup button
                        Button btn = buyBtnObj.GetComponent<Button>();
                        btn.interactable = canAfford;
                        btn.onClick.AddListener(() => OnBuyClickedWithPrice(capturedIndex));

                        // Visual feedback
                        if (!canAfford)
                        {
                            Image btnImage = buyBtnObj.GetComponent<Image>();
                            if (btnImage != null && noEnoughCurrencySprite != null)
                                btnImage.sprite = noEnoughCurrencySprite;
                        }
                        else
                        {
                            if (buySprite != null && buyBtnObj.TryGetComponent(out Image btnImage))
                                btnImage.sprite = buySprite;
                        }
                    }
                }
            }

            // Render 3D model preview
            if (skinShopRender != null)
            {
                skinShopRender.RenderSkin(_currentSkin);
            }
        }

        private void ClearBuyButtons()
        {
            foreach (var btn in _buyButtons)
            {
                if (btn != null)
                    Destroy(btn.gameObject);
            }
            _buyButtons.Clear();
        }

        private void OnInteractionClicked()
        {
            if (_currentSkin == null || skinShopManager == null) return;
            
            // Use SkinShopManager for all operations - it handles persistence automatically
            if (skinShopManager.IsSkinUnlocked(_currentSkin))
            {
                // Skin is unlocked - just dress it
                skinShopManager.TryDress(_currentSkin);
            }
            else
            {
                // Skin is locked - try to buy with first currency
                if (_currentSkin.CurrencyPrice != null && _currentSkin.CurrencyPrice.Length > 0)
                {
                    skinShopManager.TryDressOrBuy(_currentSkin, _currentSkin.CurrencyPrice[0]);
                }
            }
            
            Close();
        }

        private void OnBuyClickedWithPrice(int priceIndex)
        {
            if (_currentSkin == null || skinShopManager == null)
            {
                Debug.LogWarning("[SkinPurchaseUI] Cannot buy - missing skin data or SkinShopManager!");
                return;
            }

            if (_currentSkin.CurrencyPrice == null || priceIndex >= _currentSkin.CurrencyPrice.Length)
            {
                Debug.LogWarning("[SkinPurchaseUI] Invalid price index!");
                return;
            }

            // Try to buy the skin with selected currency
            var price = _currentSkin.CurrencyPrice[priceIndex];
            skinShopManager.TryDressOrBuy(_currentSkin, price);

            Close();
        }

        private void OnDisable()
        {
            // Clear 3D preview when UI closes
            if (skinShopRender != null)
                skinShopRender.Clear();
        }
    }
}