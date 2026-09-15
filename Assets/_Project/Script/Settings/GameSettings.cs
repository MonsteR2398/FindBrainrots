using System;
using Treasures.Services;
using UnityEngine;
using PlayerPrefs = RedefineYG.PlayerPrefs;

namespace Treasures.Settings
{
    /// <summary>
    /// Persisted gameplay settings that are not audio-related. Currently holds the look
    /// sensitivity multiplier shared by mouse and touch input. Values persist through the
    /// PluginYourGames "Storage" module (cloud save via <c>RedefineYG.PlayerPrefs</c>).
    ///
    /// The multiplier is applied on top of the per-platform base sensitivities configured on
    /// <c>CameraLookController</c> (mouseSensitivity / touchSensitivity).
    /// </summary>
    public static class GameSettings
    {
        private const string SensKey = "settings.sensitivity";

        /// <summary>Lowest selectable multiplier (quarter of the tuned base values).</summary>
        public const float MinSensitivity = 0.25f;

        /// <summary>Highest selectable multiplier (double the tuned base values).</summary>
        public const float MaxSensitivity = 2.0f;

        /// <summary>Default multiplier = no change to the tuned base values.</summary>
        public const float DefaultSensitivity = 1.0f;

        private static float _sensitivity = float.NaN;
        private static bool _subscribed;

        /// <summary>Raised whenever the sensitivity multiplier changes.</summary>
        public static event Action SensitivityChanged;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (_subscribed) return;
            _subscribed = true;

            // The cloud save is applied asynchronously - drop the cached multiplier once it has
            // arrived so the next read uses the saved (cloud) value.
            CloudSaves.Subscribe(Reload);
        }

        private static void Reload()
        {
            _sensitivity = float.NaN;
            SensitivityChanged?.Invoke();
        }

        /// <summary>
        /// Look sensitivity multiplier in [<see cref="MinSensitivity"/>..<see cref="MaxSensitivity"/>].
        /// Lazily loaded from PlayerPrefs on first access and saved on every change.
        /// </summary>
        public static float SensitivityMultiplier
        {
            get
            {
                if (float.IsNaN(_sensitivity))
                {
                    _sensitivity = Mathf.Clamp(
                        PlayerPrefs.GetFloat(SensKey, DefaultSensitivity),
                        MinSensitivity, MaxSensitivity);
                }
                return _sensitivity;
            }
            set
            {
                _sensitivity = Mathf.Clamp(value, MinSensitivity, MaxSensitivity);
                PlayerPrefs.SetFloat(SensKey, _sensitivity);
                CloudSaves.Save();
                SensitivityChanged?.Invoke();
            }
        }
    }
}
