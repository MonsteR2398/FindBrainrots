using UnityEngine;
using UnityEngine.UI;
using Treasures.CurrencySystem;
using Treasures.Services;

namespace Treasures.UI
{
    /// <summary>
    /// Hook this on a uGUI Button. On click it shows a rewarded ad.
    /// On successful reward, grants the configured currency amount.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class AdRewardButton : MonoBehaviour
    {
        [SerializeField] private CurrencyType currencyType = CurrencyType.Gold;
        [SerializeField] private long rewardAmount = 100;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            Debug.Log($"[AdReward] Showing rewarded ad for {rewardAmount} {currencyType}...");
            var ads = AppServices.Ads;
            if (ads != null && ads.IsRewardedAdReady())
            {
                ads.ShowRewardedAd(
                    onRewarded: () =>
                    {
                        if (CurrencyService.Instance != null)
                        {
                            CurrencyService.Instance.AddBalance(currencyType, rewardAmount);
                            Debug.Log($"[AdReward] Reward granted: {rewardAmount} {currencyType}");
                        }
                        else
                        {
                            Debug.LogError("[AdReward] CurrencyService instance is missing!");
                        }
                    },
                    onClosed: null);
            }
            else
            {
                Debug.LogWarning("[AdReward] Rewarded ad not ready, loading...");
                ads?.LoadRewardedAd();
            }
        }
    }
}