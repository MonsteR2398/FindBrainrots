using System.Collections.Generic;
using UnityEngine;

namespace ModularSkinShop.Data
{
    [CreateAssetMenu(fileName = "NewSkinLibrary", menuName = "SkinShop/Library")]
    public class SkinLibrarySO : ScriptableObject
    {
        public List<SkinSO> Skins;
    }
}
