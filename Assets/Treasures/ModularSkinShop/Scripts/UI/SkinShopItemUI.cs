using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ModularSkinShop.Data;
using ModularSkinShop.Core;
using Treasures.CurrencySystem;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace ModularSkinShop.UI
{
    public class SkinShopItemUI : MonoBehaviour
    {
        [Header("References")]
        public Image IconImage;
        public TextMeshProUGUI NameText;
        public TextMeshProUGUI InteractionText;
        public Button SeletButton;
        public Button InteractionButton;

        [Header("Visual")]
        public Sprite buySprite;
        public Sprite noEnoughCurrencySprite;
        public Sprite unlockedSprite;
        public Sprite selectedSprite;

        private SkinSO _skin;
        private SkinShopManager _manager;

        private List<Button> _buyButtons = new List<Button>();

        public void Setup(SkinSO skin, SkinShopManager manager)
        {
            _skin = skin;
            _manager = manager;

            IconImage.sprite = skin.Icon;
            NameText.text = skin.DisplayName;
            _buyButtons.Clear();
            InteractionButton.gameObject.SetActive(false);

            if (_skin.CurrencyPrice != null)
            {
                for (int i = 0; i < _skin.CurrencyPrice.Length; i++)
                {
                    int capturedIndex = i;
                    Button buyButton = Instantiate(InteractionButton, InteractionButton.transform.parent);
                    buyButton.gameObject.SetActive(true);
                    buyButton.onClick.RemoveAllListeners();
                    buyButton.onClick.AddListener(() => OnBuyClickedWithPrice(capturedIndex));
                    _buyButtons.Add(buyButton);
                }
            }
            UpdateUI();
            SeletButton.onClick.RemoveAllListeners();
            SeletButton.onClick.AddListener(OnSelectClicked);
        }

        public void UpdateUI()
        {

            if (_manager.IsSkinActive(_skin))
            {
                foreach (var item in _buyButtons)
                    item.gameObject.SetActive(false);

                InteractionText.text = "Active";
                InteractionButton.gameObject.SetActive(true);
                InteractionButton.interactable = false;
                if(InteractionButton.TryGetComponent(out Image image))
                    if(unlockedSprite != null)
                        image.sprite = unlockedSprite;
            }
            else if (_manager.IsSkinUnlocked(_skin))
            {
                foreach (var item in _buyButtons)
                    item.gameObject.SetActive(false);
                

                InteractionText.text = "Select";
                InteractionButton.gameObject.SetActive(true);
                InteractionButton.interactable = true;
                if(InteractionButton.TryGetComponent(out Image image))
                    if(selectedSprite != null)
                        image.sprite = selectedSprite;
            }
            else
            {
                for (int i = 0; i < _buyButtons.Count; i++)
                {
                        if(CurrencyService.Instance.GetBalance(_skin.CurrencyPrice[i].Type) >= _skin.CurrencyPrice[i].Value)
                        {
                            if(buySprite != null)
                                _buyButtons[i].GetComponent<Image>().sprite = buySprite;
                        }
                        else
                        {
                           if(noEnoughCurrencySprite != null)
                                _buyButtons[i].GetComponent<Image>().sprite = noEnoughCurrencySprite; 
                        }
                    _buyButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = $"{_skin.CurrencyPrice[i].Value}";
                    _buyButtons[i].transform.Find("Icon").GetComponent<Image>().sprite = CurrencyService.Instance.Database.GetDefinition(_skin.CurrencyPrice[i].Type).Icon;
                } 
                InteractionButton.gameObject.SetActive(false);
            }
        }

        private void OnSelectClicked() => _manager.TryDress(_skin);
        private void OnBuyClickedWithPrice(int priceIndex) => _manager.TryDressOrBuy(_skin, _skin.CurrencyPrice[priceIndex]);
            
    }
}