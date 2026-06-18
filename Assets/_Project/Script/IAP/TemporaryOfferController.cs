using System;
using UnityEngine;

namespace Treasures.IAP
{
    public class TemporaryOfferController : MonoBehaviour
    {
        [SerializeField] private OfferSO offer;

        public OfferSO Offer => offer;

        private string StartTimeKey => "OfferStart_" + (offer != null ? offer.ProductID : "unknown");
        private string PurchasedKey => "OfferPurchased_" + (offer != null ? offer.ProductID : "unknown");

        private void OnEnable()
        {
            if (IAPManager.Instance != null)
            {
                IAPManager.Instance.OnPurchaseSuccess += HandlePurchaseSuccess;
            }
        }

        private void OnDisable()
        {
            if (IAPManager.Instance != null)
            {
                IAPManager.Instance.OnPurchaseSuccess -= HandlePurchaseSuccess;
            }
        }

        private void Start()
        {
            if (IAPManager.Instance != null)
            {
                IAPManager.Instance.OnPurchaseSuccess -= HandlePurchaseSuccess;
                IAPManager.Instance.OnPurchaseSuccess += HandlePurchaseSuccess;
            }
            TriggerOffer();
        }

        private void HandlePurchaseSuccess(string productId)
        {
            if (offer != null && productId == offer.ProductID)
            {
                MarkAsPurchased();
            }
        }

        public bool IsOfferActive()
        {
            if (offer == null || !offer.IsTemporary) return false;

            if (PlayerPrefs.GetInt(PurchasedKey, 0) == 1) return false;

            if (!PlayerPrefs.HasKey(StartTimeKey))
            {
                return true;
            }

            string savedTicksStr = PlayerPrefs.GetString(StartTimeKey);
            if (long.TryParse(savedTicksStr, out long startTicks))
            {
                DateTime startTime = new DateTime(startTicks, DateTimeKind.Utc);
                TimeSpan elapsed = DateTime.UtcNow - startTime;
                return elapsed.TotalHours < offer.DurationHours;
            }

            return false;
        }

        public void TriggerOffer()
        {
            if (offer == null) return;

            if (!PlayerPrefs.HasKey(StartTimeKey))
            {
                PlayerPrefs.SetString(StartTimeKey, DateTime.UtcNow.Ticks.ToString());
                PlayerPrefs.Save();
            }

            if (IsOfferActive())
            {
                if (TemporaryOfferPopupUI.Instance != null)
                {
                    TemporaryOfferPopupUI.Instance.Show(offer, this);
                }
                else
                {
                    Debug.LogWarning("TemporaryOfferController: TemporaryOfferPopupUI instance is missing in scene!");
                }
            }
        }

        public TimeSpan GetRemainingTime()
        {
            if (offer == null || !offer.IsTemporary) return TimeSpan.Zero;

            if (!PlayerPrefs.HasKey(StartTimeKey))
            {
                return TimeSpan.FromHours(offer.DurationHours);
            }

            string savedTicksStr = PlayerPrefs.GetString(StartTimeKey);
            if (long.TryParse(savedTicksStr, out long startTicks))
            {
                DateTime startTime = new DateTime(startTicks, DateTimeKind.Utc);
                TimeSpan elapsed = DateTime.UtcNow - startTime;
                double remainingHours = offer.DurationHours - elapsed.TotalHours;
                if (remainingHours <= 0)
                {
                    return TimeSpan.Zero;
                }
                return TimeSpan.FromHours(remainingHours);
            }

            return TimeSpan.Zero;
        }

        public void MarkAsPurchased()
        {
            if (offer == null) return;
            PlayerPrefs.SetInt(PurchasedKey, 1);
            PlayerPrefs.Save();
        }
    }
}