using System;
using System.Collections.Generic;
using Treasures.Services;
using UnityEngine;

namespace ModularTreasures.Achievements
{
    public class AchievementManager : MonoBehaviour
    {
        public static AchievementManager Instance { get; private set; }

        public event Action<AchievementSO> OnAchievementUnlocked;
        public event Action<string, float> OnProgressUpdated;

        private Dictionary<string, float> _progress = new Dictionary<string, float>();
        private HashSet<string> _unlockedUniqueIds = new HashSet<string>();
        
        [Header("Database")]
        [SerializeField] private List<AchievementCategorySO> categories = new List<AchievementCategorySO>();
        private Dictionary<string, List<AchievementSO>> _definitions = new Dictionary<string, List<AchievementSO>>();

        public List<AchievementCategorySO> Categories => categories;

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
            InitializeDefinitions();

            // The cloud save is applied asynchronously - drop the cached values when it arrives so
            // they are read again from the cloud, otherwise the next progress write would push the
            // pre-load values back to the cloud.
            CloudSaves.Subscribe(ClearCache);
        }

        private void OnDestroy()
        {
            CloudSaves.Unsubscribe(ClearCache);
        }

        /// <summary>Forces the next <see cref="GetProgress"/> / <see cref="IsUnlocked"/> call to read
        /// the values from the (cloud) save again.</summary>
        private void ClearCache()
        {
            _progress.Clear();
            _unlockedUniqueIds.Clear();
        }

        private void InitializeDefinitions()
        {
            _definitions.Clear();
            foreach (var category in categories)
            {
                if (category == null) continue;
                foreach (var ach in category.Achievements)
                {
                    if (ach == null) continue;
                    if (!_definitions.ContainsKey(ach.Id))
                    {
                        _definitions.Add(ach.Id, new List<AchievementSO>());
                    }
                    _definitions[ach.Id].Add(ach);
                }
            }
        }

        public void AddProgress(string achievementId, float amount)
        {
            if (!_definitions.ContainsKey(achievementId))
            {
                Debug.LogWarning($"Achievement with ID {achievementId} not found in database.");
                return;
            }

            float currentProgress = GetProgress(achievementId);
            currentProgress += amount;
            
            _progress[achievementId] = currentProgress;
            OnProgressUpdated?.Invoke(achievementId, currentProgress);

            ModularTreasures.Quests.QuestActionSystem.TriggerAction(achievementId + "Add", amount);

            _persistence.SaveProgress(achievementId, currentProgress, false);

            foreach (var tier in _definitions[achievementId])
            {
                if (IsUnlocked(tier)) continue;

                if (currentProgress >= tier.TargetValue)
                {
                    UnlockAchievement(tier);
                }
            }
        }

        private void UnlockAchievement(AchievementSO definition)
        {
            string uniqueKey = GetUniqueKey(definition);
            if (_unlockedUniqueIds.Contains(uniqueKey)) return;

            _unlockedUniqueIds.Add(uniqueKey);
            _persistence.SaveProgress(uniqueKey, definition.TargetValue, true);
            
            OnAchievementUnlocked?.Invoke(definition);
            Debug.Log($"Achievement Unlocked: {definition.Title}");
        }

        public float GetProgress(string achievementId)
        {
            if (_progress.TryGetValue(achievementId, out float val)) return val;

            if (_persistence.LoadProgress(achievementId, out float progress, out _))
            {
                _progress[achievementId] = progress;
                return progress;
            }

            return 0;
        }

        public bool IsUnlocked(AchievementSO definition)
        {
            string uniqueKey = GetUniqueKey(definition);
            if (_unlockedUniqueIds.Contains(uniqueKey)) return true;

            if (_persistence.LoadProgress(uniqueKey, out _, out bool unlocked))
            {
                if (unlocked) _unlockedUniqueIds.Add(uniqueKey);
                return unlocked;
            }

            return false;
        }

        public bool IsUnlocked(string achievementId)
        {
            if (!_definitions.TryGetValue(achievementId, out var tiers)) return false;
            foreach (var tier in tiers)
            {
                if (!IsUnlocked(tier)) return false;
            }
            return true;
        }

        private string GetUniqueKey(AchievementSO definition)
        {
            return $"{definition.Id}_{definition.TargetValue}";
        }
}
}
