using System;
using System.Collections.Generic;
using Treasures.Services;
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

        public int UnlockedCount => unlockedItemIds.Count;
        public IReadOnlyCollection<string> UnlockedItemIds => unlockedItemIds;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
                StartCoroutine(LogCollectionStatusRoutine());
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private System.Collections.IEnumerator LogCollectionStatusRoutine()
        {
            // Wait for services to be ready
            while (Treasures.Services.AppServices.Analytics == null || !Treasures.Services.AppServices.Analytics.IsInitialized)
            {
                yield return new WaitForSeconds(1f);
            }

            // The cloud save is applied asynchronously - wait for it so the logged count is real
            // (never fall back to the pre-load defaults).
            while (!CloudSaves.IsReady)
            {
                yield return null;
            }

            // Log current count at the start of the session
            Treasures.Services.AppServices.Analytics.LogEvent("collection_session_start_count", new Dictionary<string, object>
            {
                { "count", unlockedItemIds.Count }
            });
        }

        private void Initialize()
        {
            // The project stores everything in the cloud save (YG2 Storage module) - this
            // PlayerPrefs-based persistence is redirected to it by the plugin.
            persistence = new PlayerPrefsPersistence();
            ReloadFromStorage();

            // Cloud data arrives asynchronously - re-read it once it is applied, otherwise the next
            // unlock would push the pre-load collection to the cloud.
            CloudSaves.Subscribe(ReloadFromStorage);
        }

        private void OnDestroy()
        {
            CloudSaves.Unsubscribe(ReloadFromStorage);
        }

        private void ReloadFromStorage()
        {
            if (persistence == null) return;
            unlockedItemIds = persistence.LoadUnlockedItems();
            readItemIds = persistence.LoadReadItems();
        }

        public void SetPersistence(ICollectionPersistence newPersistence)
        {
            persistence = newPersistence;
            ReloadFromStorage();
        }

        public bool IsUnlocked(string itemId) => unlockedItemIds.Contains(itemId);
        public bool IsRead(string itemId) => readItemIds.Contains(itemId);

        public void UnlockItem(string itemId)
        {
            if (unlockedItemIds.Contains(itemId)) return;

            unlockedItemIds.Add(itemId);
            persistence.SaveUnlockedItems(unlockedItemIds);
            OnItemUnlocked?.Invoke(itemId);
            
            // Log analytics
            if (Treasures.Services.AppServices.Analytics != null && Treasures.Services.AppServices.Analytics.IsInitialized)
            {
                var parameters = new Dictionary<string, object>
                {
                    { "item_id", itemId },
                    { "unlocked_count", unlockedItemIds.Count },
                    { "total_available", allItemIds.Count }
                };
                Treasures.Services.AppServices.Analytics.LogEvent("collection_item_unlocked", parameters);
                
                // Also log a user property or a specific event for the total count to track "max reached"
                Treasures.Services.AppServices.Analytics.LogEvent("collection_total_count", new Dictionary<string, object>
                {
                    { "count", unlockedItemIds.Count }
                });
            }

            // Trigger QuestActionSystem for modular quest tracking
            ModularTreasures.Quests.QuestActionSystem.TriggerAction("CollectionItemUnlocked", 1f);
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
