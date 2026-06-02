using UnityEngine;
using ModularTreasures.Quests;

namespace Treasures.CurrencySystem
{
    [CreateAssetMenu(fileName = "New Currency Condition", menuName = "Quests/Conditions/Currency Condition")]
    public class CurrencyConditionSO : ActionConditionSO
    {
        [SerializeField] private CurrencyType currencyType;

        private void OnValidate()
        {
            actionKey = $"{currencyType}Add";
        }

        public override float GetCurrentValue()
        {
            if (CurrencyService.Instance != null)
            {
                return CurrencyService.Instance.GetBalance(currencyType);
            }
            return 0f;
        }
    }
}