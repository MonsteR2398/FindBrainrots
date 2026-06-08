using UnityEngine;

namespace Treasures.Services
{
    /// <summary>
    /// App-level initialization orchestrator. Auto-starts before the first scene loads and
    /// survives scene loads (DontDestroyOnLoad). No setup in the scene is required.
    ///
    /// Initialization order (as required by AppLovin's guidelines):
    ///   1. AppLovin MAX  ->  presents GDPR/CMP consent flow + auto-inits mediated networks (incl. Google).
    ///   2. ONLY AFTER consent is collected (OnSdkInitializedEvent) -> initialize Firebase.
    /// </summary>
    public class AppBootstrap : MonoBehaviour
    {
        private static AppBootstrap _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoStart()
        {
            if (_instance != null) return;

            var go = new GameObject("[AppBootstrap]");
            _instance = go.AddComponent<AppBootstrap>();
            
            go.AddComponent<AdsCoordinator>();

            go.AddComponent<AudioManager>();

            DontDestroyOnLoad(go);
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            AppServices.Ads = new AppLovinAdsService();
            AppServices.Analytics = new FirebaseService();

            Debug.Log("[Bootstrap] Step 1 - initializing AppLovin MAX...");
            AppServices.Ads.Initialize(OnAdsInitialized);
        }

        private void OnAdsInitialized()
        {
            Debug.Log("[Bootstrap] AppLovin MAX + consent ready.");

            if (AppServices.Analytics != null && AppServices.Analytics.IsAvailable)
            {
                Debug.Log("[Bootstrap] Step 2 - initializing Firebase...");
                AppServices.Analytics.Initialize(OnFirebaseInitialized);
            }
            else
            {
                Debug.Log("[Bootstrap] Firebase SDK not present. Skipping Firebase init.");
            }
        }

        private void OnFirebaseInitialized(bool success)
        {
            Debug.Log("[Bootstrap] Firebase initialization complete. Success=" + success);
            Debug.Log("[Bootstrap] All services ready. AdsCoordinator is now active.");
        }
    }
}
