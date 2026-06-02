using UnityEngine;

namespace ModularTreasures.Quests
{
    public abstract class QuestConditionSO : ScriptableObject
    {
        protected QuestManager Manager;

        public virtual void Initialize(QuestManager manager)
        {
            Manager = manager;
            OnEnableCondition();
        }

        public virtual void Shutdown()
        {
            OnDisableCondition();
            Manager = null;
        }

        public virtual float GetCurrentValue() => 0f;

        protected abstract void OnEnableCondition();
        protected abstract void OnDisableCondition();
        
        protected void AddProgress(float amount)
        {
            if (Manager != null)
            {
                Manager.AddProgress(amount);
            }
        }
    }
}
