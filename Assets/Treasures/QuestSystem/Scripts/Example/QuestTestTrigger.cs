using Treasures.CurrencySystem;
using UnityEngine;

namespace ModularTreasures.Quests
{
    public class QuestTestTrigger : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private string actionKey = "BuySkin";
        [SerializeField] private float anyProgressAmount = 1f;
        [SerializeField] private int goldAmount = 1;

        public void AddGold()
        {
            CurrencyService.Instance.AddBalance(CurrencyType.Gold, goldAmount);
        }

        public void TriggerAction()
        {
            Debug.Log($"[QuestTest] Triggering action: {actionKey}");
            QuestActionSystem.TriggerAction(actionKey);
        }
        public void AddAnyProgress()
        {
            if (QuestManager.Instance != null)
            {
                Debug.Log($"[QuestTest] Adding progress: {anyProgressAmount}");
                QuestManager.Instance.AddProgress(anyProgressAmount);
            }
        }
    }
}
