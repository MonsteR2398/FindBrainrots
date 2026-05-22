using UnityEngine;
using System;
using System.Collections.Generic;

namespace ModularTreasures.Achievements
{
    public class AchievementManager : MonoBehaviour
    {
        public static AchievementManager Instance { get; private set; }

        public event Action<AchievementSO> OnAchievementUnlocked;
        public event Action<string, float> OnProgressUpdated;

        private Dictionary<string, float> _progress = new Dictionary<string, float>();
        private HashSet<string> _unlockedIds = new HashSet<string>();
        
        private IAchievementPersistence _persistence;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Initialize()
        {
            _persistence = new PlayerPrefsAchievementPersistence();
        }

        public void AddProgress(string achievementId, float amount, AchievementSO definition)
        {
            if (_unlockedIds.Contains(achievementId)) return;

            float currentProgress = GetProgress(achievementId);
            currentProgress += amount;
            
            _progress[achievementId] = currentProgress;
            OnProgressUpdated?.Invoke(achievementId, currentProgress);

            if (currentProgress >= definition.TargetValue)
            {
                UnlockAchievement(definition);
            }
            else
            {
                _persistence.SaveProgress(achievementId, currentProgress, false);
            }
        }

        private void UnlockAchievement(AchievementSO definition)
        {
            if (_unlockedIds.Contains(definition.Id)) return;

            _unlockedIds.Add(definition.Id);
            _persistence.SaveProgress(definition.Id, definition.TargetValue, true);
            
            OnAchievementUnlocked?.Invoke(definition);
            Debug.Log($"Achievement Unlocked: {definition.Title}");
        }

        public float GetProgress(string achievementId)
        {
            if (_progress.TryGetValue(achievementId, out float val)) return val;

            if (_persistence.LoadProgress(achievementId, out float progress, out bool unlocked))
            {
                _progress[achievementId] = progress;
                if (unlocked) _unlockedIds.Add(achievementId);
                return progress;
            }

            return 0;
        }

        public bool IsUnlocked(string achievementId)
        {
            if (_unlockedIds.Contains(achievementId)) return true;

            if (_persistence.LoadProgress(achievementId, out _, out bool unlocked))
            {
                if (unlocked) _unlockedIds.Add(achievementId);
                return unlocked;
            }

            return false;
        }
    }
}
