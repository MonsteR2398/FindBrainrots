using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ModularTreasures.Achievements
{
    public class AchievementItemUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Slider progressSlider;

        public void Setup(AchievementSO achievement, float currentProgress)
        {
            if (iconImage != null) iconImage.sprite = achievement.Icon;
            if (titleText != null) titleText.text = achievement.Title;
            if (descriptionText != null) descriptionText.text = achievement.Description;
            if (progressSlider != null)
            {
                progressSlider.maxValue = achievement.TargetValue;
                progressSlider.value = currentProgress;
            }
        }
    }
}
