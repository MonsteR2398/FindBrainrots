using UnityEngine;
using ModularTreasures.Quests;
using ModularCollection.Core;
using ModularCollection.Data;

namespace ModularCollection.Quests
{
    [CreateAssetMenu(fileName = "New Collection Condition", menuName = "Quests/Conditions/Collection Condition")]
    public class CollectionConditionSO : QuestConditionSO
    {
        [SerializeField] private CollectionCategorySO categoryFilter;

        protected override void OnEnableCondition()
        {
            if (CollectionManager.Instance != null)
            {
                CollectionManager.Instance.OnItemUnlocked += HandleItemUnlocked;
            }
        }

        protected override void OnDisableCondition()
        {
            if (CollectionManager.Instance != null)
            {
                CollectionManager.Instance.OnItemUnlocked -= HandleItemUnlocked;
            }
        }

        private void HandleItemUnlocked(string itemId)
        {
            if (categoryFilter == null)
            {
                AddProgress(1f);
            }
            else
            {
                foreach (var item in categoryFilter.Items)
                {
                    if (item.ItemID == itemId)
                    {
                        AddProgress(1f);
                        return;
                    }
                }
            }
        }

        public override float GetCurrentValue()
        {
            if (CollectionManager.Instance == null) return 0f;

            int count = 0;
            if (categoryFilter == null)
            {
                return CollectionManager.Instance.UnlockedCount;
            }
            else
            {
                foreach (var item in categoryFilter.Items)
                {
                    if (CollectionManager.Instance.IsUnlocked(item.ItemID))
                        count++;
                }
            }
            return count;
        }
    }
}