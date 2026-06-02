using UnityEngine;
using ModularTreasures.Quests;

namespace ModularTreasures.Achievements
{
    [CreateAssetMenu(fileName = "New Achievement Condition", menuName = "Quests/Conditions/Achievement Condition")]
    public class AchievementConditionSO : ActionConditionSO
    {
        [SerializeField] private string achievementId;

        private void OnValidate()
        {
            // Set actionKey automatically to match AchievementManager's trigger
            actionKey = achievementId + "Add";
        }

        public override float GetCurrentValue()
        {
            if (AchievementManager.Instance != null)
            {
                return AchievementManager.Instance.GetProgress(achievementId);
            }
            return 0f;
        }
    }
}