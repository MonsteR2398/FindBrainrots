using ModularSkinShop.Data;
using UnityEngine;

namespace ModularSkinShop.Core
{
    /// <summary>
    /// Renders skin models to a render texture for preview.
    /// Used by both SkinShopUI and SkinPurchaseUI.
    /// </summary>
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

        /// <summary>
        /// Public method to render a skin (used by SkinPurchaseUI).
        /// </summary>
        public void RenderSkin(SkinSO skin)
        {
            SelectSkin(skin);
        }

        /// <summary>
        /// Clear the current skin instance.
        /// </summary>
        public void Clear()
        {
            if (_currentSkinInstance != null)
            {
                Destroy(_currentSkinInstance);
                _currentSkinInstance = null;
            }
        }
    }
}
