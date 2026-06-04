using UnityEngine;

namespace ModularTreasures.Quests
{
    [CreateAssetMenu(fileName = "Camera Sequence Reward", menuName = "Quests/Rewards/Camera Sequence")]
    public class CameraSequenceRewardSO : QuestRewardSO
    {
        [SerializeField] private string targetName = "CameraTarget";
        [SerializeField] private float focusDuration = 3f;
        [SerializeField] private string arrivalActionKey = "CameraArrived";

        public override void GiveReward(Transform playerPos)
        {
            GameObject targetObj = GameObject.Find(targetName);
            if (targetObj == null)
            {
                Debug.LogWarning($"[CameraSequenceReward] Target '{targetName}' not found in scene!");
                return;
            }

            if (CameraSequenceManager.Instance != null)
            {
                CameraSequenceManager.Instance.PlaySequence(targetObj.transform, focusDuration, () => 
                {
                    if (!string.IsNullOrEmpty(arrivalActionKey))
                    {
                        QuestActionSystem.TriggerAction(arrivalActionKey, 1f);
                    }
                });
            }
        }
    }
}