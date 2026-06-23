using UnityEngine;
using ModularTreasures.Quests;
using ModularCollection.Core;
using ModularCollection.Data;

namespace ModularCollection.Quests
{
    [CreateAssetMenu(fileName = "New Rarity Collection Condition", menuName = "Quests/Conditions/Rarity Collection Condition")]
    public class RarityCollectionConditionSO : QuestConditionSO
    {
        [SerializeField] private CollectionCategorySO categoryFilter;
        [SerializeField] private RaritySettingsSO rarityFilter;

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
            if (categoryFilter == null) return;

            foreach (var item in categoryFilter.Items)
            {
                if (item != null && item.ItemID == itemId)
                {
                    if (rarityFilter == null || item.Rarity == rarityFilter)
                    {
                        AddProgress(1f);
                    }
                    return;
                }
            }
        }

        public override float GetCurrentValue()
        {
            if (CollectionManager.Instance == null) return 0f;

            int count = 0;
            if (categoryFilter == null)
            {
                return 0f;
            }
            else
            {
                foreach (var item in categoryFilter.Items)
                {
                    if (item != null && CollectionManager.Instance.IsUnlocked(item.ItemID))
                    {
                        if (rarityFilter == null || item.Rarity == rarityFilter)
                        {
                            count++;
                        }
                    }
                }
            }
            return count;
        }
    }
}