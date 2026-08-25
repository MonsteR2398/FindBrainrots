using System;
using UnityEngine;
using YG;

namespace Treasures.Services
{
    /// <summary>
    /// Yandex Games (YG2) implementation of <see cref="IAdsService"/> for WebGL builds.
    ///
    /// Wraps the YG2 SDK advertising modules. Those optional modules are detected through their
    /// scripting define symbols (<c>InterstitialAdv_yg</c>, <c>RewardedAdv_yg</c>, <c>StickyAdv_yg</c>)
    /// so this file still compiles before the modules are installed. Without a module the matching
    /// primitive degrades to a no-op / not-ready.
    ///
    /// Rewarded ads on Yandex require a placement (block) ID configured in <see cref="RewardedAdId"/>.
    /// </summary>
    public class YandexAdsService : AdsServiceBase
    {
        // TODO: Replace with your actual Yandex rewarded placement (block) ID.
        private const string RewardedAdId = "rewarded_block_id";

        protected override string PlatformName { get { return "Yandex Games"; } }

        #region Initialization

        protected override void PlatformBindEvents()
        {
            // IMPORTANT: only the CORE sdk event is bound here!
            // Module ad-events (onCloseInterAdv, onRewardAdv, ...) are created by the modules'
            // [InitYG] step which may run AFTER this service is constructed. Subscribing to a
            // not-yet-created event throws NRE and breaks the whole initialization chain -
            // that is why they are bound later, in OnSdkInitialized -> BindAdEventsOnce().
            YG2.onGetSDKData += OnSdkInitialized;

#if InterstitialAdv_yg
            Debug.Log("[Ads] YG2 module InterstitialAdv_yg: ACTIVE");
#else
            Debug.LogWarning("[Ads] YG2 module InterstitialAdv_yg: NOT active for this build target!");
#endif
#if RewardedAdv_yg
            Debug.Log("[Ads] YG2 module RewardedAdv_yg: ACTIVE");
#else
            Debug.LogWarning("[Ads] YG2 module RewardedAdv_yg: NOT active for this build target!");
#endif
        }

        protected override void PlatformInitialize()
        {
            try
            {
                // Idempotent: YG2 guards it internally (!_SDKEnabled). Covers the case when
                // YGSendMessage.Start never ran or SDK start was delayed for any reason.
                YG2.StartInit();

                // Safety net for the rare case when SDK data has already arrived.
                if (YG2.isSDKEnabled)
                    OnSdkInitialized();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Ads] YG2 early init attempt failed (SDK flow will retry): {e.Message}");
            }

#if UNITY_EDITOR
            ArmEditorWatchdog();
#endif
        }

        protected override void PlatformUnbindEvents()
        {
            YG2.onGetSDKData -= OnSdkInitialized;
            UnbindAdEvents();
        }

        private bool _adEventsBound;

        private void OnSdkInitialized()
        {
            Debug.Log("[Ads] YG2 onGetSDKData received.");
            YG2.onGetSDKData -= OnSdkInitialized;

            // By this moment YG2 finished AwakeInit -> all module events exist. Safe to bind.
            BindAdEventsOnce();

            if (IsInitialized)
                return;
            NotifyInitialized();
        }

        private void BindAdEventsOnce()
        {
            if (_adEventsBound)
                return;

            try
            {
#if InterstitialAdv_yg
                YG2.onCloseInterAdv += OnInterstitialHidden;
                YG2.onErrorInterAdv += OnInterstitialError;
#endif
#if RewardedAdv_yg
                YG2.onOpenRewardedAdv += OnRewardedOpened;
                YG2.onCloseRewardedAdv += OnRewardedClosed;
                YG2.onRewardAdv += OnRewardedReceived;
                YG2.onErrorRewardedAdv += OnRewardedError;
#endif
                _adEventsBound = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Ads] Failed to bind YG2 ad events: {e}");
            }
        }

        private void UnbindAdEvents()
        {
            if (!_adEventsBound)
                return;
            _adEventsBound = false;

#if InterstitialAdv_yg
            YG2.onCloseInterAdv -= OnInterstitialHidden;
            YG2.onErrorInterAdv -= OnInterstitialError;
#endif
#if RewardedAdv_yg
            YG2.onOpenRewardedAdv -= OnRewardedOpened;
            YG2.onCloseRewardedAdv -= OnRewardedClosed;
            YG2.onRewardAdv -= OnRewardedReceived;
            YG2.onErrorRewardedAdv -= OnRewardedError;
#endif
        }

        /// <summary>
        /// Self-healing: if the YG2 SDK is already enabled but our onGetSDKData handler was
        /// missed (editor init-order quirk), finish initialization right now, on demand.
        /// </summary>
        private void SelfHealInitialization()
        {
            if (!IsInitialized && YG2.isSDKEnabled)
            {
                Debug.Log("[Ads] SDK is enabled but init event was missed - self-healing.");
                OnSdkInitialized();
            }
        }

#if UNITY_EDITOR
        private YandexAdsInitWatchdog _watchdog;

        private void ArmEditorWatchdog()
        {
            if (_watchdog == null)
            {
                var go = new GameObject("[YandexAds Init Watchdog]");
                UnityEngine.Object.DontDestroyOnLoad(go);
                _watchdog = go.AddComponent<YandexAdsInitWatchdog>();
            }
            _watchdog.Arm(this);
        }

        internal bool IsReadyForWatchdog
        {
            get { return IsInitialized; }
        }

        /// <summary>Called by the watchdog when the normal onGetSDKData path did not work.</summary>
        internal void ForceInitializeFromWatchdog()
        {
            Debug.LogWarning("[Ads][WATCHDOG] Forcing initialization via OnSdkInitialized...");
            OnSdkInitialized();
        }
