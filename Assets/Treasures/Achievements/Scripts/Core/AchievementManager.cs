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

            // Save shared progress for this ID
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
            // Save unlock state using unique key
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
            // For backward compatibility or general checks, return true if ALL tiers for this ID are unlocked
            if (!_definitions.TryGetValue(achievementId, out var tiers)) return false;
            foreach (var tier in tiers)
            {
                if (!IsUnlocked(tier)) return false;
            }
            return true;
        }

        private string GetUniqueKey(AchievementSO definition)
        {
            // Use asset name or a combination of ID and Target to ensure uniqueness among tiers
            return $"{definition.Id}_{definition.TargetValue}";
        }
}
}
