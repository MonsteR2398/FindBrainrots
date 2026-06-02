using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModularTreasures.Quests
{
    public enum QuestStatus
    {
        InProgress,
        Completed
    }

    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private QuestCollectionSO questDatabase;
        [SerializeField] private bool autoLoadNextOnClaim = true;

        public event Action<QuestSO, float> OnProgressUpdated;
        public event Action<QuestSO> OnQuestCompleted;
        public event Action<QuestSO> OnQuestStarted;

        private List<string> _questIdQueue = new List<string>();
        private float _currentProgress = 0f;
        private QuestStatus _status = QuestStatus.InProgress;

        private QuestConditionSO _activeCondition;

        private const string SAVE_KEY_QUEUE = "QuestSystem_Queue";
        private const string SAVE_KEY_PROGRESS = "QuestSystem_Progress";
        private const string SAVE_KEY_STATUS = "QuestSystem_Status";

        public QuestSO CurrentQuest => GetQuestById(_questIdQueue.FirstOrDefault());
        public float CurrentProgress => _currentProgress;
        public QuestStatus Status => _status;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadProgress();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            StartQuest(CurrentQuest);
        }

        private void StartQuest(QuestSO quest)
        {
            if (quest == null)
            {
                CleanupCondition();
                OnQuestStarted?.Invoke(null);
                return;
            }

            if (_activeCondition != null)
            {
                CleanupCondition();
            }

            if (_status == QuestStatus.InProgress && quest.Condition != null)
            {
                _activeCondition = Instantiate(quest.Condition);
                _activeCondition.Initialize(this);
            }

            OnQuestStarted?.Invoke(quest);
            NotifyProgress();
        }

        public void AddProgress(float amount)
        {
            if (_status != QuestStatus.InProgress || CurrentQuest == null) return;

            _currentProgress += amount;
            _currentProgress = Mathf.Clamp(_currentProgress, 0f, CurrentQuest.TargetValue);

            NotifyProgress();
            SaveProgress();

            if (_currentProgress >= CurrentQuest.TargetValue)
            {
                CompleteQuest();
            }
        }

        private void CompleteQuest()
        {
            _status = QuestStatus.Completed;
            CleanupCondition();
            SaveProgress();
            OnQuestCompleted?.Invoke(CurrentQuest);
        }

        public void ClaimReward(Transform targetPos)
        {
            if (_status != QuestStatus.Completed || CurrentQuest == null) return;
            QuestSO finishedQuest = CurrentQuest;

            foreach (var reward in finishedQuest.Rewards)
            {
                if (reward != null) reward.GiveReward(targetPos);
            }

            if (autoLoadNextOnClaim)
            {
                LoadNextQuest(finishedQuest);
            }
        }

        private void LoadNextQuest(QuestSO finishedQuest)
        {
            if (_questIdQueue.Count > 0)
            {
                _questIdQueue.RemoveAt(0);
            }

            if (finishedQuest.IsRepeatable)
            {
                _questIdQueue.Add(finishedQuest.Id);
            }

            _currentProgress = 0f;
            if (CurrentQuest != null && CurrentQuest.StartFromCurrentValue && CurrentQuest.Condition != null)
            {
                _currentProgress = CurrentQuest.Condition.GetCurrentValue();
            }

            _status = QuestStatus.InProgress;
            
            SaveProgress();
            StartQuest(CurrentQuest);
        }

        private void CleanupCondition()
        {
            if (_activeCondition != null)
            {
                _activeCondition.Shutdown();
                _activeCondition = null;
            }
        }

        private void NotifyProgress()
        {
            OnProgressUpdated?.Invoke(CurrentQuest, _currentProgress);
        }

        private QuestSO GetQuestById(string id)
        {
            if (string.IsNullOrEmpty(id) || questDatabase == null) return null;
            return questDatabase.Quests.FirstOrDefault(q => q.Id == id);
        }

        #region Persistence

        private void SaveProgress()
        {
            string queueData = string.Join(",", _questIdQueue);
            PlayerPrefs.SetString(SAVE_KEY_QUEUE, queueData);
            PlayerPrefs.SetFloat(SAVE_KEY_PROGRESS, _currentProgress);
            PlayerPrefs.SetInt(SAVE_KEY_STATUS, (int)_status);
            PlayerPrefs.Save();
        }

        private void LoadProgress()
        {
            bool hasSavedProgress = PlayerPrefs.HasKey(SAVE_KEY_PROGRESS);
            _currentProgress = PlayerPrefs.GetFloat(SAVE_KEY_PROGRESS, 0f);
            _status = (QuestStatus)PlayerPrefs.GetInt(SAVE_KEY_STATUS, (int)QuestStatus.InProgress);

            string queueData = PlayerPrefs.GetString(SAVE_KEY_QUEUE, "");
            if (!string.IsNullOrEmpty(queueData))
            {
                _questIdQueue = queueData.Split(',').ToList();
            }
            else if (questDatabase != null)
            {
                // First time initialization
                _questIdQueue = questDatabase.Quests.Select(q => q.Id).ToList();

                if (!hasSavedProgress && CurrentQuest != null && CurrentQuest.StartFromCurrentValue && CurrentQuest.Condition != null)
                {
                    _currentProgress = CurrentQuest.Condition.GetCurrentValue();
                }
            }
        }

        #endregion
    }
}
