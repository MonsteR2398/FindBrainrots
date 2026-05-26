using ModularSkinShop.Data;
using UnityEngine;

namespace ModularSkinShop.Core
{
    public class SkinShopRender : MonoBehaviour
    {
        public Transform spawnTarget;
        public SkinShopManager ShopManager;
        private GameObject _currentSkinInstance;


        private void Start()
        {
            if (ShopManager != null)
            {
                ShopManager.OnSkinSelect += SelectSkin;

                string activeId = ShopManager.GetActiveId();
                if (!string.IsNullOrEmpty(activeId) && ShopManager.Library != null)
                {
                    var activeSkin = ShopManager.Library.Skins.Find(s => s.ID == activeId);
                    if (activeSkin != null) SelectSkin(activeSkin);
                }
            }
        }
        private void SelectSkin(SkinSO skin)
        {
            if (skin == null || skin.Prefab == null) return;
            
            if (_currentSkinInstance != null)
                Destroy(_currentSkinInstance);

            _currentSkinInstance = Instantiate(skin.Prefab, spawnTarget);
            _currentSkinInstance.transform.localPosition = Vector3.zero;
            _currentSkinInstance.transform.localRotation = Quaternion.identity;
        }
    }
}
