using System;
using UnityEngine;

namespace Treasures.Services
{
    /// <summary>
    /// AppLovin MAX implementation of <see cref="IAdsService"/> for mobile (Android/iOS) builds.
    /// </summary>
    public class AppLovinAdsService : AdsServiceBase
    {
        // TODO: Replace with your actual Ad Unit IDs from AppLovin dashboard
        private const string InterstitialAdUnitId = "8c09ebe742c77325";
        private const string BannerAdUnitId = "07963619ee83b6a0";
        private const string RewardedAdUnitId = "295b278f68e8c76b";

        protected override string PlatformName { get { return "AppLovin MAX"; } }

#if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR
        private int _interstitialRetryAttempt;
        private int _rewardedRetryAttempt;

        #region Initialization

        protected override void PlatformBindEvents()
        {
            MaxSdkCallbacks.OnSdkInitializedEvent += OnSdkInitialized;

            // Interstitial callbacks
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoaded;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialLoadFailed;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHidden;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialDisplayFailed;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;

            // Rewarded callbacks
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoaded;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailed;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHidden;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdDisplayFailed;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedReward;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;

            // Banner callbacks
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
        }

        protected override void PlatformInitialize()
        {
            MaxSdk.InitializeSdk();
        }

        private void OnSdkInitialized(MaxSdkBase.SdkConfiguration configuration)
        {
            MaxSdkCallbacks.OnSdkInitializedEvent -= OnSdkInitialized;
            NotifyInitialized();
        }

        #endregion

        #region Banners

        protected override void PlatformShowBanner()
        {
            MaxSdk.CreateBanner(BannerAdUnitId, MaxSdkBase.BannerPosition.BottomCenter);
            MaxSdk.SetBannerBackgroundColor(BannerAdUnitId, Color.black);
            MaxSdk.ShowBanner(BannerAdUnitId);
            Debug.Log("[Ads] ShowBanner (Simulated in Editor)");
        }

        protected override void PlatformHideBanner()
        {
#if !UNITY_EDITOR
            MaxSdk.HideBanner(BannerAdUnitId);
#else
            Debug.Log("[Ads] HideBanner (Simulated in Editor)");
#endif
        }

        #endregion

        #region Interstitials

        protected override bool PlatformIsInterstitialReady()
        {
            return !IsShowingInterstitial && MaxSdk.IsInterstitialReady(InterstitialAdUnitId);
        }

        protected override void PlatformLoadInterstitial()
        {
            Debug.Log("[Ads] Loading Interstitial...");
            MaxSdk.LoadInterstitial(InterstitialAdUnitId);
        }

        protected override void PlatformShowInterstitial()
        {
            MaxSdk.ShowInterstitial(InterstitialAdUnitId);
        }

        private void OnInterstitialLoaded(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _interstitialRetryAttempt = 0;
            Debug.Log("[Ads] Interstitial loaded.");
        }

        private void OnInterstitialLoadFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            _interstitialRetryAttempt++;
            Debug.LogWarning($"[Ads] Interstitial load failed: {errorInfo.Message}. Attempt: {_interstitialRetryAttempt}");
        }

        private void OnInterstitialHidden(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            NotifyInterstitialClosed();
        }

        private void OnInterstitialDisplayFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            NotifyInterstitialDisplayFailed(errorInfo.Message);
        }

        #endregion

        #region Rewarded Ads

        protected override bool PlatformIsRewardedReady()
        {
            return !IsShowingRewarded && MaxSdk.IsRewardedAdReady(RewardedAdUnitId);
        }

        protected override void PlatformLoadRewarded()
        {
            Debug.Log("[Ads] Loading Rewarded Ad...");
            MaxSdk.LoadRewardedAd(RewardedAdUnitId);
        }

        protected override void PlatformShowRewarded()
        {
            MaxSdk.ShowRewardedAd(RewardedAdUnitId);
        }

        private void OnRewardedAdLoaded(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            _rewardedRetryAttempt = 0;
            Debug.Log("[Ads] Rewarded ad loaded.");
        }

        private void OnRewardedAdLoadFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            _rewardedRetryAttempt++;
            Debug.LogWarning($"[Ads] Rewarded ad load failed: {errorInfo.Message}. Attempt: {_rewardedRetryAttempt}");
        }

        private void OnRewardedAdHidden(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            NotifyRewardedClosed();
        }

        private void OnRewardedAdDisplayFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            NotifyRewardedDisplayFailed(errorInfo.Message);
        }

        private void OnRewardedAdReceivedReward(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
        {
            NotifyRewardedReceived();
        }

        private void OnAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log($"[Ads] Ad revenue paid for {adUnitId}. Revenue: {adInfo.Revenue}");

            if (AppServices.Analytics != null && AppServices.Analytics.IsInitialized)
            {
                var parameters = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "ad_platform", "AppLovin" },
                    { "ad_source", adInfo.NetworkName },
                    { "ad_unit_name", adInfo.AdUnitIdentifier },
                    { "ad_format", adInfo.AdFormat },
                    { "value", adInfo.Revenue },
                    { "currency", "USD" }
                };

                AppServices.Analytics.LogEvent("ad_impression", parameters);
            }
        }

        #endregion
#else
        // Non-Android/iOS (and not Editor) targets: the base class still satisfies IAdsService,
        // but no MAX SDK calls are made. All Platform* primitives are no-ops / not-ready.
        protected override void PlatformBindEvents() { }
        protected override void PlatformInitialize() { }
        protected override void PlatformShowBanner() { }
        protected override void PlatformHideBanner() { }
        protected override bool PlatformIsInterstitialReady() => false;
        protected override void PlatformLoadInterstitial() { }
        protected override void PlatformShowInterstitial() { }
        protected override bool PlatformIsRewardedReady() => false;
        protected override void PlatformLoadRewarded() { }
        protected override void PlatformShowRewarded() { }
#endif
    }
}
