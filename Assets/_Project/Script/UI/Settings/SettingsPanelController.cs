using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Treasures.Services;
using Treasures.Settings;
using Treasures.Localization;
using L10n = Treasures.Localization.Localization;

namespace Treasures.UI
{
    /// <summary>
    /// Drives the in-game Settings panel. Binds three sliders to:
    ///   - Music volume   -> AppServices.Audio.MusicVolume   (0..1)
    ///   - SFX volume     -> AppServices.Audio.SfxVolume     (0..1)
    ///   - Sensitivity    -> GameSettings.SensitivityMultiplier (Min..Max)
    /// The open/close buttons toggle <see cref="panelRoot"/>. Values are read back from the
    /// live settings every time the panel opens, so the UI always reflects the saved state.
    /// </summary>
    public class SettingsPanelController : MonoBehaviour
    {
        [Header("Window")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button openButton;
        [SerializeField] private Button closeButton;

        [Header("Sliders")]
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Slider sensitivitySlider;

        [Header("Language")]
        [SerializeField] private Button languagePrevButton;
        [SerializeField] private Button languageNextButton;
        [SerializeField] private TMP_Text languageValueLabel;

        private void Awake()
        {
            if (openButton != null) openButton.onClick.AddListener(Open);
            if (closeButton != null) closeButton.onClick.AddListener(Close);

            if (languagePrevButton != null) languagePrevButton.onClick.AddListener(SelectPreviousLanguage);
            if (languageNextButton != null) languageNextButton.onClick.AddListener(SelectNextLanguage);

            if (musicSlider != null)
            {
                musicSlider.minValue = 0f;
                musicSlider.maxValue = 1f;
                musicSlider.onValueChanged.AddListener(OnMusicChanged);
            }
            if (sfxSlider != null)
            {
                sfxSlider.minValue = 0f;
                sfxSlider.maxValue = 1f;
                sfxSlider.onValueChanged.AddListener(OnSfxChanged);
            }
            if (sensitivitySlider != null)
            {
                sensitivitySlider.minValue = GameSettings.MinSensitivity;
                sensitivitySlider.maxValue = GameSettings.MaxSensitivity;
                sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
            }

            if (panelRoot != null) panelRoot.SetActive(false);
        }

        /// <summary>Show the panel and sync the sliders to the current saved values.</summary>
        public void Open()
        {
            RefreshFromSettings();
            if (panelRoot != null) panelRoot.SetActive(true);
        }

        /// <summary>Hide the panel.</summary>
        public void Close()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        private void RefreshFromSettings()
        {
            if (AppServices.Audio != null)
            {
                if (musicSlider != null) musicSlider.SetValueWithoutNotify(AppServices.Audio.MusicVolume);
                if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(AppServices.Audio.SfxVolume);
            }
            if (sensitivitySlider != null)
                sensitivitySlider.SetValueWithoutNotify(GameSettings.SensitivityMultiplier);

            RefreshLanguageLabel();
        }

        private void RefreshLanguageLabel()
        {
            if (languageValueLabel != null)
                languageValueLabel.text = L10n.CurrentLanguageName;
        }

        private void SelectPreviousLanguage() => CycleLanguage(-1);
        private void SelectNextLanguage() => CycleLanguage(+1);

        private void CycleLanguage(int step)
        {
            int count = L10n.LanguageCount;
            if (count <= 0) return;

            // Wrap around so the selector cycles through all languages.
            int next = (L10n.CurrentLanguageIndex + step + count) % count;
            L10n.CurrentLanguageIndex = next;
            RefreshLanguageLabel();
        }

        private void OnMusicChanged(float value)
        {
            if (AppServices.Audio != null) AppServices.Audio.MusicVolume = value;
        }

        private void OnSfxChanged(float value)
        {
            if (AppServices.Audio != null) AppServices.Audio.SfxVolume = value;
        }

        private void OnSensitivityChanged(float value)
        {
            GameSettings.SensitivityMultiplier = value;
        }
    }
}
