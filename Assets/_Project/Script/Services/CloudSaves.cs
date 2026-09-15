using System;
using UnityEngine;
using YG;

namespace Treasures.Services
{
    /// <summary>
    /// Central access point for the PluginYourGames "Storage" module (cloud saves).
    /// <para>
    /// All persistent game data of this project lives in <see cref="YG2.saves"/> - directly or
    /// through <c>RedefineYG.PlayerPrefs</c> (the plugin's PlayerPrefs override) - and is pushed to
    /// the platform with <see cref="YG2.SaveProgress"/>. This helper adds the two things the raw
    /// plugin API does not provide:
    /// </para>
    /// <list type="bullet">
    /// <item><description>
    /// <see cref="DataReady"/> - raised when the <b>asynchronous</b> cloud data has been applied to
    /// <see cref="YG2.saves"/>. Any manager that caches save data in memory must re-read it there,
    /// otherwise its next write would push stale defaults back to the cloud.
    /// </description></item>
    /// <item><description>
    /// <see cref="Save"/> - remembers a save requested before the SDK finished initializing and
    /// flushes it as soon as the data is available, instead of dropping the write.
    /// </description></item>
    /// </list>
    /// </summary>
    public static class CloudSaves
    {
        /// <summary>True once the plugin has delivered the save data (cloud, local or defaults).</summary>
        public static bool IsReady { get; private set; }

        /// <summary>Raised every time the plugin refreshes <see cref="YG2.saves"/>.</summary>
        public static event Action DataReady;

        private static bool _hooked;
        private static bool _saveQueued;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitializeSession()
        {
            EnsureHooked();
        }

        /// <summary>
        /// Subscribes to <see cref="DataReady"/>. The handler is invoked immediately when the data
        /// has already been delivered, so callers can never miss the cloud load.
        /// </summary>
        public static void Subscribe(Action handler)
        {
            if (handler == null) return;

            EnsureHooked();
            DataReady += handler;

            if (IsReady) handler();
        }

        /// <summary>Removes a handler registered through <see cref="Subscribe"/>.</summary>
        public static void Unsubscribe(Action handler)
        {
            if (handler != null) DataReady -= handler;
        }

        /// <summary>
        /// Persists <see cref="YG2.saves"/> (cloud and/or local depending on the plugin settings).
        /// When the SDK is not initialized yet the request is queued and executed right after the
        /// data arrives, so early writes are not lost.
        /// </summary>
        public static void Save()
        {
            if (YG2.isSDKEnabled) YG2.SaveProgress();
            else _saveQueued = true;
        }

        private static void EnsureHooked()
        {
            if (!_hooked)
            {
                _hooked = true;

                // YG2 resets its delegates in Initialize() (BeforeSceneLoad), so this hook must run
                // after that point - it does (first Awake or AfterSceneLoad runtime init).
                YG2.onGetSDKData -= HandleSdkData;
                YG2.onGetSDKData += HandleSdkData;
            }

            // Data may already be present when the SDK initialized synchronously (StartInit() always
            // raises onGetSDKData before that point).
            if (YG2.isSDKEnabled) HandleSdkData();
        }

        private static void HandleSdkData()
        {
            IsReady = true;

            try
            {
                DataReady?.Invoke();
            }
            finally
            {
                if (_saveQueued)
                {
                    _saveQueued = false;
                    if (YG2.isSDKEnabled) YG2.SaveProgress();
                }
            }
        }
    }
}
