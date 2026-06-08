using UnityEngine;
using UnityEngine.Audio;

namespace Treasures.Services
{
    /// <summary>
    /// Runtime audio service. Loads the <c>GameAudioMixer</c> from Resources, routes a music
    /// AudioSource and an SFX AudioSource through their mixer groups, and drives the exposed
    /// volume parameters (Master / Music / SFX). Volume settings persist via PlayerPrefs.
    ///
    /// Created and owned by <see cref="AppBootstrap"/> (DontDestroyOnLoad). Access through
    /// <c>AppServices.Audio</c>.
    /// </summary>
    public class AudioManager : MonoBehaviour, IAudioService
    {
        private const string MixerResourcePath = "GameAudioMixer";

        // Exposed mixer parameter names (must match the AudioMixer asset).
        private const string MasterParam = "MasterVolume";
        private const string MusicParam = "MusicVolume";
        private const string SfxParam = "SFXVolume";

        // PlayerPrefs keys.
        private const string MasterKey = "audio.master";
        private const string MusicKey = "audio.music";
        private const string SfxKey = "audio.sfx";

        // Mixer group names.
        private const string MusicGroupName = "Music";
        private const string SfxGroupName = "SFX";

        private const float MinDb = -80f;

        private AudioMixer _mixer;
        private AudioSource _musicSource;
        private AudioSource _sfxSource;
        private AudioMixerGroup _sfxGroup;

        private float _masterVolume = 1f;
        private float _musicVolume = 1f;
        private float _sfxVolume = 1f;

        public float MasterVolume
        {
            get => _masterVolume;
            set => SetVolume(ref _masterVolume, value, MasterParam, MasterKey);
        }

        public float MusicVolume
        {
            get => _musicVolume;
            set => SetVolume(ref _musicVolume, value, MusicParam, MusicKey);
        }

        public float SfxVolume
        {
            get => _sfxVolume;
            set => SetVolume(ref _sfxVolume, value, SfxParam, SfxKey);
        }

        private void Awake()
        {
            _mixer = Resources.Load<AudioMixer>(MixerResourcePath);
            if (_mixer == null)
            {
                Debug.LogError($"[AudioManager] Could not load AudioMixer at Resources/{MixerResourcePath}. Audio routing disabled.");
            }

            AudioMixerGroup musicGroup = null;
            if (_mixer != null)
            {
                var musicGroups = _mixer.FindMatchingGroups(MusicGroupName);
                if (musicGroups.Length > 0) musicGroup = musicGroups[0];

                var sfxGroups = _mixer.FindMatchingGroups(SfxGroupName);
                if (sfxGroups.Length > 0) _sfxGroup = sfxGroups[0];
            }

            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
            _musicSource.loop = true;
            _musicSource.outputAudioMixerGroup = musicGroup;

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;
            _sfxSource.loop = false;
            _sfxSource.outputAudioMixerGroup = _sfxGroup;

            // Load persisted volumes (default 1.0 = full).
            _masterVolume = PlayerPrefs.GetFloat(MasterKey, 1f);
            _musicVolume = PlayerPrefs.GetFloat(MusicKey, 1f);
            _sfxVolume = PlayerPrefs.GetFloat(SfxKey, 1f);

            ApplyToMixer(MasterParam, _masterVolume);
            ApplyToMixer(MusicParam, _musicVolume);
            ApplyToMixer(SfxParam, _sfxVolume);

            // Register with the global service locator.
            AppServices.Audio = this;
        }

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (clip == null) return;
            _musicSource.clip = clip;
            _musicSource.loop = loop;
            _musicSource.Play();
        }

        public void StopMusic() => _musicSource.Stop();

        public void PauseMusic() => _musicSource.Pause();

        public void ResumeMusic() => _musicSource.UnPause();

        public void PlaySfx(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null) return;
            _sfxSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale));
        }

        public void PlaySfxAt(AudioClip clip, Vector3 position, float volumeScale = 1f)
        {
            if (clip == null) return;

            var go = new GameObject($"SFX_{clip.name}");
            go.transform.position = position;

            var src = go.AddComponent<AudioSource>();
            src.clip = clip;
            src.volume = Mathf.Clamp01(volumeScale);
            src.spatialBlend = 1f; // 3D
            src.outputAudioMixerGroup = _sfxGroup;
            src.Play();

            Destroy(go, clip.length + 0.1f);
        }

        private void SetVolume(ref float field, float value, string param, string prefsKey)
        {
            field = Mathf.Clamp01(value);
            ApplyToMixer(param, field);
            PlayerPrefs.SetFloat(prefsKey, field);
            PlayerPrefs.Save();
        }

        private void ApplyToMixer(string param, float normalized)
        {
            if (_mixer == null) return;
            _mixer.SetFloat(param, NormalizedToDecibels(normalized));
        }

        /// <summary>
        /// Convert a normalized [0..1] volume to mixer decibels using a perceptual log curve.
        /// 0 maps to silence (-80 dB), 1 maps to 0 dB.
        /// </summary>
        private static float NormalizedToDecibels(float normalized)
        {
            normalized = Mathf.Clamp01(normalized);
            if (normalized <= 0.0001f) return MinDb;
            return Mathf.Log10(normalized) * 20f;
        }
    }
}
