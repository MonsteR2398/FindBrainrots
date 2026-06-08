using UnityEngine;

namespace Treasures.Services
{
    /// <summary>
    /// App-level audio service. Plays music + sound effects through a shared AudioMixer and
    /// exposes independent, persisted volume controls (Master / Music / SFX).
    /// All volume values are normalized [0..1] and converted to mixer decibels internally.
    /// </summary>
    public interface IAudioService
    {
        /// <summary>Overall output volume, normalized [0..1]. Persisted automatically.</summary>
        float MasterVolume { get; set; }

        /// <summary>Music bus volume, normalized [0..1]. Persisted automatically.</summary>
        float MusicVolume { get; set; }

        /// <summary>Sound-effects bus volume, normalized [0..1]. Persisted automatically.</summary>
        float SfxVolume { get; set; }

        /// <summary>Start (or cross-replace) the background music track.</summary>
        void PlayMusic(AudioClip clip, bool loop = true);

        /// <summary>Stop the currently playing music track.</summary>
        void StopMusic();

        /// <summary>Pause the currently playing music track.</summary>
        void PauseMusic();

        /// <summary>Resume a paused music track.</summary>
        void ResumeMusic();

        /// <summary>Play a one-shot sound effect (2D) on the SFX bus.</summary>
        void PlaySfx(AudioClip clip, float volumeScale = 1f);

        /// <summary>Play a one-shot sound effect at a world position, routed through the SFX bus.</summary>
        void PlaySfxAt(AudioClip clip, Vector3 position, float volumeScale = 1f);
    }
}
