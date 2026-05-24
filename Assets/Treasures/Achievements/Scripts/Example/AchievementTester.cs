using UnityEngine;
using UnityEngine.InputSystem;
using ModularTreasures.Achievements;

namespace ModularTreasures.Achievements
{
    public class AchievementTester : MonoBehaviour
    {
        [SerializeField] private AchievementSO achievement;
        [SerializeField] private GameObject achievementWindow;

        private void Update()
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                if (achievement != null)
                {
                    Debug.Log("Adding 1 progress to achievement...");
                    AchievementManager.Instance.AddProgress(achievement.Id, 1f);
                }
}

            if (Keyboard.current.aKey.wasPressedThisFrame)
            {
                if (achievementWindow != null)
                {
                    achievementWindow.SetActive(!achievementWindow.activeSelf);
                }
            }
        }
    }
}
