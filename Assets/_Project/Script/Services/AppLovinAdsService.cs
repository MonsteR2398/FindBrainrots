using System;
using UnityEngine;

namespace Treasures.Services
{
    /// <summary>
    /// AppLovin MAX implementation of <see cref="IAdsService"/>.
    /// </summary>
    public class AppLovinAdsService : IAdsService
    {
        // TODO: Replace with your actual Ad Unit IDs from AppLovin dashboard
        private const string InterstitialAdUnitId = "8c09ebe742c77325";
        private const string BannerAdUnitId = "07963619ee83b6a0";
        private const string RewardedAdUnitId = "295b278f68e8c76b";

        public bool IsInitialized { get; private set; }

#if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR
        private Action _onInitialized;
        private Action _onInterstitialClosed;
        private Action _onRewardedClosed;
        private Action _onRewardedReceived;
        
        private int _interstitialRetryAttempt;
        private int _rewardedRetryAttempt;

        public void Initialize(Action onInitialized)
        {
            if (IsInitialized)
            {
                onInitialized?.Invoke();
                return;
            }

            _onInitialized = onInitialized;

            MaxSdkCallbacks.OnSdkInitializedEvent += OnSdkInitialized;
            
            // Interstitial callbacks
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoaded;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialLoadFailed;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHidden;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialDisplayFailed;

            // Rewarded callbacks
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoaded;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailed;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHidden;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdDisplayFailed;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedReward;

            MaxSdk.InitializeSdk();
            Debug.Log("[Ads] AppLovin MAX initialization requested.");
        }

        private void OnSdkInitialized(MaxSdkBase.SdkConfiguration configuration)
        {
            MaxSdkCallbacks.OnSdkInitializedEvent -= OnSdkInitialized;
            IsInitialized = true;

            Debug.Log("[Ads] AppLovin MAX initialized.");

            LoadInterstitial();
            LoadRewardedAd();

            var callback = _onInitialized;
            _onInitialized = null;
            callback?.Invoke();
        }

        #region Banners

        public void ShowBanner()
        {
            if (!IsInitialized) return;
            MaxSdk.CreateBanner(BannerAdUnitId, MaxSdkBase.BannerPosition.BottomCenter);
            MaxSdk.SetBannerBackgroundColor(BannerAdUnitId, Color.black);
            MaxSdk.ShowBanner(BannerAdUnitId);
            Debug.Log("[Ads] ShowBanner (Simulated in Editor)");
        }

        public void HideBanner()
        {
            if (!IsInitialized) return;
#if !UNITY_EDITOR
            MaxSdk.HideBanner(BannerAdUnitId);
#else
            Debug.Log("[Ads] HideBanner (Simulated in Editor)");
#endif
        }

        #endregion

        #region Interstitials

        public bool IsInterstitialReady()
        {
            return IsInitialized && MaxSdk.IsInterstitialReady(InterstitialAdUnitId);
        }

        public void LoadInterstitial()
        {
            if (!IsInitialized) return;
            Debug.Log("[Ads] Loading Interstitial...");
            MaxSdk.LoadInterstitial(InterstitialAdUnitId);
        }

        public void ShowInterstitial(Action onClosed = null)
        {
            if (IsInterstitialReady())
            {
                _onInterstitialClosed = onClosed;
                MaxSdk.ShowInterstitial(InterstitialAdUnitId);
            }
            else
            {
                Debug.LogWarning("[Ads] Interstitial not ready!");
                LoadInterstitial();
                onClosed?.Invoke();
            }
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
            Debug.Log("[Ads] Interstitial closed.");
            LoadInterstitial(); // Preload next
            
            var callback = _onInterstitialClosed;
            _onInterstitialClosed = null;
            callback?.Invoke();
        }

        private void OnInterstitialDisplayFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            Debug.LogError("[Ads] Interstitial display failed: " + errorInfo.Message);
            LoadInterstitial();
            
            var callback = _onInterstitialClosed;
            _onInterstitialClosed = null;
            callback?.Invoke();
        }

        #endregion

        #region Rewarded Ads

        public bool IsRewardedAdReady()
        {
            return IsInitialized && MaxSdk.IsRewardedAdReady(RewardedAdUnitId);
        }

        public void LoadRewardedAd()
        {
            if (!IsInitialized) return;
            Debug.Log("[Ads] Loading Rewarded Ad...");
            MaxSdk.LoadRewardedAd(RewardedAdUnitId);
        }

        public void ShowRewardedAd(Action onRewarded, Action onClosed = null)
        {
            if (IsRewardedAdReady())
            {
                _onRewardedReceived = onRewarded;
                _onRewardedClosed = onClosed;
                MaxSdk.ShowRewardedAd(RewardedAdUnitId);
            }
            else
            {
                Debug.LogWarning("[Ads] Rewarded ad not ready!");
                LoadRewardedAd();
                onClosed?.Invoke();
            }
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
            Debug.Log("[Ads] Rewarded ad closed.");
            LoadRewardedAd();
            
            var callback = _onRewardedClosed;
            _onRewardedClosed = null;
            callback?.Invoke();
        }

        private void OnRewardedAdDisplayFailed(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            Debug.LogError("[Ads] Rewarded ad display failed: " + errorInfo.Message);
            LoadRewardedAd();
            
            var callback = _onRewardedClosed;
            _onRewardedClosed = null;
            callback?.Invoke();
        }

        private void OnRewardedAdReceivedReward(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("[Ads] Rewarded ad received reward.");
            var callback = _onRewardedReceived;
            _onRewardedReceived = null;
            callback?.Invoke();
        }

        #endregion

#else
        public void Initialize(Action onInitialized) => onInitialized?.Invoke();
        public void ShowBanner() { }
        public void HideBanner() { }
        public bool IsInterstitialReady() => false;
        public void LoadInterstitial() { }
        public void ShowInterstitial(Action onClosed = null) => onClosed?.Invoke();
        public bool IsRewardedAdReady() => false;
        public void LoadRewardedAd() { }
        public void ShowRewardedAd(Action onRewarded, Action onClosed = null) => onClosed?.Invoke();
#endif
    }
}
