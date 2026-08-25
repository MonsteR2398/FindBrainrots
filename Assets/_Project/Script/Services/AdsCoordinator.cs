using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using YG;
using PlayerPrefs = RedefineYG.PlayerPrefs;
using L10n = Treasures.Localization.Localization;

namespace Treasures.Services
{
    /// <summary>
    /// Coordinates ad display logic based on Remote Config settings.
    /// Handles the interstitial timer, initial banner display and the pre-interstitial
    /// sequence: pause -> on-screen countdown (2,1,0) -> show ad -> unpause.
    /// </summary>
    public class AdsCoordinator : MonoBehaviour
    {
        private const string BannerEnabledKey = "banner_enabled";
        private const string InitialAdDelayKey = "initial_ad_delay_sec";
        private const string InterstitialIntervalKey = "interstitial_interval_sec";

        // Pre-ad countdown configuration ("2, 1, 0").
        private const int CountdownStartNumber = 2;
        private const float CountdownStepSeconds = 1f;
        private const float CountdownDimAlpha = 0.6f;
        private const int CountdownSortingOrder = 30000; // Above game UI, below YG2 ad overlay (32767).
        private const string CountdownKey = "ads.countdown"; // "Реклама через {0}.."

        private bool _isBannerShowing;
        private bool _isInterstitialShowing;
        private float _lastInterstitialTime;

        private GameObject _countdownRoot;
        private Text _countdownText;

        private IEnumerator Start()
        {
            while (!AppServices.AdsReady || !AppServices.FirebaseReady)
            {
                yield return new WaitForSeconds(1f);
            }

            Debug.Log("[AdsCoordinator] Services ready. Waiting for initial delay...");

            long initialDelay = AppServices.Analytics.GetLong(InitialAdDelayKey, 90);
            yield return new WaitForSeconds(initialDelay);

            ApplyBannerSettings();

            // Set last time so the first interstitial triggers immediately after the initial delay
            long interval = AppServices.Analytics.GetLong(InterstitialIntervalKey, 90);
            _lastInterstitialTime = Time.time - interval;
            StartCoroutine(InterstitialTimerLoop());
        }

        private void ApplyBannerSettings()
        {
            bool bannerEnabled = AppServices.Analytics.GetBool(BannerEnabledKey, true);
            if (bannerEnabled)
            {
                Debug.Log("[AdsCoordinator] Showing banner from Remote Config.");
                AppServices.Ads.ShowBanner();
                _isBannerShowing = true;
            }
            else
            {
                Debug.Log("[AdsCoordinator] Banner disabled in Remote Config.");
                AppServices.Ads.HideBanner();
                _isBannerShowing = false;
            }
        }

        private IEnumerator InterstitialTimerLoop()
        {
            while (true)
            {
                if (_isInterstitialShowing)
                {
                    yield return new WaitForSecondsRealtime(1f);
                    continue;
                }

                long interval = AppServices.Analytics.GetLong(InterstitialIntervalKey, 90);
                float timeSinceLastAd = Time.time - _lastInterstitialTime;

                if (timeSinceLastAd >= interval)
                {
                    if (CanShowInterstitialNow())
                    {
                        Debug.Log("[AdsCoordinator] Time's up! Starting pre-ad sequence.");
                        yield return StartCoroutine(PreAdSequence());
                        continue;
                    }
                    else
                    {
                        yield return new WaitForSeconds(5f);
                        continue;
                    }
                }

                yield return new WaitForSeconds(1f);
            }
        }

        /// <summary>
        /// True when everything is in place to actually show an interstitial right now:
        /// service ready, ad loaded, and YG2-side frequency guard passed.
        /// The last check matters - YG2.InterstitialAdvShow() silently does nothing when its
        /// internal timer has not completed, which would otherwise leave the game paused.
        /// </summary>
        private bool CanShowInterstitialNow()
        {
            var ads = AppServices.Ads;
            if (ads == null || !AppServices.AdsReady) return false;
            if (!ads.IsInterstitialReady()) return false;

#if InterstitialAdv_yg
            if (YG2.nowAdsShow) return false;           // Something is already on screen.
            if (!YG2.isTimerAdvCompleted) return false; // YG2 frequency guard not passed yet.
#endif
            return true;
        }

