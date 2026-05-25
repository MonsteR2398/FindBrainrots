using UnityEngine;
using Treasures.CurrencySystem;
using ModularTreasures.Achievements;
using System.Collections.Generic;

namespace Treasures.Pickups
{
    [CreateAssetMenu(fileName = "New Currency Effect", menuName = "Treasures/Pickups/Effects/Currency")]
    public class CurrencyPickupEffect : BasePickupEffect
    {
        [System.Serializable]
        private class AchievementEntry
        {
            public string key;
            public int value;
        }

        [SerializeField] private CurrencyType currencyType;
        [SerializeField] private int amount = 10;

        [Header("ACHIEVEMENTS")]
        [SerializeField] private List<AchievementEntry> _achievements = new List<AchievementEntry>();

        public override void Apply(GameObject picker)
        {
            if (CurrencyService.Instance != null)
            {
                CurrencyService.Instance.AddBalance(currencyType, amount, true);

                if (AchievementManager.Instance != null)
                {
                    foreach (var achieve in _achievements)
                        AchievementManager.Instance.AddProgress(achieve.key, achieve.value);
                }
            }
            else
            {
                Debug.LogWarning("[CurrencyEffect] CurrencyService instance not found!");
            }
        }
    }
}
