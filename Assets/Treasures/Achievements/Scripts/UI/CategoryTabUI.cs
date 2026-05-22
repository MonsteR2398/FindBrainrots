using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace ModularTreasures.Achievements
{
    public class CategoryTabUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Button button;

        public event Action<AchievementCategorySO> OnSelected;
        private AchievementCategorySO _category;

        public void Setup(AchievementCategorySO category)
        {
            _category = category;
            if (iconImage != null) iconImage.sprite = category.Icon;
            if (nameText != null) nameText.text = category.CategoryName;
            
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnSelected?.Invoke(_category));
        }
    }
}
