using System.Collections;
using UnityEngine;

namespace Treasures.Services
{
    /// <summary>
    /// Coordinates ad display logic based on Remote Config settings.
    /// Handles the interstitial timer and initial banner display.
    /// </summary>
    public class AdsCoordinator : MonoBehaviour
    {
        private const string BannerEnabledKey = "banner_enabled";
        private const string InitialAdDelayKey = "initial_ad_delay_sec";
        private const string InterstitialIntervalKey = "interstitial_interval_sec";

        private bool _isBannerShowing;
        private bool _isInterstitialShowing;
        private float _lastInterstitialTime;

        private IEnumerator Start()
        {
            while (!AppServices.AdsReady || !AppServices.FirebaseReady)
            {
                yield return new WaitForSeconds(1f);
            }

            Debug.Log("[AdsCoordinator] Services ready. Waiting for initial delay...");

            long initialDelay = AppServices.Analytics.GetLong(InitialAdDelayKey, 180);
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
                    yield return new WaitForSeconds(1f);
                    continue;
                }

                long interval = AppServices.Analytics.GetLong(InterstitialIntervalKey, 90);
                float timeSinceLastAd = Time.time - _lastInterstitialTime;

                if (timeSinceLastAd >= interval)
                {
                    if (AppServices.Ads.IsInterstitialReady())
                    {
                        Debug.Log("[AdsCoordinator] Time's up! Showing interstitial.");
                        _isInterstitialShowing = true;
                        
                        AppServices.Ads.ShowInterstitial(() =>
                        {
                            _isInterstitialShowing = false;
                            _lastInterstitialTime = Time.time;
                            
                            // Track total interstitial views
                            int totalViews = PlayerPrefs.GetInt("TotalInterstitialsViewed", 0) + 1;
                            PlayerPrefs.SetInt("TotalInterstitialsViewed", totalViews);
                            PlayerPrefs.Save();

                            if (totalViews == 15)
                            {
                                Debug.Log("[AdsCoordinator] Milestone reached: 15 interstitials viewed. Logging event.");
                                AppServices.Analytics.LogEvent("milestone_inter_15");
                            }

                            Debug.Log($"[AdsCoordinator] Interstitial closed. Total viewed: {totalViews}. Timer reset.");
                        });
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
    }
}
