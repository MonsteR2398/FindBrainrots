using UnityEngine;

namespace ModularTreasures.Quests
{
    public abstract class QuestRewardSO : ScriptableObject
    {
        public abstract void GiveReward();
    }
}
