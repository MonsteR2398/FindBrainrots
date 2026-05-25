using UnityEngine;

namespace ModularCollection.Data
{
    [CreateAssetMenu(fileName = "CollectibleItem", menuName = "Modular Collection/Collectible Item")]
    public class CollectibleItemSO : ScriptableObject
    {
        [SerializeField] private string itemID;
        [SerializeField] private string itemName;
        [SerializeField] private Sprite icon;
        [SerializeField] private RaritySettingsSO rarity;
        [SerializeField] private GameObject worldPrefab;
        [SerializeField] private Texture2D overrideTexture;

        public string ItemID => itemID;
        public string ItemName => itemName;
        public Sprite Icon => icon;
        public RaritySettingsSO Rarity => rarity;
        public GameObject WorldPrefab => worldPrefab;
        public Texture2D OverrideTexture => overrideTexture;

        public void Initialize(string id, string name, Sprite icon, RaritySettingsSO rarity, GameObject worldPrefab = null, Texture2D overrideTexture = null)
        {
            this.itemID = id;
            this.itemName = name;
            this.icon = icon;
            this.rarity = rarity;
            this.worldPrefab = worldPrefab;
            this.overrideTexture = overrideTexture;
        }
    }
}
