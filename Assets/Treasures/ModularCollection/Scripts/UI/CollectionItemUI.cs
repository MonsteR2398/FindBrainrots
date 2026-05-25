using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ModularCollection.Data;
using ModularCollection.Core;

namespace ModularCollection.UI
{
    public class CollectionItemUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI rarityText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private GameObject newBadge;
        [SerializeField] private Button button;

        [Header("Settings")]
        [SerializeField] private Sprite lockedIcon;
        [SerializeField] private string lockedName = "???";

        private CollectibleItemSO currentItem;

        public void Setup(CollectibleItemSO item, bool isUnlocked, bool isRead)
        {
            currentItem = item;
            
            if (isUnlocked)
            {
                iconImage.sprite = item.Icon;
                nameText.text = item.ItemName;
                rarityText.text = item.Rarity.DisplayName;
                backgroundImage.color = item.Rarity.RarityColor;
                newBadge.SetActive(!isRead);
            }
            else
            {
                iconImage.sprite = lockedIcon;
                nameText.text = lockedName;
                rarityText.text = "";
                backgroundImage.color = Color.gray;
                newBadge.SetActive(false);
            }

            button.onClick.RemoveAllListeners();
            if (isUnlocked && !isRead)
            {
                button.onClick.AddListener(OnClick);
            }
        }

        private void OnClick()
        {
            CollectionManager.Instance.MarkAsRead(currentItem.ItemID);
            newBadge.SetActive(false);
            button.onClick.RemoveListener(OnClick);
        }
    }
}
