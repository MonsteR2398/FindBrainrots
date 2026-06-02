using UnityEngine;

namespace ModularTreasures.Quests
{
    [CreateAssetMenu(fileName = "New Action Condition", menuName = "Quests/Conditions/Action Condition")]
    public class ActionConditionSO : QuestConditionSO
    {
        [SerializeField] protected string actionKey;

        protected override void OnEnableCondition()
        {
            QuestActionSystem.OnActionTriggered += HandleAction;
        }

        protected override void OnDisableCondition()
        {
            QuestActionSystem.OnActionTriggered -= HandleAction;
        }

        protected virtual void HandleAction(string key, float value)
        {
            if (key == actionKey)
                AddProgress(value);
        }
    }

    // Небольшой помощник, позволяющий вызывать общие евенты, не зная о системе квестов
    public static class QuestActionSystem
    {
        public static System.Action<string, float> OnActionTriggered;
            
        public static void TriggerAction(string key, float value) => OnActionTriggered?.Invoke(key, value);
    }
}
