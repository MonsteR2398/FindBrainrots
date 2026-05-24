using System;
using UnityEngine;
using ModularSkinShop.Data;
using ModularSkinShop.Interfaces;
using ModularSkinShop.Example;
using Treasures.CurrencySystem;

namespace ModularSkinShop.Core
{
    public class SkinShopManager : MonoBehaviour
    {
        [Header("Data")]
        public SkinLibrarySO Library;

        private IShopPersistence _persistence;

        public event Action<SkinSO> OnSkinDressed;
        public event Action<SkinSO> OnSkinSelect;
        public event Action<SkinSO> OnSkinPurchased;
        public event Action OnBalanceChanged;

        [Header("MockSave - Обязательно заменить на общую систему сохранений")]
        public MockShopSave mockShopSave;

        private void Awake() {
            _persistence = mockShopSave.GetComponent<IShopPersistence>();
        }

        public bool IsSkinUnlocked(SkinSO skin) => _persistence.IsUnlocked(skin.ID);
        public bool IsSkinActive(SkinSO skin) => _persistence.GetActiveId() == skin.ID;

        public void TryDressOrBuy(SkinSO skin, CurrencyValue currencyValue)
        {
            if (IsSkinUnlocked(skin))
                DressSkin(skin);
            else
                TryBuySkin(skin, currencyValue);

            TrySelect(skin);
        }

        public void TryDress(SkinSO skin)
        {
            if (IsSkinUnlocked(skin))
                DressSkin(skin);
                
            TrySelect(skin);
        }

        private void TrySelect(SkinSO skin)
        {
            SelectSkin(skin);
        }

        private void SelectSkin(SkinSO skin)
        {
            OnSkinSelect?.Invoke(skin);
        }

        private void DressSkin(SkinSO skin)
        {
            _persistence.SetActive(skin.ID);
            OnSkinDressed?.Invoke(skin);
        }

        private void TryBuySkin(SkinSO skin, CurrencyValue currency)
        {
            if (CurrencyService.Instance.TrySpend(currency.Type, currency.Value))
            {
                _persistence.Unlock(skin.ID);
                
                OnSkinPurchased?.Invoke(skin);
                OnBalanceChanged?.Invoke();
                
                DressSkin(skin);
            }
        }

        public bool IsUnlocked(string id)
        {
            throw new NotImplementedException();
        }

        public void Unlock(string id)
        {
            throw new NotImplementedException();
        }

        public string GetActiveId()
        {
            throw new NotImplementedException();
        }

        public void SetActive(string id)
        {
            throw new NotImplementedException();
        }

    }
}
