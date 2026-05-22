using System.Collections.Generic;

namespace ModularTreasures.Achievements
{
    public interface IAchievementPersistence
    {
        void SaveProgress(string achievementId, float progress, bool isUnlocked);
        bool LoadProgress(string achievementId, out float progress, out bool isUnlocked);
    }
}
