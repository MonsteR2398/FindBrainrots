using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ModularTreasures.Quests
{
    public class QuestUIController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Button claimButton;

        [Header("Colors")]
        [SerializeField] private Color activeColor = new Color(0.15f, 0.15f, 0.15f, 0.9f);
        [SerializeField] private Color completedColor = new Color(0.1f, 0.5f, 0.1f, 1f);

        private Vector3 _originalScale;

        private void Awake()
        {
            _originalScale = transform.localScale;
            if (claimButton != null)
            {
                claimButton.onClick.AddListener(OnClaimClicked);
            }
        }

        private void Start()
        {
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnProgressUpdated += UpdateUI;
                QuestManager.Instance.OnQuestCompleted += HandleQuestCompleted;
                QuestManager.Instance.OnQuestStarted += HandleQuestStarted;
                
                UpdateUI(QuestManager.Instance.CurrentQuest, QuestManager.Instance.CurrentProgress);
                if (QuestManager.Instance.Status == QuestStatus.Completed)
                    HandleQuestCompleted(QuestManager.Instance.CurrentQuest);
                else
                    HandleQuestStarted(QuestManager.Instance.CurrentQuest);
            }
        }

        private void OnDisable()
        {
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnProgressUpdated -= UpdateUI;
                QuestManager.Instance.OnQuestCompleted -= HandleQuestCompleted;
                QuestManager.Instance.OnQuestStarted -= HandleQuestStarted;
            }
        }

        private void Update()
        {
            if (QuestManager.Instance != null && QuestManager.Instance.Status == QuestStatus.Completed)
            {
                float scale = 1f + Mathf.Sin(Time.time * 2f) * 0.05f;
                transform.localScale = _originalScale * scale;
            }
            else
            {
                transform.localScale = _originalScale;
            }
        }

        private void HandleQuestStarted(QuestSO quest)
        {
            if (quest == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
            titleText.text = quest.Title;
            backgroundImage.color = activeColor;
            if (claimButton != null) claimButton.interactable = false;
        }

        private void HandleQuestCompleted(QuestSO quest)
        {
            backgroundImage.color = completedColor;
            if (claimButton != null) claimButton.interactable = true;
        }

        private void UpdateUI(QuestSO quest, float currentProgress)
        {
            if (quest == null) return;

            progressSlider.maxValue = quest.TargetValue;
            progressSlider.value = currentProgress;
            progressText.text = $"{(int)currentProgress} / {(int)quest.TargetValue}";
        }

        private void OnClaimClicked()
        {
            QuestManager.Instance.ClaimReward();
        }
    }
}
