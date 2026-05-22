using UnityEngine;
using System.Collections.Generic;

namespace ModularTreasures.Achievements
{
    [CreateAssetMenu(fileName = "New Category", menuName = "Achievements/Category")]
    public class AchievementCategorySO : ScriptableObject
    {
        [SerializeField] private string categoryName;
        [SerializeField] private Sprite icon;
        [SerializeField] private List<AchievementSO> achievements = new List<AchievementSO>();

        public string CategoryName => categoryName;
        public Sprite Icon => icon;
        public List<AchievementSO> Achievements => achievements;
    }
}
