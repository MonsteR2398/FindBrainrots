using UnityEngine;
using ModularSkinShop.Interfaces;
using System.Collections.Generic;

namespace ModularSkinShop.Example
{
    public class MockShopSave : MonoBehaviour, IShopPersistence
    {
        public string DefaultSkinId = "";

        private HashSet<string> _unlockedSkins;
        private string _activeSkinId;
        private bool _isLoaded = false;

        private void Awake()
        {
            EnsureLoaded();
        }

        private void EnsureLoaded()
        {
            if (_isLoaded) return;
            
            _unlockedSkins = new HashSet<string>();
            string unlocked = PlayerPrefs.GetString("Shop_Unlocked", DefaultSkinId);
            foreach (var id in unlocked.Split(','))
                if (!string.IsNullOrEmpty(id)) _unlockedSkins.Add(id);

            _activeSkinId = PlayerPrefs.GetString("Shop_Active", DefaultSkinId);
            _isLoaded = true;
        }

        // IShopPersistence
        public bool IsUnlocked(string id)
        {
            EnsureLoaded();
            return _unlockedSkins.Contains(id);
        }

        public void Unlock(string id)
        {
            EnsureLoaded();
            if (!_unlockedSkins.Contains(id))
            {
                _unlockedSkins.Add(id);
                PlayerPrefs.SetString("Shop_Unlocked", string.Join(",", _unlockedSkins));
                PlayerPrefs.Save();
            }
        }

        public string GetActiveId()
        {
            EnsureLoaded();
            return _activeSkinId;
        }

        public void SetActive(string id)
        {
            EnsureLoaded();
            _activeSkinId = id;
            PlayerPrefs.SetString("Shop_Active", id);
            PlayerPrefs.Save();
            Debug.Log($"Skin set to: {id}");
        }
    }
}
