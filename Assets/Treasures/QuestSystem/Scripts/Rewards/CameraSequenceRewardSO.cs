using UnityEngine;

namespace ModularTreasures.Quests
{
    [CreateAssetMenu(fileName = "Camera Sequence Reward", menuName = "Quests/Rewards/Camera Sequence")]
    public class CameraSequenceRewardSO : QuestRewardSO
    {
        [SerializeField] private string targetId = "CameraTarget";
        [SerializeField] private float focusDuration = 3f;
        [SerializeField] private string arrivalActionKey = "CameraArrived";
        [SerializeField] private bool waitForManualRelease = false;

        public override void GiveReward(Transform playerPos)
        {
            if (CameraSequenceManager.Instance == null)
            {
                Debug.LogWarning("[CameraSequenceReward] CameraSequenceManager not found in scene!");
                return;
            }

            Transform target = CameraSequenceManager.Instance.GetTarget(targetId);

            if (target == null)
            {
                Debug.Log($"[CameraSequenceReward] Target '{targetId}' is not registered in this scene. The sequence will be triggered via QuestCameraSequenceTrigger when entering its scene.");
                return;
            }

            CameraSequenceManager.Instance.PlaySequence(target, null, focusDuration, () =>
            {
                if (!string.IsNullOrEmpty(arrivalActionKey))
                {
                    QuestActionSystem.TriggerAction(arrivalActionKey, 1f);
                }
            }, waitForManualRelease);
        }
    }
}