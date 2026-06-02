using UnityEngine;
using Treasures.CurrencySystem;

namespace ModularTreasures.Quests
{
    [CreateAssetMenu(fileName = "New Currency Reward", menuName = "Quests/Rewards/Currency Reward")]
    public class CurrencyRewardSO : QuestRewardSO
    {
        [SerializeField] private CurrencyType type;
        [SerializeField] private long amount;

        public override void GiveReward(Transform targetPos)
        {
            if (CurrencyService.Instance != null)
                CurrencyService.Instance.AddBalance(type, amount, targetPos);
        }
    }
}
