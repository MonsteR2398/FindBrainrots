using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ModularTreasures;
using L10n = Treasures.Localization.Localization;

namespace Treasures.IAP
{
    public class ShopItemUI : MonoBehaviour
    {
        [Header("Offer Data")]
        [SerializeField] private OfferSO offer;

        [Header("UI Components")]
        [SerializeField] private TMPro.TextMeshProUGUI titleText;
        [SerializeField] private TMPro.TextMeshProUGUI priceText;
        [SerializeField] private TMPro.TextMeshProUGUI rewardText;
        [SerializeField] private UnityEngine.UI.Image iconImage;
        [SerializeField] private UnityEngine.UI.Button buyButton;

        private void Start()
        {
            if (buyButton != null)
            {
                buyButton.onClick.AddListener(OnBuyClicked);
            }

            if (offer != null)
            {
                RefreshUI();
            }
        }

        private void OnEnable()
        {
            // Live refresh when the player switches language while the shop is visible.
            L10n.LanguageChanged += RefreshUI;
        }

        private void OnDisable()
        {
            L10n.LanguageChanged -= RefreshUI;
        }

        private void OnDestroy()
        {
            if (buyButton != null)
            {
                buyButton.onClick.RemoveListener(OnBuyClicked);
            }
        }

        public void Setup(OfferSO newOffer)
        {
            offer = newOffer;
            RefreshUI();
        }

        public void RefreshUI()
        {
            if (offer == null) return;

            // The offer title doubles as a localization key: if an entry with this key exists
            // in LocalizationTable it is translated, otherwise the raw title is shown as-is.
            string localizedTitle = L10n.Get(offer.Title);
            Debug.Log($"[Shop][Diag] Offer '{offer.name}' title='{offer.Title}' -> localized='{localizedTitle}'");
            if (titleText != null) titleText.text = localizedTitle;
            int reward = 0;
            if(offer.CurrencyRewards.Count > 0)
                reward = offer.CurrencyRewards[0].Value;
            else if(offer.BoostRewards.Count > 0)
                reward = offer.BoostRewards[0].Value;

            if (rewardText != null) rewardText.text = NumberFormatter.Format(reward, NumberFormatMode.Separated);
            if (iconImage != null && offer.Icon != null) iconImage.sprite = offer.Icon;

            if (priceText != null)
            {
                string price = null;
                if (IAPManager.Instance != null && IAPManager.Instance.IsInitialized())
                {
                    price = IAPManager.Instance.GetLocalizedPrice(offer.ProductID);
                }
                priceText.text = price ?? "N/A";
            }
        }

        private void OnBuyClicked()
        {
            if (offer == null) return;

            if (IAPManager.Instance != null)
            {
                IAPManager.Instance.InitiatePurchase(offer.ProductID);
            }
            else
            {
                Debug.LogError("ShopItemUI: IAPManager.Instance is missing!");
            }
        }
    }
}