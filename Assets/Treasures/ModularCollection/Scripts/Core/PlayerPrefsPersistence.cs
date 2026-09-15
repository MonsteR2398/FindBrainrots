using System.Collections.Generic;
using Treasures.Services;
using UnityEngine;
using PlayerPrefs = RedefineYG.PlayerPrefs;

namespace ModularCollection.Core
{
    /// <summary>
    /// Collection persistence backed by the PluginYourGames "Storage" module: the unlocked / read
    /// item ids are stored in <c>YG2.saves</c> (cloud save) through the plugin's PlayerPrefs
    /// override.
    /// </summary>
    public class PlayerPrefsPersistence : ICollectionPersistence
    {
        private const string UnlockedKey = "Collection_Unlocked_IDs";
        private const string ReadKey = "Collection_Read_IDs";
        private const char Separator = '|';

        public void SaveUnlockedItems(HashSet<string> unlockedIds)
        {
            PlayerPrefs.SetString(UnlockedKey, string.Join(Separator.ToString(), unlockedIds));
            CloudSaves.Save();
        }

        public HashSet<string> LoadUnlockedItems()
        {
            string data = PlayerPrefs.GetString(UnlockedKey, "");
            if (string.IsNullOrEmpty(data)) return new HashSet<string>();
            return new HashSet<string>(data.Split(Separator));
        }

        public void SaveReadItems(HashSet<string> readIds)
        {
            PlayerPrefs.SetString(ReadKey, string.Join(Separator.ToString(), readIds));
            CloudSaves.Save();
        }

        public HashSet<string> LoadReadItems()
        {
            string data = PlayerPrefs.GetString(ReadKey, "");
            if (string.IsNullOrEmpty(data)) return new HashSet<string>();
            return new HashSet<string>(data.Split(Separator));
        }
    }
}
