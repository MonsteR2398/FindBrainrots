using UnityEngine;
using System.Collections.Generic;

namespace ModularTreasures.Achievements
{
    public class AchievementUI : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private CategoryTabUI categoryTabPrefab;
        [SerializeField] private AchievementItemUI achievementItemPrefab;

        [Header("Containers")]
        [SerializeField] private Transform categoryContainer;
        [SerializeField] private Transform achievementContainer;

        [Header("Data")]
        [SerializeField] private List<AchievementCategorySO> categories;

        private AchievementCategorySO _currentCategory;

        private void OnEnable()
        {
            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.OnProgressUpdated += HandleProgressUpdated;
                
                if (_currentCategory == null && categories.Count > 0)
                    _currentCategory = categories[0];
    
                if (_currentCategory != null)
                    DisplayCategory(_currentCategory);
            }

        }

        private void OnDisable()
        {
            if (AchievementManager.Instance != null)
                AchievementManager.Instance.OnProgressUpdated -= HandleProgressUpdated;
        }

        private void HandleProgressUpdated(string id, float progress)
        {
            if (_currentCategory != null)
            {
                DisplayCategory(_currentCategory);
            }
        }

        private void Start()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            foreach (Transform child in categoryContainer) Destroy(child.gameObject);

            foreach (var category in categories)
            {
                var tab = Instantiate(categoryTabPrefab, categoryContainer);
                tab.Setup(category);
                tab.OnSelected += (cat) => 
                {
                    _currentCategory = cat;
                    DisplayCategory(cat);
                };
            }

            if (_currentCategory == null && categories.Count > 0)
            {
                _currentCategory = categories[0];
            }

            if (_currentCategory != null)
            {
                DisplayCategory(_currentCategory);
            }
        }

        private void DisplayCategory(AchievementCategorySO category)
        {
            foreach (Transform child in achievementContainer) Destroy(child.gameObject);

            foreach (var achievement in category.Achievements)
            {
                var item = Instantiate(achievementItemPrefab, achievementContainer);
                float progress = AchievementManager.Instance.GetProgress(achievement.Id);
                item.Setup(achievement, progress);
            }
        }
    }
}
