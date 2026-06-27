using System.Collections.Generic;
using UnityEngine;

namespace ModularTreasures.Quests
{
    [CreateAssetMenu(fileName = "New Quest", menuName = "Quests/Quest")]
    public class QuestSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id;
        [SerializeField] private string title;
        [SerializeField] private Sprite icon;

        [Header("Settings")]
        [SerializeField] private float targetValue;
        [SerializeField] private QuestConditionSO condition;
        [SerializeField] private bool isRepeatable;
        [SerializeField] private bool startFromCurrentValue;

        [Header("Rewards")]
        [SerializeField] private List<QuestRewardSO> rewards;

        public string Id => id;
        public string Title => Treasures.Localization.Localization.Get(title);
        public Sprite Icon => icon;
        public float TargetValue => targetValue;
        public QuestConditionSO Condition => condition;
        public bool IsRepeatable => isRepeatable;
        public bool StartFromCurrentValue => startFromCurrentValue;
        public List<QuestRewardSO> Rewards => rewards;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(id))
            {
                id = name;
            }
        }
    }
}
