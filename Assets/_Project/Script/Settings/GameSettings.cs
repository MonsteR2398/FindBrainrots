using System;
using UnityEngine;

namespace Treasures.Settings
{
    /// <summary>
    /// Persisted gameplay settings that are not audio-related. Currently holds the look
    /// sensitivity multiplier shared by mouse and touch input. Values persist via PlayerPrefs.
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

        /// <summary>Raised whenever the sensitivity multiplier changes.</summary>
        public static event Action SensitivityChanged;

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
                PlayerPrefs.Save();
                SensitivityChanged?.Invoke();
            }
        }
    }
}
