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
        [SerializeField] private CurrencyDefinition goldDef;
        [SerializeField] private CurrencyDefinition diamondDef;
        [SerializeField] private Button giveGoldButton;
        [SerializeField] private Button giveDiamondButton;

        private void Start()
        {
            vfxManager.OnItemReachedTarget += HandleItemReached;
            
            giveGoldButton.onClick.AddListener(() => GiveReward(goldDef, 1000, giveGoldButton.transform.position));
            giveDiamondButton.onClick.AddListener(() => GiveReward(diamondDef, 50, giveDiamondButton.transform.position));
        }

        private void GiveReward(CurrencyDefinition def, long amount, Vector3 pos)
        {
            CurrencyService.Instance.AddBalance(def.Type, amount, silent: true);
            
            vfxManager.SpawnVFX(def.Type, amount, pos);
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
