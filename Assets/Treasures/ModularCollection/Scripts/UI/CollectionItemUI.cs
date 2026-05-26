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
                backgroundImage.sprite = item.Rarity.CardSprite;
                newBadge.SetActive(!isRead);
                iconImage.gameObject.SetActive(true);
            }
            else
            {
                if(lockedIcon != null)
                    iconImage.sprite = lockedIcon;
                else
                    iconImage.gameObject.SetActive(false);
                nameText.text = lockedName;
                rarityText.text = "";
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
