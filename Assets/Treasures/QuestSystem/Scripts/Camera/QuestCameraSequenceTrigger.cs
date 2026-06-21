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
            Debug.Log($"[QuestCameraSequenceTrigger] Initialize on GameObject '{gameObject.name}'. Target Quest: '{questIdToCheck}', Target Camera: '{cameraTargetId}'");

            yield return null;

            string claimedKey = questIdToCheck.StartsWith("Quest_") ? $"{questIdToCheck}_Claimed" : $"Quest_{questIdToCheck}_Claimed";
            int isClaimed = PlayerPrefs.GetInt(claimedKey, 0);
            Debug.Log($"[QuestCameraSequenceTrigger] Step 1: Checking PlayerPrefs for '{claimedKey}'. Value is: {isClaimed}");

            if (isClaimed != 1)
            {
                Debug.Log($"[QuestCameraSequenceTrigger] Condition failed: Quest '{questIdToCheck}' has NOT been claimed yet. Aborting sequence trigger.");
                yield break;
            }

            int isPlayed = PlayerPrefs.GetInt(PlaySaveKey, 0);
            Debug.Log($"[QuestCameraSequenceTrigger] Step 2: Checking PlayerPrefs for '{PlaySaveKey}'. Value is: {isPlayed}");

            if (isPlayed == 1)
            {
                Debug.Log($"[QuestCameraSequenceTrigger] Condition failed: Sequence for Quest '{questIdToCheck}' was ALREADY played in a past session. Aborting sequence trigger.");
                yield break; 
            }

            if (delayBeforeStart > 0f)
            {
                Debug.Log($"[QuestCameraSequenceTrigger] Step 3: Waiting for delayBeforeStart of {delayBeforeStart} seconds.");
                yield return new WaitForSeconds(delayBeforeStart);
            }

            Debug.Log($"[QuestCameraSequenceTrigger] Step 4: Checking CameraSequenceManager.Instance.");
            if (CameraSequenceManager.Instance == null)
            {
                Debug.LogError("[QuestCameraSequenceTrigger] Error: CameraSequenceManager.Instance is null in this scene! Cannot play camera sequence.");
                yield break;
            }

            Debug.Log($"[QuestCameraSequenceTrigger] Step 5: Getting target '{cameraTargetId}' from CameraSequenceManager.");
            Transform target = CameraSequenceManager.Instance.GetTarget(cameraTargetId);
            if (target == null)
            {
                Debug.LogError($"[QuestCameraSequenceTrigger] Error: Target '{cameraTargetId}' is NOT registered in CameraSequenceManager! Make sure a CameraTarget component with this ID exists and is active in the scene.");
                yield break;
            }

            Debug.Log($"[QuestCameraSequenceTrigger] Step 6: Triggering PlaySequence for target '{cameraTargetId}' (Position: {target.position}). Duration: {focusDuration}, Arrival Action Key: '{arrivalActionKey}', Wait For Manual Release: {waitForManualRelease}");
            CameraSequenceManager.Instance.PlaySequence(target, null, focusDuration, () =>
            {
                Debug.Log($"[QuestCameraSequenceTrigger] Focus arrived! Triggering QuestActionSystem.TriggerAction with key: '{arrivalActionKey}'");
                if (!string.IsNullOrEmpty(arrivalActionKey))
                {
                    QuestActionSystem.TriggerAction(arrivalActionKey, 1f);
                }
            }, waitForManualRelease);

            Debug.Log($"[QuestCameraSequenceTrigger] Step 7: Saving to PlayerPrefs '{PlaySaveKey}' = 1 to mark it as played.");
            PlayerPrefs.SetInt(PlaySaveKey, 1);
            PlayerPrefs.Save();
        }
    }
}