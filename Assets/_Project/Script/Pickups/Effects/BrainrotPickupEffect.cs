using UnityEngine;
using ModularTreasures.Achievements;
using System.Collections.Generic;
using ModularCollection.Data;

namespace Treasures.Pickups
{
    [CreateAssetMenu(fileName = "New Brainrot Effect", menuName = "Treasures/Pickups/Effects/Brainrot")]
    public class BrainrotPickupEffect : BasePickupEffect
    {
        [System.Serializable]
        private class AchievementEntry
        {
            public string key;
            public int value;
        }

        [Header("ACHIEVEMENTS")]
        [SerializeField] private List<AchievementEntry> _achievements = new List<AchievementEntry>();

        public override void Apply(GameObject picker)
        {

            if (AchievementManager.Instance != null)
            {
                foreach (var achieve in _achievements)
                    AchievementManager.Instance.AddProgress(achieve.key, achieve.value);
            }
            else
            {
                Debug.LogWarning("[BrainrotEffect] AchievementManager instance not found!");
            }
        }
    }
}
