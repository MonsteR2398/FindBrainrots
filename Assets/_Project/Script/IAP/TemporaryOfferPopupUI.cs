using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Treasures.IAP
{
    public class TemporaryOfferPopupUI : MonoBehaviour
    {
        public static TemporaryOfferPopupUI Instance { get; private set; }

        [Header("UI Panels")]
        [SerializeField] private GameObject popupPanel;

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Button buyButton;
        [SerializeField] private Button closeButton;

        private OfferSO _currentOffer;
        private TemporaryOfferController _currentController;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            if (popupPanel != null)
            {
                popupPanel.SetActive(false);
            }
        }

        private void Start()
        {
            if (buyButton != null)
            {
                buyButton.onClick.AddListener(OnBuyClicked);
            }
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Hide);
            }
        }

        private void OnDestroy()
        {
            if (buyButton != null)
            {
                buyButton.onClick.RemoveListener(OnBuyClicked);
            }
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Hide);
            }
        }

        private void Update()
        {
            if (popupPanel != null && popupPanel.activeSelf && _currentController != null)
            {
                TimeSpan remaining = _currentController.GetRemainingTime();
                if (remaining <= TimeSpan.Zero)
                {
                    if (timerText != null) timerText.text = "00:00:00";
                    if (buyButton != null) buyButton.interactable = false;
                    Hide();
                }
                else
                {
                    if (timerText != null)
                    {
                        timerText.text = string.Format("{0:D2}:{1:mm}:{2:ss}", 
                            (int)remaining.TotalHours, 
                            remaining, 
                            remaining);
                    }
                    if (buyButton != null) buyButton.interactable = true;
                }
            }
        }

        public void Show(OfferSO offer, TemporaryOfferController controller)
        {
            _currentOffer = offer;
            _currentController = controller;

            if (offer == null) return;

            if (titleText != null) titleText.text = offer.Title;
            
            if (priceText != null)
            {
                if (IAPManager.Instance != null && IAPManager.Instance.IsInitialized())
                {
                    priceText.text = IAPManager.Instance.GetLocalizedPrice(offer.ProductID);
                }
                else
                {
                    priceText.text = "N/A";
                }
            }

            if (popupPanel != null)
            {
                popupPanel.SetActive(true);
            }
        }

        public void Hide()
        {
            if (popupPanel != null)
            {
                popupPanel.SetActive(false);
            }
            _currentOffer = null;
            _currentController = null;
        }

        private void OnBuyClicked()
        {
            if (_currentOffer == null) return;

            if (IAPManager.Instance != null)
            {
                IAPManager.Instance.InitiatePurchase(_currentOffer.ProductID);
            }
            else
            {
                Debug.LogError("TemporaryOfferPopupUI: IAPManager.Instance is missing!");
            }
        }
    }
}