        /// <summary>
        /// Pre-interstitial sequence: freeze gameplay -> show countdown "2, 1, 0" ->
        /// request the interstitial. Pausing goes through YG2.PauseGame so the plugin
        /// manages timeScale / audio / cursor / EventSystem and restores them afterwards.
        /// </summary>
        private IEnumerator PreAdSequence()
        {
            // Freeze gameplay: time stops, audio pauses, cursor becomes visible and free.
            YG2.PauseGame(true);

            EnsureCountdownUI();
            _countdownRoot.SetActive(true);

            for (int n = CountdownStartNumber; n >= 0; n--)
            {
                // "Реклама через 2.." / "Ad in 2.." - re-read every tick,
                // so a language switch mid-countdown is picked up too.
                if (_countdownText != null)
                    _countdownText.text = string.Format(L10n.Get(CountdownKey), n);
                yield return new WaitForSecondsRealtime(CountdownStepSeconds);
            }

            _countdownRoot.SetActive(false);

            var ads = AppServices.Ads;
            if (ads == null || !ads.IsInterstitialReady())
            {
                // Ad became unavailable during the countdown - never leave the game frozen.
                Debug.LogWarning("[AdsCoordinator] Interstitial lost during countdown - resuming.");
                ResumeAfterInterstitial();
                yield break;
            }

            _isInterstitialShowing = true;

            // Call chain on Yandex targets (WebGL/Editor):
            // AdsServiceBase.ShowInterstitial -> YandexAdsService.PlatformShowInterstitial
            // -> YG2.InterstitialAdvShow(). AppLovin handles mobile builds instead.
            // HandleInterstitialFinished runs on close AND on failure - pause always ends.
            ads.ShowInterstitial(HandleInterstitialFinished);
        }

        private void HandleInterstitialFinished()
        {
            ResumeAfterInterstitial();

            _lastInterstitialTime = Time.time;

            // Track total interstitial views.
            int totalViews = PlayerPrefs.GetInt("TotalInterstitialsViewed", 0) + 1;
            PlayerPrefs.SetInt("TotalInterstitialsViewed", totalViews);
            PlayerPrefs.Save();

            if (totalViews == 15)
            {
                Debug.Log("[AdsCoordinator] Milestone reached: 15 interstitials viewed.");
                if (AppServices.Analytics != null)
                    AppServices.Analytics.LogEvent("milestone_inter_15");
            }

            Debug.Log($"[AdsCoordinator] Interstitial finished. Total viewed: {totalViews}. Timer reset.");
        }

        private void ResumeAfterInterstitial()
        {
            _isInterstitialShowing = false;
            YG2.PauseGame(false); // Restores the timeScale/audio/cursor snapshot taken at pause.
        }

        /// <summary>Creates (once) the fullscreen dimmed overlay with the big countdown number.</summary>
        private void EnsureCountdownUI()
        {
            if (_countdownRoot != null) return;

            _countdownRoot = new GameObject("InterstitialCountdown");

            var canvas = _countdownRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = CountdownSortingOrder;

            // Dim background that also blocks clicks while counting down.
            var dim = new GameObject("Dim");
            dim.transform.SetParent(_countdownRoot.transform, false);
            var dimImage = dim.AddComponent<Image>();
            dimImage.color = new Color(0f, 0f, 0f, CountdownDimAlpha);
            dimImage.rectTransform.anchorMin = Vector2.zero;
            dimImage.rectTransform.anchorMax = Vector2.one;
            dimImage.rectTransform.offsetMin = Vector2.zero;
            dimImage.rectTransform.offsetMax = Vector2.zero;

            // Big centered number.
            var label = new GameObject("Number");
            label.transform.SetParent(_countdownRoot.transform, false);
            var labelRect = label.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            _countdownText = label.AddComponent<Text>();
            _countdownText.alignment = TextAnchor.MiddleCenter;
            _countdownText.fontSize = 220;
            _countdownText.color = Color.white;
            _countdownText.font = GetBuiltinFont();

            _countdownRoot.SetActive(false);
        }

        private static Font GetBuiltinFont()
        {
            try { return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
            catch { return Resources.GetBuiltinResource<Font>("Arial.ttf"); }
        }
    }
}
