using UnityEngine;
using TMPro;
using ModularCollection.Core;
using ModularCollection.Data;
using Treasures.Localization;
using Treasures.Services;
using PlayerPrefs = RedefineYG.PlayerPrefs;

namespace ModularTreasures.Quests
{
    public class BrainrotTaskHUD : MonoBehaviour
    {
        [SerializeField] private TextMeshPro taskText;
        [SerializeField] private CollectionCategorySO brainrotCategory;
        [SerializeField] private int targetCount = 10;
        [SerializeField] private GameObject panelToDisable;
        [SerializeField] private string disableActionKey = "BrainrotQuest_FocusArrival";
        [SerializeField] private string taskTitleOverride = "Найди бреинротов";
        [SerializeField] private RaritySettingsSO rarityFilter;

        private void Start()
        {
            if (CollectionManager.Instance != null)
                CollectionManager.Instance.OnItemUnlocked += HandleItemUnlocked;
            
            QuestActionSystem.OnActionTriggered += HandleQuestAction;
            Localization.LanguageChanged += UpdateDisplay;
            UpdateDisplay();

            // Load saved disabled state. It lives in the cloud save, which is applied
            // asynchronously - so read it now and again once the YG2 Storage data has arrived.
            CloudSaves.Subscribe(ApplySavedState);
            ApplySavedState();
        }

        private void OnDestroy()
        {
            CloudSaves.Unsubscribe(ApplySavedState);
        }

        private void ApplySavedState()
        {
            if (PlayerPrefs.GetInt(GetSaveKey(), 0) == 1)
            {
                DisableHUD(false); // disable without triggering a save again
            }
        }

        private void OnDisable()
        {
            if (CollectionManager.Instance != null)
                CollectionManager.Instance.OnItemUnlocked -= HandleItemUnlocked;

            QuestActionSystem.OnActionTriggered -= HandleQuestAction;
            Localization.LanguageChanged -= UpdateDisplay;
        }

        private void HandleItemUnlocked(string itemId) => UpdateDisplay();

        private void HandleQuestAction(string key, float value)
        {
            if (key == disableActionKey)
            {
                DisableHUD(true);
            }
        }

        private void UpdateDisplay()
        {
            if (brainrotCategory == null || CollectionManager.Instance == null) return;

            int current = 0;
            foreach (var item in brainrotCategory.Items)
            {
                if (item != null && CollectionManager.Instance.IsUnlocked(item.ItemID))
                {
                    if (rarityFilter == null || item.Rarity == rarityFilter)
                    {
                        current++;
                    }
                }
            }
            
            if (taskText != null)
            {
                string key = string.IsNullOrEmpty(taskTitleOverride) ? "Найди бреинротов" : taskTitleOverride;
                string localizedTitle = Localization.Get(key);
                taskText.text = $"{localizedTitle}\n{current}/{targetCount}";
            }
        }

        public void DisableHUD()
        {
            DisableHUD(true);
        }

        public void DisableHUD(bool shouldSave)
        {
            if (panelToDisable != null) panelToDisable.SetActive(false);
            else gameObject.SetActive(false);

            if (shouldSave)
            {
                PlayerPrefs.SetInt(GetSaveKey(), 1);
                CloudSaves.Save();
            }

            if (CameraSequenceManager.Instance != null)
            {
                CameraSequenceManager.Instance.ReleaseCamera();
            }
        }

        private string GetSaveKey()
        {
            return "BrainrotTaskHUD_Disabled_" + disableActionKey;
        }
    }
}