#endif

        #endregion

        #region Banners

        protected override void PlatformShowBanner()
        {
#if StickyAdv_yg
            YG2.StickyAdActivity(true);
#else
            Debug.Log("[Ads] Yandex sticky banner unavailable (StickyAdv module not installed).");
#endif
        }

        protected override void PlatformHideBanner()
        {
#if StickyAdv_yg
            YG2.StickyAdActivity(false);
#else
            Debug.Log("[Ads] Yandex sticky banner unavailable (StickyAdv module not installed).");
#endif
        }

        #endregion

        #region Interstitials

        protected override bool PlatformIsInterstitialReady()
        {
#if InterstitialAdv_yg
            SelfHealInitialization();
            return !IsShowingInterstitial;
#else
            return false;
#endif
        }

        protected override void PlatformLoadInterstitial()
        {
#if InterstitialAdv_yg
            YG2.optionalPlatform.LoadInterAdv();
#else
            Debug.LogWarning("[Ads] InterstitialAdv_yg module is not installed - enable it in PluginYG2.");
#endif
        }

        protected override void PlatformShowInterstitial()
        {
            // In the Editor YG2 plays its own ad simulation; on WebGL - a real fullscreen ad.
#if InterstitialAdv_yg
            YG2.InterstitialAdvShow();
#else
            Debug.LogWarning("[Ads] InterstitialAdv_yg module is not installed - cannot show interstitial.");
#endif
        }

#if InterstitialAdv_yg
        private void OnInterstitialHidden() => NotifyInterstitialClosed();
        private void OnInterstitialError() => NotifyInterstitialDisplayFailed("yg2 interstitial error");
#endif

        #endregion

        #region Rewarded Ads

        protected override bool PlatformIsRewardedReady()
        {
#if RewardedAdv_yg
            SelfHealInitialization();
            return !IsShowingRewarded;
#else
            return false;
#endif
        }

        protected override void PlatformLoadRewarded()
        {
#if RewardedAdv_yg
            YG2.optionalPlatform.LoadRewardedAdv();
#else
            Debug.LogWarning("[Ads] RewardedAdv_yg module is not installed - enable it in PluginYG2.");
#endif
        }

        protected override void PlatformShowRewarded()
        {
            // In the Editor YG2 plays its own ad simulation; on WebGL - a real rewarded video.
#if RewardedAdv_yg
            Debug.Log($"[Ads] Calling YG2.RewardedAdvShow('{RewardedAdId}')...");
            YG2.RewardedAdvShow(RewardedAdId);
#else
            Debug.LogWarning("[Ads] RewardedAdv_yg module is not installed - cannot show rewarded ad.");
#endif
        }

#if RewardedAdv_yg
        private void OnRewardedOpened() { }
        private void OnRewardedClosed() => NotifyRewardedClosed();
        private void OnRewardedReceived(string adUnitId) => NotifyRewardedReceived();
        private void OnRewardedError() => NotifyRewardedDisplayFailed("yg2 rewarded error");
#endif

        #endregion
    }

#if UNITY_EDITOR
    /// <summary>
    /// Editor-only watchdog: polls YG2 state and force-initializes the ads service as soon as
    /// the SDK becomes enabled, in case the onGetSDKData event was missed. If initialization
    /// still fails after the timeout - dumps full YG2 state to the console.
    /// </summary>
    internal sealed class YandexAdsInitWatchdog : MonoBehaviour
    {
        private const float PollIntervalSeconds = 0.5f;
        private const float GiveUpAfterSeconds = 20f;

        private YandexAdsService _service;
        private float _deadline;
        private bool _healed;
        private bool _armed;

        public void Arm(YandexAdsService service)
        {
            _service = service;
            _deadline = Time.realtimeSinceStartup + GiveUpAfterSeconds;
            _armed = true;
            CancelInvoke(nameof(Poll));
            InvokeRepeating(nameof(Poll), PollIntervalSeconds, PollIntervalSeconds);
        }

        private void Poll()
        {
            if (!_armed)
                return;

            // 1) Service already initialized by the normal path -> done.
            if (_service == null || _service.IsReadyForWatchdog)
            {
                Finish();
                return;
            }

            try
            {
                // 2) SDK is up but our init was missed -> catch up immediately.
                if (YG2.isSDKEnabled && !_healed)
                {
                    _healed = true;
                    Debug.LogWarning("[Ads][WATCHDOG] SDK готов, но событие инициализации было пропущено - догоняю.");
                    _service.ForceInitializeFromWatchdog();
                    return; // Next poll will confirm and finish.
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[Ads][WATCHDOG] Ошибка чтения состояния YG2: " + e.Message);
            }

            // 3) Still nothing after the timeout -> dump state for diagnostics.
            if (Time.realtimeSinceStartup >= _deadline)
            {
                string state;
                try
                {
                    state = $"isSDKEnabled={YG2.isSDKEnabled}, platform={YG2.platform}, " +
                            $"infoYG={(YG2.infoYG != null ? "ok" : "NULL")}, " +
                            $"syncInitSDK={YG2.infoYG.Basic.syncInitSDK}, " +
                            $"sendMessage={(YG2.sendMessage != null ? "alive" : "NULL")}, " +
                            $"nowInterAdv={YG2.nowInterAdv}, nowRewardAdv={YG2.nowRewardAdv}";
                }
                catch (Exception e)
                {
                    state = "state read failed: " + e.Message;
                }

                Debug.LogError(
                    "[Ads][WATCHDOG] Сервис рекламы не инициализировался за " +
                    $"{GiveUpAfterSeconds:0} сек! Состояние YG2: " + state);

                Finish();
            }
        }

        private void Finish()
        {
            _armed = false;
            CancelInvoke(nameof(Poll));
        }
    }
#endif
} 