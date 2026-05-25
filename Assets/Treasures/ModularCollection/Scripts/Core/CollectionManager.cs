using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModularCollection.Core
{
    public class CollectionManager : MonoBehaviour
    {
        public static CollectionManager Instance { get; private set; }

        public event Action<string> OnItemUnlocked;
        public event Action<string> OnItemRead;

        private HashSet<string> unlockedItemIds = new HashSet<string>();
        private HashSet<string> readItemIds = new HashSet<string>();
        private List<string> allItemIds = new List<string>();
        private ICollectionPersistence persistence;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Initialize()
        {
            // Default to PlayerPrefs if nothing else is provided
            persistence = new PlayerPrefsPersistence();
            unlockedItemIds = persistence.LoadUnlockedItems();
            readItemIds = persistence.LoadReadItems();
        }

        public void SetPersistence(ICollectionPersistence newPersistence)
        {
            persistence = newPersistence;
            unlockedItemIds = persistence.LoadUnlockedItems();
            readItemIds = persistence.LoadReadItems();
        }

        public bool IsUnlocked(string itemId) => unlockedItemIds.Contains(itemId);
        public bool IsRead(string itemId) => readItemIds.Contains(itemId);

        public void UnlockItem(string itemId)
        {
            if (unlockedItemIds.Contains(itemId)) return;

            unlockedItemIds.Add(itemId);
            persistence.SaveUnlockedItems(unlockedItemIds);
            OnItemUnlocked?.Invoke(itemId);
        }


        public void MarkAsRead(string itemId)
        {
            if (!unlockedItemIds.Contains(itemId)) return;
            if (readItemIds.Contains(itemId)) return;

            readItemIds.Add(itemId);
            persistence.SaveReadItems(readItemIds);
            OnItemRead?.Invoke(itemId);
        }

        public void SetAllItemIds(IEnumerable<string> itemIds)
        {
            allItemIds = new List<string>(itemIds);
        }

        public void UnlockRandomItem()
        {
            if (allItemIds == null || allItemIds.Count == 0)
                return;

            var available = new List<string>();
            foreach (var id in allItemIds)
            {
                if (!unlockedItemIds.Contains(id))
                    available.Add(id);
            }

            if (available.Count == 0)
                return;

            string selectedId = available[UnityEngine.Random.Range(0, available.Count)];
            UnlockItem(selectedId);
        }

        public bool HasUnreadInRarity(string rarityId, IReadOnlyList<Data.CollectibleItemSO> allItems)
        {
            foreach (var item in allItems)
            {
                if (item.Rarity.RarityID == rarityId && IsUnlocked(item.ItemID) && !IsRead(item.ItemID))
                    return true;
            }
            return false;
        }
    }
}
