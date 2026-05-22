using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ModularSkinShop.Data;
using ModularSkinShop.Core;
using Treasures.CurrencySystem;

namespace ModularSkinShop.UI
{
    public class SkinShopItemUI : MonoBehaviour
    {
        [Header("References")]
        public Image IconImage;
        public TextMeshProUGUI NameText;
        public TextMeshProUGUI PriceText;
        public Button SeletButton;
        public Button BuyButton;

        [Header("Visual")]
        public Image buyImage;
        public Sprite buySprite;
        public Sprite noEnoughCurrencySprite;
        public Sprite unlockedSprite;
        public Sprite selectedSprite;

        private SkinSO _skin;
        private SkinShopManager _manager;

        public void Setup(SkinSO skin, SkinShopManager manager)
        {
            _skin = skin;
            _manager = manager;

            IconImage.sprite = skin.Icon;
            NameText.text = skin.DisplayName;

            UpdateUI();

            SeletButton.onClick.RemoveAllListeners();
            SeletButton.onClick.AddListener(OnSelectClicked);

            BuyButton.onClick.RemoveAllListeners();
            BuyButton.onClick.AddListener(OnBuyClicked);
        }

        public void UpdateUI()
        {
            if (_manager.IsSkinActive(_skin))
            {
                PriceText.text = "Active";
                BuyButton.interactable = false;
                if(buyImage != null)
                    if(unlockedSprite != null)
                        buyImage.sprite = unlockedSprite;
            }
            else if (_manager.IsSkinUnlocked(_skin))
            {
                PriceText.text = "Select";
                BuyButton.interactable = true;
                if(buyImage != null)
                    if(selectedSprite != null)
                        buyImage.sprite = selectedSprite;
            }
            else
            {
                if(buyImage != null)
                {
                    if(CurrencyService.Instance.GetBalance(CurrencyType.Gold) >= _skin.Price)
                    {
                        if(buySprite != null)
                            buyImage.sprite = buySprite;
                    }
                    else
                    {
                       if(noEnoughCurrencySprite != null)
                            buyImage.sprite = noEnoughCurrencySprite; 
                    }
                        
                }

                PriceText.text = $"Buy: {_skin.Price}";
                BuyButton.interactable = true;
            }
        }

        private void OnSelectClicked() => _manager.TryDress(_skin);
        private void OnBuyClicked() => _manager.TryDressOrBuy(_skin);
            
    }
}
