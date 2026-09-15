using Treasures.Services;
using UnityEngine;
using PlayerPrefs = RedefineYG.PlayerPrefs;

namespace ModularTreasures.Achievements
{
    /// <summary>
    /// Achievement persistence backed by the PluginYourGames "Storage" module: all values are
    /// written into <c>YG2.saves</c> (cloud save) through the plugin's PlayerPrefs override.
    /// </summary>
    public class PlayerPrefsAchievementPersistence : IAchievementPersistence
    {
        private const string ProgressKey = "Achievement_Progress_";
        private const string UnlockedKey = "Achievement_Unlocked_";

        public void SaveProgress(string achievementId, float progress, bool isUnlocked)
        {
            PlayerPrefs.SetFloat(ProgressKey + achievementId, progress);
            PlayerPrefs.SetInt(UnlockedKey + achievementId, isUnlocked ? 1 : 0);
            CloudSaves.Save();
        }

        public bool LoadProgress(string achievementId, out float progress, out bool isUnlocked)
        {
            if (PlayerPrefs.HasKey(ProgressKey + achievementId))
            {
                progress = PlayerPrefs.GetFloat(ProgressKey + achievementId);
                isUnlocked = PlayerPrefs.GetInt(UnlockedKey + achievementId) == 1;
                return true;
            }

            progress = 0;
            isUnlocked = false;
            return false;
        }
    }
}
