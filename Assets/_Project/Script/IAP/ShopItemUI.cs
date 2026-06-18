using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Treasures.IAP
{
    public class ShopItemUI : MonoBehaviour
    {
        [Header("Offer Data")]
        [SerializeField] private OfferSO offer;

        [Header("UI Components")]
        [SerializeField] private TMPro.TextMeshProUGUI titleText;
        [SerializeField] private TMPro.TextMeshProUGUI descriptionText;
        [SerializeField] private TMPro.TextMeshProUGUI priceText;
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

            if (titleText != null) titleText.text = offer.Title;
            if (descriptionText != null) descriptionText.text = offer.Description;
            if (iconImage != null && offer.Icon != null) iconImage.sprite = offer.Icon;

            if (priceText != null)
            {
                if (IAPManager.Instance != null && IAPManager.Instance.IsInitialized())
                {
                    priceText.text = IAPManager.Instance.GetLocalizedPrice(offer.ProductID, offer.DefaultPriceString);
                }
                else
                {
                    priceText.text = offer.DefaultPriceString;
                }
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