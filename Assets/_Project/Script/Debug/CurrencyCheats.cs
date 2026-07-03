using UnityEngine;
using UnityEngine.InputSystem;
using Treasures.CurrencySystem;

namespace Treasures.Utility
{
    public class CurrencyCheats : MonoBehaviour
    {
        [SerializeField] private long goldAmount = 100000;
        [SerializeField] private long diamondAmount = 1000;

        private void Update()
        {
            if (Keyboard.current == null) return;

            if (Keyboard.current.kKey.wasPressedThisFrame)
            {
                CurrencyService.Instance.AddBalance(CurrencyType.Gold, goldAmount);
                UnityEngine.Debug.Log($"[Cheat] Added {goldAmount} Gold");
            }

            if (Keyboard.current.lKey.wasPressedThisFrame)
            {
                CurrencyService.Instance.AddBalance(CurrencyType.Diamond, diamondAmount);
                UnityEngine.Debug.Log($"[Cheat] Added {diamondAmount} Crystals (Diamond)");
            }
        }
    }
}
