using System;
using UnityEngine;

namespace Treasures.Services
{
    /// <summary>
    /// Shared base implementation of <see cref="IAdsService"/> used by both AppLovin MAX (mobile)
    /// and Yandex Games (WebGL) ad services.
    ///
    /// It owns the platform-agnostic orchestration (initialization callback, interstitial and
    /// rewarded state machines, on-not-ready fallbacks) while delegating the actual SDK calls to
    /// the abstract primitives implemented by each platform.
    /// </summary>
    public abstract class AdsServiceBase : IAdsService
    {
        /// <summary>Display name used in logs, e.g. "AppLovin MAX" or "Yandex Games".</summary>
        protected abstract string PlatformName { get; }

        /// <summary>True once the underlying SDK finished initializing.</summary>
        public bool IsInitialized { get; private set; }

        private Action _onInitialized;
        private Action _onInterstitialClosed;
        private Action _onRewardedClosed;
        private Action _onRewardedReceived;

        private bool _isShowingInterstitial;
        private bool _isShowingRewarded;

        /// <summary>True while an interstitial is on screen (blocks showing another one).</summary>
        protected bool IsShowingInterstitial => _isShowingInterstitial;

        /// <summary>True while a rewarded ad is on screen (blocks showing another one).</summary>
        protected bool IsShowingRewarded => _isShowingRewarded;

        #region Initialization

        public void Initialize(Action onInitialized)
        {
            if (IsInitialized)
            {
                onInitialized?.Invoke();
                return;
            }

            _onInitialized = onInitialized;

            PlatformBindEvents();
            PlatformInitialize();
            Debug.Log($"[Ads] {PlatformName} initialization requested.");
        }

        /// <summary>Called by the derived class from its SDK-init callback to finish initialization.</summary>
        protected void NotifyInitialized()
        {
            IsInitialized = true;
            Debug.Log($"[Ads] {PlatformName} initialized.");

            LoadInterstitial();
            LoadRewardedAd();

            var callback = _onInitialized;
            _onInitialized = null;
            callback?.Invoke();
        }

        /// <summary>Start the underlying SDK initialization.</summary>
        protected abstract void PlatformInitialize();

        /// <summary>Subscribe to the underlying SDK callbacks.</summary>
        protected abstract void PlatformBindEvents();

        /// <summary>Unsubscribe from the underlying SDK callbacks (optional).</summary>
        protected virtual void PlatformUnbindEvents() { }

        #endregion

        #region Banners

        public virtual void ShowBanner()
        {
            if (!IsInitialized) return;
            PlatformShowBanner();
        }

        public virtual void HideBanner()
        {
            if (!IsInitialized) return;
            PlatformHideBanner();
        }

        protected abstract void PlatformShowBanner();

        protected abstract void PlatformHideBanner();

        #endregion

        #region Interstitials

        public bool IsInterstitialReady()
        {
            bool ready = IsInitialized && !_isShowingInterstitial && PlatformIsInterstitialReady();
#if UNITY_EDITOR
            if (!ready)
                Debug.Log($"[Ads][Diag] IsInterstitialReady=false (initialized={IsInitialized}, showing={_isShowingInterstitial}, platformReady={PlatformIsInterstitialReady()})");
#endif
            return ready;
        }

        public void LoadInterstitial()
        {
            if (!IsInitialized) return;
            Debug.Log($"[Ads] Loading {PlatformName} interstitial...");
            PlatformLoadInterstitial();
        }

        public void ShowInterstitial(Action onClosed = null)
        {
            if (IsInterstitialReady())
            {
                _isShowingInterstitial = true;
                _onInterstitialClosed = onClosed;
                PlatformShowInterstitial();
            }
            else
            {
                Debug.LogWarning($"[Ads] {PlatformName} interstitial not ready!");
                LoadInterstitial();
                onClosed?.Invoke();
            }
        }

        /// <summary>Report to the base that an interstitial was closed (reloads the next one).</summary>
        protected void NotifyInterstitialClosed()
        {
            Debug.Log($"[Ads] {PlatformName} interstitial closed.");
            _isShowingInterstitial = false;
            LoadInterstitial(); // Preload next

            var callback = _onInterstitialClosed;
            _onInterstitialClosed = null;
            callback?.Invoke();
        }

        /// <summary>Report to the base that showing the interstitial failed (falls through to closed).</summary>
        protected void NotifyInterstitialDisplayFailed(string message)
        {
            Debug.LogError($"[Ads] {PlatformName} interstitial display failed: " + message);
            _isShowingInterstitial = false;
            LoadInterstitial();

            var callback = _onInterstitialClosed;
            _onInterstitialClosed = null;
            callback?.Invoke();
        }

        protected abstract bool PlatformIsInterstitialReady();

        protected abstract void PlatformLoadInterstitial();

        protected abstract void PlatformShowInterstitial();

        #endregion

        #region Rewarded Ads

        public bool IsRewardedAdReady()
        {
            bool ready = IsInitialized && !_isShowingRewarded && PlatformIsRewardedReady();
#if UNITY_EDITOR
            if (!ready)
                Debug.Log($"[Ads][Diag] IsRewardedAdReady=false (initialized={IsInitialized}, showing={_isShowingRewarded}, platformReady={PlatformIsRewardedReady()})");
#endif
            return ready;
        }

        public void LoadRewardedAd()
        {
            if (!IsInitialized) return;
            Debug.Log($"[Ads] Loading {PlatformName} rewarded ad...");
            PlatformLoadRewarded();
        }

        public void ShowRewardedAd(Action onRewarded, Action onClosed = null)
        {
            if (IsRewardedAdReady())
            {
                _isShowingRewarded = true;
                _onRewardedReceived = onRewarded;
                _onRewardedClosed = onClosed;
                PlatformShowRewarded();
            }
            else
            {
                Debug.LogWarning($"[Ads] {PlatformName} rewarded ad not ready!");
                LoadRewardedAd();
                onClosed?.Invoke();
            }
        }

        /// <summary>Report to the base that a reward should be granted.</summary>
        protected void NotifyRewardedReceived()
        {
            Debug.Log($"[Ads] {PlatformName} rewarded ad received reward.");
            var callback = _onRewardedReceived;
            _onRewardedReceived = null;
            callback?.Invoke();
        }

        /// <summary>Report to the base that a rewarded ad was closed (does NOT grant a reward).</summary>
        protected void NotifyRewardedClosed()
        {
            Debug.Log($"[Ads] {PlatformName} rewarded ad closed.");
            _isShowingRewarded = false;
            LoadRewardedAd();

            var callback = _onRewardedClosed;
            _onRewardedClosed = null;
            callback?.Invoke();
        }

        /// <summary>Report to the base that showing the rewarded ad failed (falls through to closed).</summary>
        protected void NotifyRewardedDisplayFailed(string message)
        {
            Debug.LogError($"[Ads] {PlatformName} rewarded ad display failed: " + message);
            _isShowingRewarded = false;
            LoadRewardedAd();

            var callback = _onRewardedClosed;
            _onRewardedClosed = null;
            callback?.Invoke();
        }

        protected abstract bool PlatformIsRewardedReady();

        protected abstract void PlatformLoadRewarded();

        protected abstract void PlatformShowRewarded();

        #endregion
    }
}