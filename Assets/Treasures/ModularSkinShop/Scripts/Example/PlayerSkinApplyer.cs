using UnityEngine;
using ModularSkinShop.Core;
using ModularSkinShop.Data;

namespace ModularSkinShop.Example
{
    public class PlayerSkinApplyer : MonoBehaviour
    {
        public SkinShopManager ShopManager;
        public Transform SkinParent;

        private GameObject _currentSkinInstance;

        private void Start()
        {
            if (ShopManager != null)
            {
                ShopManager.OnSkinDressed += ApplySkin;
                
                string activeId = PlayerPrefs.GetString("Shop_Active", "");
                if (!string.IsNullOrEmpty(activeId) && ShopManager.Library != null)
                {
                    var activeSkin = ShopManager.Library.Skins.Find(s => s.ID == activeId);
                    if (activeSkin != null) ApplySkin(activeSkin);
                }
            }
        }

        private void ApplySkin(SkinSO skin)
        {
            if (skin == null || skin.Prefab == null) return;

            if (_currentSkinInstance != null)
                Destroy(_currentSkinInstance);

            _currentSkinInstance = Instantiate(skin.Prefab, SkinParent);
            _currentSkinInstance.transform.localPosition = Vector3.zero;
            _currentSkinInstance.transform.localRotation = Quaternion.identity;
        }
    }
}
