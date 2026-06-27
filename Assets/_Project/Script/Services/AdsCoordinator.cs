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

            Debug.Log("[AdsCoordinator] Services ready. Applying Remote Config...");

            ApplyBannerSettings();

            _lastInterstitialTime = Time.time;
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

                long interval = AppServices.Analytics.GetLong(InterstitialIntervalKey, 60);
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
                            Debug.Log("[AdsCoordinator] Interstitial closed. Timer reset.");
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
