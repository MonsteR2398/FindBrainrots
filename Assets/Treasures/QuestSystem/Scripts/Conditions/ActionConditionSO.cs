using UnityEngine;

namespace ModularTreasures.Quests
{
    [CreateAssetMenu(fileName = "New Action Condition", menuName = "Quests/Conditions/Action Condition")]
    public class ActionConditionSO : QuestConditionSO
    {
        [SerializeField] private string actionKey;

        protected override void OnEnableCondition()
        {
            QuestActionSystem.OnActionTriggered += HandleAction;
        }

        protected override void OnDisableCondition()
        {
            QuestActionSystem.OnActionTriggered -= HandleAction;
        }

        private void HandleAction(string key)
        {
            if (key == actionKey)
                AddProgress(1f);
        }
    }

    // Небольшой помощник, позволяющий вызывать общие евенты, не зная о системе квестов
    public static class QuestActionSystem
    {
        public static System.Action<string> OnActionTriggered;
            
        public static void TriggerAction(string key) => OnActionTriggered?.Invoke(key);
    }
}
