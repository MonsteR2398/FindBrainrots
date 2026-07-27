using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ModularSkinShop.Core;
using ModularSkinShop.Data;
using Unity.VisualScripting;

namespace ModularSkinShop.UI
{
    public class SkinShopUI : MonoBehaviour
    {
        [Header("References")]
        public SkinShopManager Manager;
        public Transform ItemContainer;
        public GameObject ItemPrefab;

        private List<SkinShopItemUI> _spawnedItems = new List<SkinShopItemUI>();


        private void Start()
        {
            if (Manager == null) Manager = GetComponentInParent<SkinShopManager>();
            
            PopulateShop();
            RefreshUI();

            Manager.OnSkinDressed += (skin) => RefreshUI();
            Manager.OnSkinPurchased += (skin) => RefreshUI();

            ItemPrefab.SetActive(false);
        }

        public void RefreshUI()
        {
            foreach (var item in _spawnedItems)
            {
                item.UpdateUI();
            }
        }

        public void OpenUI(bool active)
        {
            transform.GetChild(0).gameObject.SetActive(active);
        }

        private void PopulateShop()
        {
            //_spawnedItems.Clear();
            foreach (var skin in Manager.Library.Skins)
            {
                // Skip skins that are map-only (should only be purchased on the map)
                if (skin.mapOnly) continue;
                
                var go = Instantiate(ItemPrefab, ItemContainer);
                go.SetActive(true);
                var itemUI = go.GetComponent<SkinShopItemUI>();
                itemUI.Setup(skin, Manager);
                _spawnedItems.Add(itemUI);
            }
        }


    }
}
