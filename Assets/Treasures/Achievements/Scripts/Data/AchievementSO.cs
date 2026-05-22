using UnityEngine;

namespace ModularTreasures.Achievements
{
    [CreateAssetMenu(fileName = "New Achievement", menuName = "Achievements/Achievement")]
    public class AchievementSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id;
        [SerializeField] private string title;
        [TextArea][SerializeField] private string description;
        [SerializeField] private Sprite icon;

        [Header("Settings")]
        [SerializeField] private float targetValue;

        public string Id => id;
        public string Title => title;
        public string Description => description;
        public Sprite Icon => icon;
        public float TargetValue => targetValue;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(id))
            {
                id = name;
            }
        }
    }
}
