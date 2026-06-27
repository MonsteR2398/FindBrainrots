using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using Treasures.CurrencySystem;

namespace Treasures.IAP
{
    public enum OfferCategory
    {
        StandardCurrency,
        SpecialBundle,
        NoAds
    }

    [CreateAssetMenu(fileName = "NewOffer", menuName = "Treasures/IAP/Offer")]
    public class OfferSO : ScriptableObject
    {
        [Header("IAP Configuration")]
        [Tooltip("Matches Google Play Console Product ID")]
        [SerializeField] private string productID;
        [SerializeField] private ProductType productType = ProductType.Consumable;
        [SerializeField] private OfferCategory category = OfferCategory.StandardCurrency;

        [Header("Display Details")]
        [SerializeField] private string title;
        [SerializeField] private Sprite icon;
        [SerializeField] private string defaultPriceString = "$1.99";

        [Header("Rewards")]
        [SerializeField] private List<CurrencyValue> currencyRewards = new List<CurrencyValue>();
        //[SerializeField] private List<string> skinUnlockIDs = new List<string>();
        [SerializeField] private bool removeAdsReward;

        [Header("Time Limits")]
        [SerializeField] private bool isTemporary;
        [SerializeField] private float durationHours = 24f;

        // Public getters
        public string ProductID => productID;
        public ProductType ProductType => productType;
        public OfferCategory Category => category;
        public string Title => title;
        public Sprite Icon => icon;
        public string DefaultPriceString => defaultPriceString;
        public List<CurrencyValue> CurrencyRewards => currencyRewards;
        //public List<string> SkinUnlockIDs => skinUnlockIDs;
        public bool RemoveAdsReward => removeAdsReward;
        public bool IsTemporary => isTemporary;
        public float DurationHours => durationHours;
    }
}