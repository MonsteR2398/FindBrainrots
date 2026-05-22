using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace ModularTreasures.Achievements
{
    public class AchievementNotificationManager : MonoBehaviour
    {
        [SerializeField] private GameObject notificationPrefab;
        [SerializeField] private Transform notificationContainer;
        [SerializeField] private float displayDuration = 3f;

        private void Start()
        {
            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.OnAchievementUnlocked += ShowNotification;
            }
        }

        private void OnDestroy()
        {
            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.OnAchievementUnlocked -= ShowNotification;
            }
        }

        private void ShowNotification(AchievementSO achievement)
        {
            StartCoroutine(NotificationRoutine(achievement));
        }

        private IEnumerator NotificationRoutine(AchievementSO achievement)
        {
            GameObject notif = Instantiate(notificationPrefab, notificationContainer);
            
            // Setup components (assuming hierarchy we created)
            var icon = notif.transform.Find("Icon")?.GetComponent<Image>();
            var text = notif.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();

            if (icon != null) icon.sprite = achievement.Icon;
            if (text != null) text.text = $"Unlocked: {achievement.Title}";

            yield return new WaitForSeconds(displayDuration);

            Destroy(notif);
        }
    }
}
