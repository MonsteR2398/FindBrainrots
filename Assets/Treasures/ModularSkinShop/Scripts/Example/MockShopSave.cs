using System.Collections.Generic;
using ModularSkinShop.Interfaces;
using Treasures.Services;
using UnityEngine;
using PlayerPrefs = RedefineYG.PlayerPrefs;

namespace ModularSkinShop.Example
{
    /// <summary>
    /// Skin shop persistence backed by the PluginYourGames "Storage" module: the unlocked skins and
    /// the active skin are stored in <c>YG2.saves</c> (cloud save) through the plugin's PlayerPrefs
    /// override.
    /// </summary>
    public class MockShopSave : MonoBehaviour, IShopPersistence
    {
        public string DefaultSkinId = "";

        private HashSet<string> _unlockedSkins;
        private string _activeSkinId;
        private bool _isLoaded = false;

        private void Awake()
        {
            EnsureLoaded();

            // Cloud data arrives asynchronously - re-read it so the next write cannot push the
            // pre-load defaults back to the cloud.
            CloudSaves.Subscribe(Reload);
        }

        private void OnDestroy()
        {
            CloudSaves.Unsubscribe(Reload);
        }

        private void Reload()
        {
            _isLoaded = false;
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
                CloudSaves.Save();
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
            CloudSaves.Save();
            Debug.Log($"Skin set to: {id}");
        }
    }
}
