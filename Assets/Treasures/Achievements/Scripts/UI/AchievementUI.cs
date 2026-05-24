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

        private AchievementCategorySO _currentCategory;

        private void OnEnable()
        {
            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.OnProgressUpdated += HandleProgressUpdated;
                RefreshUI();
            }
        }

        private void OnDisable()
        {
            if (AchievementManager.Instance != null)
                AchievementManager.Instance.OnProgressUpdated -= HandleProgressUpdated;
        }

        public void OpenUI(bool active)
        {
            gameObject.SetActive(active);
        }

        private void HandleProgressUpdated(string id, float progress)
        {
            if (_currentCategory != null)
            {
                DisplayCategory(_currentCategory);
            }
        }

        private void RefreshUI()
        {
            foreach (Transform child in categoryContainer) Destroy(child.gameObject);

            var categories = AchievementManager.Instance != null ? AchievementManager.Instance.Categories : null;
            if (categories == null || categories.Count == 0) return;

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

            if (_currentCategory == null)
            {
                _currentCategory = categories[0];
            }

            DisplayCategory(_currentCategory);
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
