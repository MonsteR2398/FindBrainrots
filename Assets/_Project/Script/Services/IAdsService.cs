using System;

namespace Treasures.Services
{
    public interface IAdsService
    {
        bool IsInitialized { get; }
        void Initialize(Action onInitialized);

        // Banners
        void ShowBanner();
        void HideBanner();

        // Interstitials
        bool IsInterstitialReady();
        void LoadInterstitial();
        void ShowInterstitial(Action onClosed = null);

        // Rewarded Ads
        bool IsRewardedAdReady();
        void LoadRewardedAd();
        void ShowRewardedAd(Action onRewarded, Action onClosed = null);
    }
}
