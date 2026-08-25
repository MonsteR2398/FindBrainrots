using UnityEngine;

namespace ModularTreasures.Quests
{
    public class CameraTarget : MonoBehaviour
    {
        [SerializeField] private string id;

        public string Id => id;

        private void Start()
        {
            if (CameraSequenceManager.Instance != null)
            {
                CameraSequenceManager.Instance.RegisterTarget(id, transform);
            }
            else
            {
                Debug.LogWarning($"[CameraTarget] Could not register target '{id}' because CameraSequenceManager.Instance is null!");
            }
        }

        private void OnDisable()
        {
            if (CameraSequenceManager.Instance != null)
            {
                Debug.Log($"[CameraTarget] Unregistering target '{id}' on GameObject '{gameObject.name}' due to OnDisable.");
                CameraSequenceManager.Instance.UnregisterTarget(id, transform);
            }
        }

        private void OnDestroy()
        {
            if (CameraSequenceManager.Instance != null)
            {
                Debug.Log($"[CameraTarget] Unregistering target '{id}' on GameObject '{gameObject.name}' due to OnDestroy.");
                CameraSequenceManager.Instance.UnregisterTarget(id, transform);
            }
        }
    }
}