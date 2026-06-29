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
            if (FindObjectsByType<SkinShopManager>().Length > 1)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);

            _persistence = mockShopSave.GetComponent<IShopPersistence>();
            InitializeDefaultSkin();
        }

        private void InitializeDefaultSkin()
        {
            if (Library == null || Library.Skins.Count == 0) return;

            bool anyUnlocked = false;
            foreach (var skin in Library.Skins)
            {
                if (_persistence.IsUnlocked(skin.ID))
                {
                    anyUnlocked = true;
                    break;
                }
            }

            if (!anyUnlocked)
            {
                var firstSkin = Library.Skins[0];
                _persistence.Unlock(firstSkin.ID);
                _persistence.SetActive(firstSkin.ID);
            }
            else
            {
                string activeId = _persistence.GetActiveId();
                if (string.IsNullOrEmpty(activeId) || Library.Skins.Find(s => s.ID == activeId) == null)
                {
                    foreach (var skin in Library.Skins)
                    {
                        if (_persistence.IsUnlocked(skin.ID))
                        {
                            _persistence.SetActive(skin.ID);
                            break;
                        }
                    }
                }
            }
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

        public bool IsUnlocked(string id) => _persistence.IsUnlocked(id);
        public void Unlock(string id) => _persistence.Unlock(id);
        public string GetActiveId() => _persistence.GetActiveId();
        public void SetActive(string id) => _persistence.SetActive(id);

    }
}
