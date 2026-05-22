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
        public int Price;
    }
}
