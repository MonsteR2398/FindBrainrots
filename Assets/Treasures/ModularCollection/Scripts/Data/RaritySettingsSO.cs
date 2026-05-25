using UnityEngine;

namespace ModularCollection.Data
{
    [CreateAssetMenu(fileName = "RaritySettings", menuName = "Modular Collection/Rarity Settings")]
    public class RaritySettingsSO : ScriptableObject
    {
        [SerializeField] private string rarityID;
        [SerializeField] private string displayName;
        [SerializeField] private Color rarityColor = Color.white;

        public string RarityID => rarityID;
        public string DisplayName => displayName;
        public Color RarityColor => rarityColor;

        public void Initialize(string id, string name, Color color)
        {
            rarityID = id;
            displayName = name;
            rarityColor = color;
        }
    }
}
