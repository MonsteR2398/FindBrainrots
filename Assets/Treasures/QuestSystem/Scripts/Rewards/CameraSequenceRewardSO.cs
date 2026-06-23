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

            if (string.IsNullOrEmpty(targetId)) return;

            string[] ids = targetId.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
            string[] arrivalKeys = !string.IsNullOrEmpty(arrivalActionKey) 
                ? arrivalActionKey.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries) 
                : new string[0];

            for (int i = 0; i < ids.Length; i++)
            {
                string id = ids[i].Trim();
                Transform target = CameraSequenceManager.Instance.GetTarget(id);

                if (target == null)
                {
                    Debug.Log($"[CameraSequenceReward] Target '{id}' is not registered in this scene.");
                    continue;
                }

                string currentArrivalKey = (i < arrivalKeys.Length) ? arrivalKeys[i].Trim() : "";

                CameraSequenceManager.Instance.PlaySequence(target, null, focusDuration, () =>
                {
                    if (!string.IsNullOrEmpty(currentArrivalKey))
                    {
                        QuestActionSystem.TriggerAction(currentArrivalKey, 1f);
                    }
                }, waitForManualRelease);
            }
        }
    }
}