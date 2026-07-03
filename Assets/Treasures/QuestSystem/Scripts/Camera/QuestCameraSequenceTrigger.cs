using UnityEngine;
using System.Collections;

namespace ModularTreasures.Quests
{
    public class QuestCameraSequenceTrigger : MonoBehaviour
    {
        [Header("Quest Settings")]
        [Tooltip("ID квеста, сдачу которого нужно отслеживать.")]
        [SerializeField] private string questIdToCheck;

        [Header("Camera Sequence Settings")]
        [Tooltip("ID цели для камеры, зарегистрированной в CameraSequenceManager.")]
        [SerializeField] private string cameraTargetId = "CameraTarget";
        
        [Tooltip("Задержка перед стартом после загрузки сцены (чтобы сцена успела полностью прогрузиться).")]
        [SerializeField] private float delayBeforeStart = 1f;
        
        [SerializeField] private float focusDuration = 3f;
        [SerializeField] private string arrivalActionKey = "CameraArrived";
        [SerializeField] private bool waitForManualRelease = false;

        private string PlaySaveKey => $"Quest_{questIdToCheck}_CameraSequencePlayed";

        private IEnumerator Start()
        {
            yield return null;

            string claimedKey = questIdToCheck.StartsWith("Quest_") ? $"{questIdToCheck}_Claimed" : $"Quest_{questIdToCheck}_Claimed";
            int isClaimed = PlayerPrefs.GetInt(claimedKey, 0);

            if (isClaimed != 1)
            {
                // Debug.Log($"[QuestCameraSequenceTrigger] Condition failed: Quest '{questIdToCheck}' has NOT been claimed yet. Aborting sequence trigger.");
                yield break;
            }

            int isPlayed = PlayerPrefs.GetInt(PlaySaveKey, 0);

            if (isPlayed == 1)
            {
                // Debug.Log($"[QuestCameraSequenceTrigger] Condition failed: Sequence for Quest '{questIdToCheck}' was ALREADY played in a past session. Aborting sequence trigger.");
                yield break; 
            }

            if (delayBeforeStart > 0f)
            {
                yield return new WaitForSeconds(delayBeforeStart);
            }

            if (CameraSequenceManager.Instance == null)
            {
                Debug.LogError("[QuestCameraSequenceTrigger] Error: CameraSequenceManager.Instance is null in this scene! Cannot play camera sequence.");
                yield break;
            }

            Transform target = CameraSequenceManager.Instance.GetTarget(cameraTargetId);
            if (target == null)
            {
                Debug.LogError($"[QuestCameraSequenceTrigger] Error: Target '{cameraTargetId}' is NOT registered in CameraSequenceManager! Make sure a CameraTarget component with this ID exists and is active in the scene.");
                yield break;
            }

            CameraSequenceManager.Instance.PlaySequence(target, null, focusDuration, () =>
            {
                // Debug.Log($"[QuestCameraSequenceTrigger] Focus arrived! Triggering QuestActionSystem.TriggerAction with keys: '{arrivalActionKey}'");
                if (!string.IsNullOrEmpty(arrivalActionKey))
                {
                    string[] keys = arrivalActionKey.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                    foreach (var key in keys)
                    {
                        QuestActionSystem.TriggerAction(key.Trim(), 1f);
                    }
                }
            }, waitForManualRelease);

            PlayerPrefs.SetInt(PlaySaveKey, 1);
            PlayerPrefs.Save();
        }
    }
}