using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Treasures.CurrencySystem
{
    public class CurrencyController : MonoBehaviour
    {
        [SerializeField] private CurrencyFlyVFX vfxManager;
        [SerializeField] private List<CurrencyCounterUI> counters;
        
        [Header("Settings")]
        [SerializeField] private Button giveGoldButton;
        [SerializeField] private Button giveDiamondButton;

        private void Start()
        {
            vfxManager.OnItemReachedTarget += HandleItemReached;
            
            giveGoldButton.onClick.AddListener(() => GiveReward(CurrencyType.Gold, 1000, giveGoldButton.transform.position));
            giveDiamondButton.onClick.AddListener(() => GiveReward(CurrencyType.Diamond, 50, giveDiamondButton.transform.position));
        }

        private void GiveReward(CurrencyType type, long amount, Vector3 pos)
        {
            CurrencyService.Instance.AddBalance(type, amount, silent: false);
            vfxManager.SpawnVFX(type, amount, pos);
        }

        private void HandleItemReached(CurrencyType type, long amount)
        {
            foreach (var counter in counters)
            {
                if (counter.CurrencyType == type)
                {
                    counter.AddVisualAmount(amount);
                }
            }
        }
    }
}
