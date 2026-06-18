using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;
using Treasures.CurrencySystem;
using ModularSkinShop.Core;

namespace Treasures.IAP
{
    public class IAPManager : MonoBehaviour, IStoreListener
    {
        public static IAPManager Instance { get; private set; }

        [Header("Catalog Reference")]
        [SerializeField] private OfferCatalogSO catalog;

        private IStoreController m_StoreController;
        private IExtensionProvider m_StoreExtensionProvider;

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

        private void Start()
        {
            InitializePurchasing();
        }

        private void InitializePurchasing()
        {
            if (IsInitialized()) return;

            if (catalog == null)
            {
                Debug.LogError("IAPManager: Offer catalog is not assigned!");
                return;
            }

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            foreach (var offer in catalog.Offers)
            {
                builder.AddProduct(offer.ProductID, offer.ProductType);
            }

            UnityPurchasing.Initialize(this, builder);
        }

        public bool IsInitialized()
        {
            return m_StoreController != null && m_StoreExtensionProvider != null;
        }

        public void InitiatePurchase(string productId)
        {
            if (IsInitialized())
            {
                Product product = m_StoreController.products.WithID(productId);
                if (product != null && product.availableToPurchase)
                {
                    Debug.Log($"IAPManager: Buying product asynchronously: '{product.definition.id}'");
                    m_StoreController.InitiatePurchase(product);
                }
                else
                {
                    Debug.LogError("IAPManager: BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase.");
                    OnPurchaseFailedEvent?.Invoke(productId, "Product not found or unavailable");
                }
            }
            else
            {
                Debug.LogError("IAPManager: BuyProductID FAIL. Not initialized.");
                OnPurchaseFailedEvent?.Invoke(productId, "IAP System Not Initialized");
            }
        }

        public string GetLocalizedPrice(string productId, string defaultPrice)
        {
            if (IsInitialized())
            {
                Product product = m_StoreController.products.WithID(productId);
                if (product != null && product.metadata != null && !string.IsNullOrEmpty(product.metadata.localizedPriceString))
                {
                    return product.metadata.localizedPriceString;
                }
            }
            return defaultPrice;
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

            // 1. Grant currency rewards
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

            // // 2. Grant skin unlock rewards
            // if (offer.SkinUnlockIDs != null)
            // {
            //     SkinShopManager skinShop = FindFirstObjectByType<SkinShopManager>();
            //     foreach (var skinID in offer.SkinUnlockIDs)
            //     {
            //         if (skinShop != null)
            //         {
            //             skinShop.Unlock(skinID);
            //             Debug.Log($"IAPManager: Skin {skinID} unlocked via SkinShopManager.");
            //         }
            //         else
            //         {
            //             // Backup to PlayerPrefs
            //             PlayerPrefs.SetInt("SkinUnlocked_" + skinID, 1);
            //             PlayerPrefs.Save();
            //             Debug.LogWarning($"IAPManager: SkinShopManager not found in scene. Unlocked skin {skinID} in PlayerPrefs as backup.");
            //         }
            //     }
            // }

            // 3. Remove Ads reward
            if (offer.RemoveAdsReward)
            {
                PlayerPrefs.SetInt("NoAdsPurchased", 1);
                PlayerPrefs.Save();
                Debug.Log("IAPManager: Granted Remove Ads reward.");
            }
        }

        private bool ValidateReceipt(string receipt)
        {
            #if UNITY_EDITOR
            return true;
            #else
            try
            {
                var googlePlayTangleType = System.Type.GetType("UnityEngine.Purchasing.Security.GooglePlayTangle, UnityEngine.Purchasing.Security")
                                           ?? System.Type.GetType("UnityEngine.Purchasing.Security.GooglePlayTangle, Assembly-CSharp-firstpass")
                                           ?? System.Type.GetType("UnityEngine.Purchasing.Security.GooglePlayTangle, Assembly-CSharp");
                                           
                var appleTangleType = System.Type.GetType("UnityEngine.Purchasing.Security.AppleTangle, UnityEngine.Purchasing.Security")
                                      ?? System.Type.GetType("UnityEngine.Purchasing.Security.AppleTangle, Assembly-CSharp-firstpass")
                                      ?? System.Type.GetType("UnityEngine.Purchasing.Security.AppleTangle, Assembly-CSharp");

                if (googlePlayTangleType != null && appleTangleType != null)
                {
                    var googleDataMethod = googlePlayTangleType.GetMethod("Data", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    var appleDataMethod = appleTangleType.GetMethod("Data", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                    if (googleDataMethod != null && appleDataMethod != null)
                    {
                        byte[] googlePlayTangleData = (byte[])googleDataMethod.Invoke(null, null);
                        byte[] appleTangleData = (byte[])appleDataMethod.Invoke(null, null);

                        var validator = new CrossPlatformValidator(
                            googlePlayTangleData,
                            appleTangleData,
                            Application.identifier
                        );

                        var result = validator.Validate(receipt);
                        return result != null && result.Length > 0;
                    }
                }
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Local receipt validation failed: {ex.Message}. Falling back to true.");
                return true;
            }
            #endif
        }

        // --- IStoreListener Interface Implementation ---

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            m_StoreController = controller;
            m_StoreExtensionProvider = extensions;
            Debug.Log("IAPManager: Purchasing initialized successfully.");
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogError($"IAPManager: Purchasing initialization failed: {error}");
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.LogError($"IAPManager: Purchasing initialization failed: {error}. Message: {message}");
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            string productId = args.purchasedProduct.definition.id;

            if (!ValidateReceipt(args.purchasedProduct.receipt))
            {
                Debug.LogError($"IAPManager: Receipt validation failed for product {productId}");
                return PurchaseProcessingResult.Complete;
            }

            try
            {
                GrantRewards(productId);
                OnPurchaseSuccess?.Invoke(productId);
            }
            catch (Exception ex)
            {
                Debug.LogError($"IAPManager: Error granting rewards for product {productId}: {ex.Message}");
            }

            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Debug.LogError($"IAPManager: Purchase failed for {product.definition.id}. Reason: {failureReason}");
            OnPurchaseFailedEvent?.Invoke(product.definition.id, failureReason.ToString());
        }
    }
}