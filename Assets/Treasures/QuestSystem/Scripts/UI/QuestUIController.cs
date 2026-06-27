using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Treasures.Localization;

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
        [SerializeField] private Image IconImage;
        [SerializeField] private Image FingerImage;

        [Header("Settings")]
        [SerializeField] private NumberFormatMode formatMode = NumberFormatMode.Abbreviated;

        [Header("Colors")]
        [SerializeField] private Color activeColor = new Color(0.15f, 0.15f, 0.15f, 0.9f);
        [SerializeField] private Color completedColor = new Color(0.1f, 0.5f, 0.1f, 1f);

        private RectTransform panelParent;
        private Vector3 _originalScale;

        private void Awake()
        {
            if (claimButton != null)
            {
                claimButton.onClick.AddListener(() => OnClaimClicked(claimButton.transform));
            }
        }

        private void OnEnable()
        {
            Localization.LanguageChanged += RefreshLocalizedText;

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

        private void Start()
        {
            panelParent = backgroundImage.rectTransform;
            _originalScale = panelParent.localScale;
        }

        private void OnDisable()
        {
            Localization.LanguageChanged -= RefreshLocalizedText;

            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnProgressUpdated -= UpdateUI;
                QuestManager.Instance.OnQuestCompleted -= HandleQuestCompleted;
                QuestManager.Instance.OnQuestStarted -= HandleQuestStarted;
            }
        }

        private void RefreshLocalizedText()
        {
            if (QuestManager.Instance != null && QuestManager.Instance.CurrentQuest != null)
            {
                titleText.text = QuestManager.Instance.CurrentQuest.Title;
            }
        }

        private void Update()
        {
            if (QuestManager.Instance != null && QuestManager.Instance.Status == QuestStatus.Completed)
            {
                float scale = 1f + Mathf.Sin(Time.time * 2f) * 0.05f;
               panelParent.localScale = _originalScale * scale;
               FingerImage.enabled = true;
            }
            else
            {
                panelParent.localScale = _originalScale;
               FingerImage.enabled = false;
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
            IconImage.sprite = quest.Icon;
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

            string currentStr = NumberFormatter.Format(currentProgress, formatMode);
            string targetStr = NumberFormatter.Format(quest.TargetValue, formatMode);
            progressText.text = $"{currentStr} / {targetStr}";
        }

        private void OnClaimClicked(Transform targetPos)
        {
            QuestManager.Instance.ClaimReward(targetPos);
        }
    }
}
