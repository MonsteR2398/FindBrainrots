using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ModularCollection.Data;
using System;

namespace ModularCollection.UI
{
    public class RarityFilterButtonUI : MonoBehaviour
    {
        [SerializeField] private Image buttonImage;
        [SerializeField] private TextMeshProUGUI labelText;
        [SerializeField] private GameObject newIndicator;
        [SerializeField] private Button button;

        private RaritySettingsSO rarity;
        private Action<RaritySettingsSO> onClickCallback;

        public void Setup(RaritySettingsSO rarity, Action<RaritySettingsSO> onClick, bool hasNew)
        {
            this.rarity = rarity;
            this.onClickCallback = onClick;
            
            labelText.text = rarity.DisplayName;
            buttonImage.sprite = rarity.ButtonSprite;
            newIndicator.SetActive(hasNew);

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClickCallback?.Invoke(rarity));
        }

        public void SetNewIndicator(bool hasNew)
        {
            newIndicator.SetActive(hasNew);
        }
    }
}
