using System;
using System.Collections.Generic;
using UnityEngine;
using YG;
using Treasures.CurrencySystem;
using Treasures.Boosts;
using Treasures.Services;
using ModularSkinShop.Core;

namespace Treasures.IAP
{
    public class IAPManager : MonoBehaviour
    {
        public static IAPManager Instance { get; private set; }

        private const string RemoveAdsProductId = "removead";

        public static bool IsAdsRemoved => Instance != null && Instance.IsRemoveAdsOwned();

        [Header("Catalog Reference")]
        [SerializeField] private OfferCatalogSO catalog;

        private bool _purchasesLoaded;

        public event Action<string> OnPurchaseSuccess;
        public event Action<string, string> OnPurchaseFailedEvent;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            YG2.onGetPayments += OnGetPayments;
            YG2.onPurchaseSuccess += OnPurchaseSuccessCallback;
            YG2.onPurchaseFailed += OnPurchaseFailedCallback;
        }

        private void OnDisable()
        {
            YG2.onGetPayments -= OnGetPayments;
            YG2.onPurchaseSuccess -= OnPurchaseSuccessCallback;
            YG2.onPurchaseFailed -= OnPurchaseFailedCallback;
        }

        private void Start()
        {
            if (YG2.purchases?.Length > 0)
                _purchasesLoaded = true;

            if (!_purchasesLoaded && YG2.isSDKEnabled)
                Debug.Log("IAPManager: Waiting for YG2 purchases to load (onGetPayments)...");
        }

        private void OnGetPayments()
        {
            _purchasesLoaded = true;

            foreach (var offer in catalog?.Offers ?? new List<OfferSO>())
            {
                if (offer == null) continue;
                Debug.Log($"IAPManager: Registered YG2 product '{offer.ProductID}' ({offer.name}) price = '{GetLocalizedPrice(offer.ProductID)}'");
            }
        }

        public bool IsInitialized()
        {
            return _purchasesLoaded;
        }

        public void InitiatePurchase(string productId)
        {
            if (!YG2.isSDKEnabled)
            {
                Debug.LogError($"IAPManager: BuyProductID FAIL. YG2 SDK is not enabled. Product: '{productId}'");
                OnPurchaseFailedEvent?.Invoke(productId, "IAP System Not Initialized");
                return;
            }

            if (!_purchasesLoaded)
            {
                Debug.LogWarning($"IAPManager: Purchases not loaded yet. Attempting '{productId}' anyway.");
            }

            if (YG2.PurchaseByID(productId) == null)
            {
                Debug.LogError($"IAPManager: BuyProductID FAIL. Product with ID '{productId}' is not in the YG2 catalog. Available: {ListRegisteredProducts()}");
                OnPurchaseFailedEvent?.Invoke(productId, "Product not found or unavailable");
                return;
            }

            Debug.Log($"IAPManager: Buying product '{productId}' via YG2.");
            YG2.BuyPayments(productId);
        }

        private string ListRegisteredProducts()
        {
            if (YG2.purchases == null) return "none";
            var ids = new List<string>();
            foreach (var p in YG2.purchases)
                ids.Add(p.id);
            return string.Join(", ", ids);
        }

        public string GetLocalizedPrice(string productId)
        {
            var purchase = YG2.PurchaseByID(productId);
            if (purchase != null && !string.IsNullOrEmpty(purchase.price))
                return purchase.price;
            return "N/A";
        }

        private bool IsRemoveAdsOwned()
        {
            var purchase = YG2.PurchaseByID(RemoveAdsProductId);
            return purchase != null && purchase.consumed;
        }

        public void GrantRewards(string productId)
        {
            if (catalog == null)
            {
                Debug.LogError("IAPManager: Catalog is null during GrantRewards!");
                return;
            }

            OfferSO offer = catalog.Offers.Find(o => o.ProductID == productId);
            if (offer == null)
            {
                Debug.LogError($"IAPManager: Offer with ID {productId} not found in catalog!");
                return;
            }

            if (offer.CurrencyRewards != null)
            {
                foreach (var currencyReward in offer.CurrencyRewards)
                {
                    if (CurrencyService.Instance != null)
                    {
                        CurrencyService.Instance.AddBalance(currencyReward.Type, currencyReward.Value);
                        Debug.Log($"IAPManager: Granted currency {currencyReward.Type} x {currencyReward.Value}");
                    }
                    else
                    {
                        Debug.LogError("IAPManager: CurrencyService instance is missing!");
                    }
                }
            }

            if (offer.BoostRewards != null)
            {
                foreach (var boosterReward in offer.BoostRewards)
                {
                    if (BoostController.Instance != null)
                    {
                        BoostController.Instance.AddStock(boosterReward.Type, boosterReward.Value);
                        Debug.Log($"IAPManager: Granted boost {boosterReward.Type} x {boosterReward.Value}");
                    }
                    else
                    {
                        Debug.LogError("IAPManager: BoostController instance is missing!");
                    }
                }
            }

            if (productId == RemoveAdsProductId)
            {
                Debug.Log("IAPManager: Granted Remove Ads reward (tracked via YG2 purchase state).");

                if (AppServices.Ads != null)
                    AppServices.Ads.HideBanner();
            }
            else if (offer.RemoveAdsReward)
            {
                Debug.Log($"IAPManager: Granting temporary ads suppression for offer '{productId}'.");
                if (AppServices.Ads != null)
                    AppServices.Ads.HideBanner();
            }
        }

        private void OnPurchaseSuccessCallback(string productId)
        {
            try
            {
                GrantRewards(productId);
            }
            catch (Exception ex)
            {
                Debug.LogError($"IAPManager: Error granting rewards for product {productId}: {ex.Message}");
                OnPurchaseFailedEvent?.Invoke(productId, $"Grant error: {ex.Message}");
                return;
            }

            OnPurchaseSuccess?.Invoke(productId);
        }

        private void OnPurchaseFailedCallback(string productId)
        {
            Debug.LogError($"IAPManager: Purchase failed for {productId}.");
            OnPurchaseFailedEvent?.Invoke(productId, "Purchase failed");
        }
    }
}