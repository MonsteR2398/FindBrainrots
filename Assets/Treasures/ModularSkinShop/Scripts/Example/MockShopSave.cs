using UnityEngine;
using ModularSkinShop.Interfaces;
using System.Collections.Generic;

namespace ModularSkinShop.Example
{
    public class MockShopSave : MonoBehaviour, IShopPersistence
    {
        public string DefaultSkinId = "";

        private HashSet<string> _unlockedSkins = new HashSet<string>();
        private string _activeSkinId;

        private void Awake()
        {
            string unlocked = PlayerPrefs.GetString("Shop_Unlocked", DefaultSkinId);
            foreach (var id in unlocked.Split(','))
                if (!string.IsNullOrEmpty(id)) _unlockedSkins.Add(id);

            _activeSkinId = PlayerPrefs.GetString("Shop_Active", DefaultSkinId);
        }

        // IShopPersistence
        public bool IsUnlocked(string id) => _unlockedSkins.Contains(id);
        public void Unlock(string id)
        {
            if (!_unlockedSkins.Contains(id))
            {
                _unlockedSkins.Add(id);
                PlayerPrefs.SetString("Shop_Unlocked", string.Join(",", _unlockedSkins));
            }
        }
        public string GetActiveId() => _activeSkinId;
        public void SetActive(string id)
        {
            _activeSkinId = id;
            PlayerPrefs.SetString("Shop_Active", id);
            Debug.Log($"Skin set to: {id}");
        }
    }
}
