namespace Treasures.Services
{
    /// <summary>
    /// Global access point for the app-level services. Populated by <c>AppBootstrap</c> at startup.
    /// Use the *Ready helpers before calling into a service from gameplay code.
    /// </summary>
    public static class AppServices
    {
        /// <summary>Ads service (AppLovin MAX on mobile). May be null on unsupported platforms.</summary>
        public static IAdsService Ads { get; internal set; }

        /// <summary>Analytics/backend service (Firebase). Never throws even when the SDK is absent.</summary>
        public static IAnalyticsService Analytics { get; internal set; }

        /// <summary>Audio service (music + SFX playback and persisted volume controls).</summary>
        public static IAudioService Audio { get; internal set; }

        /// <summary>True when ads SDK + consent flow are fully initialized.</summary>
        public static bool AdsReady => Ads != null && Ads.IsInitialized;

        /// <summary>
        /// True only when the Firebase SDK is present in the project AND finished initializing,
        /// OR when the SDK is not present at all (meaning we are ready to proceed without it).
        /// </summary>
        public static bool FirebaseReady => Analytics != null && (!Analytics.IsAvailable || Analytics.IsInitialized);
    }
}
