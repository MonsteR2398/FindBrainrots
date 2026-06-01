using System.Collections.Generic;
using UnityEngine;

namespace ModularTreasures.Quests
{
    [CreateAssetMenu(fileName = "New Quest Collection", menuName = "Quests/Quest Collection")]
    public class QuestCollectionSO : ScriptableObject
    {
        [SerializeField] private List<QuestSO> quests;

        public List<QuestSO> Quests => quests;
        
        public QuestSO GetQuest(int index)
        {
            if (index >= 0 && index < quests.Count)
            {
                return quests[index];
            }
            return null;
        }

        public int Count => quests.Count;
    }
}
