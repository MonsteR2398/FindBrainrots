using UnityEngine;
using Treasures.CurrencySystem;

namespace ModularTreasures.Quests
{
    [CreateAssetMenu(fileName = "New Currency Condition", menuName = "Quests/Conditions/Currency Condition")]
    public class CurrencyConditionSO : QuestConditionSO
    {
        [SerializeField] private CurrencyType type;

        protected override void OnEnableCondition()
        {
            if (CurrencyService.Instance != null)
                CurrencyService.Instance.OnCurrencyReceived += HandleCurrencyReceived;
        }

        protected override void OnDisableCondition()
        {
            if (CurrencyService.Instance != null)
                CurrencyService.Instance.OnCurrencyReceived -= HandleCurrencyReceived;
        }

        private void HandleCurrencyReceived(CurrencyType receivedType, long amount)
        {
            if (receivedType == type)
                AddProgress((float)amount);
        }
    }
}
