using UnityEngine;
using UnityEngine.UI;
using ModularSkinShop.Data;
using TMPro;
using Treasures.CurrencySystem;

namespace Treasures.WorldSystem
{
    /// <summary>
    /// Component that marks a location on the map where a skin can be purchased.
    /// Attach this to the same GameObject as the skin display/model.
    /// Stores the SkinSO data that will be shown in the purchase UI.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class SkinPurchasePoint : MonoBehaviour
    {
        [Tooltip("Skin data for this purchase point")]
        [SerializeField] private SkinSO skinData;

        [Tooltip("World prices")]
        [SerializeField] private GameObject pricePrefab;
        [SerializeField] private GameObject pricesRoot;

        [Tooltip("If true, the UI only opens once until the scene is reloaded.")]
        [SerializeField] private bool openOnce = false;

        private bool _used;
        private SkinPurchaseUI _purchaseUI;

        private void Start()
        {
            SpawnPrices();
            
            // Ensure collider is set as trigger
            Collider c = GetComponent<Collider>();
            if (c != null) c.isTrigger = true;
        }

        /// <summary>
        /// Spawn price elements from skin data.
        /// </summary>
        private void SpawnPrices()
        {
            if (skinData == null || pricePrefab == null || pricesRoot == null)
            {
                Debug.LogWarning("[SkinPurchasePoint] Missing references for price spawning!");
                return;
            }
        
            if (skinData.CurrencyPrice == null || skinData.CurrencyPrice.Length == 0)
            {
                Debug.LogWarning("[SkinPurchasePoint] No currency prices defined in skin data!");
                return;
            }
        
            // Clear existing prices
            foreach (Transform child in pricesRoot.transform)
                Destroy(child.gameObject);
        
            for (int i = 0; i < skinData.CurrencyPrice.Length; i++)
            {
                var price = skinData.CurrencyPrice[i];
                GameObject priceObj = Instantiate(pricePrefab, pricesRoot.transform);
                priceObj.SetActive(true);
        
                // ---- ВКЛЮЧАЕМ ВСЁ ----
                foreach (Transform child in priceObj.GetComponentsInChildren<Transform>(true))
                    if (child != priceObj.transform) child.gameObject.SetActive(true);
        
                foreach (Behaviour comp in priceObj.GetComponentsInChildren<Behaviour>(true))
                    comp.enabled = true;
                // ----------------------
        
                // Find Icon (Image) in children
                Image iconImage = priceObj.transform.Find("Icon")?.GetComponent<Image>();
                if (iconImage != null && CurrencyService.Instance != null && CurrencyService.Instance.Database != null)
                {
                    var currencyDef = CurrencyService.Instance.Database.GetDefinition(price.Type);
                    if (currencyDef != null && currencyDef.Icon != null)
                    {
                        iconImage.sprite = currencyDef.Icon;
                        iconImage.enabled = true; // теперь уже не обязательно, но оставьте на всякий случай
                    }
                }
        
                // Find Text (TMP_Text) in children
                TMP_Text priceText = priceObj.transform.Find("Text")?.GetComponent<TMP_Text>();
                if (priceText != null)
                {
                    priceText.text = price.Value.ToString();
                    priceText.enabled = true;
                }
            }
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                if (openOnce && _used) return;

                PlayerController pc = other.GetComponentInParent<PlayerController>();
                if (pc == null) return;

                if (skinData == null)
                {
                    Debug.LogWarning("[SkinPurchasePoint] No skin data assigned!");
                    return;
                }

                // Find SkinPurchaseUI
                if (_purchaseUI == null)
                {
                    _purchaseUI = UIRegistry.Get("SkinPurchase") as SkinPurchaseUI;
                }

                if (_purchaseUI != null)
                {
                    _purchaseUI.SetSkinData(skinData);
                    _purchaseUI.Open();
                    _used = true;
                }
                else
                {
                    Debug.LogWarning("[SkinPurchasePoint] SkinPurchaseUI not found in UIRegistry!");
                }
            }
        }

        /// <summary>
        /// Get the skin data for this purchase point.
        /// </summary>
        public SkinSO SkinData => skinData;
    }
}
