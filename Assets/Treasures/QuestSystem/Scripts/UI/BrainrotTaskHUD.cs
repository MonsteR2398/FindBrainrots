using UnityEngine;
using TMPro;
using ModularCollection.Core;
using ModularCollection.Data;

namespace ModularTreasures.Quests
{
    public class BrainrotTaskHUD : MonoBehaviour
    {
        [SerializeField] private TextMeshPro taskText;
        [SerializeField] private CollectionCategorySO brainrotCategory;
        [SerializeField] private int targetCount = 10;
        [SerializeField] private GameObject panelToDisable;
        [SerializeField] private string disableActionKey = "BrainrotQuest_FocusArrival";

        private void Start()
        {
            if (CollectionManager.Instance != null)
                CollectionManager.Instance.OnItemUnlocked += HandleItemUnlocked;
            
            QuestActionSystem.OnActionTriggered += HandleQuestAction;
            UpdateDisplay();
        }

        private void OnDisable()
        {
            if (CollectionManager.Instance != null)
                CollectionManager.Instance.OnItemUnlocked -= HandleItemUnlocked;

            QuestActionSystem.OnActionTriggered -= HandleQuestAction;
        }

        private void HandleItemUnlocked(string itemId) => UpdateDisplay();

        private void HandleQuestAction(string key, float value)
        {
            if (key == disableActionKey)
            {
                DisableHUD();
            }
        }

        private void UpdateDisplay()
        {
            if (brainrotCategory == null || CollectionManager.Instance == null) return;

            int current = 0;
            foreach (var item in brainrotCategory.Items)
            {
                if (CollectionManager.Instance.IsUnlocked(item.ItemID)) current++;
            }
            
            if (taskText != null)
                taskText.text = $"Найди бреинротов\n{current}/{targetCount}";
        }

        public void DisableHUD()
        {
            if (panelToDisable != null) panelToDisable.SetActive(false);
            else gameObject.SetActive(false);

            if (CameraSequenceManager.Instance != null)
            {
                CameraSequenceManager.Instance.ReleaseCamera();
            }
        }
    }
}