using Treasures.CurrencySystem;
using UnityEngine;

namespace ModularSkinShop.Data
{
    [CreateAssetMenu(fileName = "NewSkin", menuName = "SkinShop/Skin")]
    public class SkinSO : ScriptableObject
    {
        public string ID;
        public string DisplayName;
        public GameObject Prefab;
        public Sprite Icon;
        public CurrencyValue[] CurrencyPrice;
        
        [Header("Map Purchase Settings")]
        [Tooltip("If true, this skin can only be purchased on the map, not in the main shop")]
        public bool mapOnly = false;
    }
}
