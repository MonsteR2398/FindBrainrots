using UnityEngine;

namespace ModularTreasures.Achievements
{
    public class PlayerPrefsAchievementPersistence : IAchievementPersistence
    {
        private const string ProgressKey = "Achievement_Progress_";
        private const string UnlockedKey = "Achievement_Unlocked_";

        public void SaveProgress(string achievementId, float progress, bool isUnlocked)
        {
            PlayerPrefs.SetFloat(ProgressKey + achievementId, progress);
            PlayerPrefs.SetInt(UnlockedKey + achievementId, isUnlocked ? 1 : 0);
            PlayerPrefs.Save();
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
