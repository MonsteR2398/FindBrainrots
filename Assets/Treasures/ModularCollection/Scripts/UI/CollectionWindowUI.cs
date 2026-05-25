using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ModularCollection.Data;
using ModularCollection.Core;
using System.Linq;

namespace ModularCollection.UI
{
    public class CollectionWindowUI : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] public CollectionCategorySO currentCategory;
        [SerializeField] public RarityDatabaseSO rarityDatabase;

        [Header("References")]
        [SerializeField] private Transform itemGridRoot;
        [SerializeField] private Transform rarityFilterRoot;
        [SerializeField] private TextMeshProUGUI headerTitle;

        [Header("Prefabs")]
        [SerializeField] private CollectionItemUI itemPrefab;
        [SerializeField] private RarityFilterButtonUI filterButtonPrefab;

        private RaritySettingsSO selectedRarity;
        private List<RarityFilterButtonUI> filterButtons = new List<RarityFilterButtonUI>();

        private void Start()
        {
            if (currentCategory == null || rarityDatabase == null) return;

            CollectionManager.Instance.SetAllItemIds(currentCategory.Items.Select(i => i.ItemID));
            InitializeUI();
            
            CollectionManager.Instance.OnItemUnlocked += HandleItemUnlocked;
            CollectionManager.Instance.OnItemRead += HandleItemRead;
        }

        private void OnDestroy()
        {
            if (CollectionManager.Instance != null)
            {
                CollectionManager.Instance.OnItemUnlocked -= HandleItemUnlocked;
                CollectionManager.Instance.OnItemRead -= HandleItemRead;
            }
        }

        private void InitializeUI()
        {
            foreach (Transform child in rarityFilterRoot) Destroy(child.gameObject);
            filterButtons.Clear();

            foreach (var rarity in rarityDatabase.Rarities)
            {
                var btn = Instantiate(filterButtonPrefab, rarityFilterRoot);
                bool hasNew = CollectionManager.Instance.HasUnreadInRarity(rarity.RarityID, currentCategory.Items);
                btn.Setup(rarity, SelectRarity, hasNew);
                filterButtons.Add(btn);
            }

            if (rarityDatabase.Rarities.Count > 0)
                SelectRarity(rarityDatabase.Rarities[0]);
        }

        public void OpenUI(bool active)
        {
            gameObject.transform.GetChild(0).gameObject.SetActive(active);
        }
        
        public void SelectRarity(RaritySettingsSO rarity)
        {
            selectedRarity = rarity;
            headerTitle.text = rarity.DisplayName;
            RefreshGrid();
        }

        public void RefreshGrid()
        {
            foreach (Transform child in itemGridRoot) Destroy(child.gameObject);

            var filteredItems = currentCategory.Items.Where(i => i.Rarity == selectedRarity);

            foreach (var item in filteredItems)
            {
                var itemUI = Instantiate(itemPrefab, itemGridRoot);
                bool unlocked = CollectionManager.Instance.IsUnlocked(item.ItemID);
                bool read = CollectionManager.Instance.IsRead(item.ItemID);
                itemUI.Setup(item, unlocked, read);
            }
        }

        private void HandleItemUnlocked(string itemId)
        {
            UpdateNewIndicators();
            RefreshGrid();
        }

        private void HandleItemRead(string itemId)
        {
            UpdateNewIndicators();
        }

        private void UpdateNewIndicators()
        {
            for (int i = 0; i < rarityDatabase.Rarities.Count; i++)
            {
                var rarity = rarityDatabase.Rarities[i];
                bool hasNew = CollectionManager.Instance.HasUnreadInRarity(rarity.RarityID, currentCategory.Items);
                filterButtons[i].SetNewIndicator(hasNew);
            }
        }
    }
}
