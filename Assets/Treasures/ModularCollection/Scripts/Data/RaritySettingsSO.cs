using UnityEngine;

namespace ModularCollection.Data
{
    [CreateAssetMenu(fileName = "RaritySettings", menuName = "Modular Collection/Rarity Settings")]
    public class RaritySettingsSO : ScriptableObject
    {
        [SerializeField] private string rarityID;
        [SerializeField] private string displayName;
        [SerializeField] private Color rarityColor = Color.white;
        [SerializeField] private Sprite rarityButtonSprite;
        [SerializeField] private Sprite rarityCardSprite;
        [SerializeField] private Sprite glitterImageSprite;

        public string RarityID => rarityID;
        public string DisplayName => displayName;
        public Color Color => rarityColor;
        public Sprite ButtonSprite => rarityButtonSprite;
        public Sprite CardSprite => rarityCardSprite;
        public Sprite GlitterSprite => glitterImageSprite;

        public void Initialize(string id, string name, Color color)
        {
            rarityID = id;
            displayName = name;
            rarityColor = color;
        }
    }
}
