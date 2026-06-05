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
                CameraSequenceManager.Instance.RegisterTarget(id, transform);
        }

        private void OnDisable()
        {
            if (CameraSequenceManager.Instance != null)
                CameraSequenceManager.Instance.UnregisterTarget(id, transform);
        }
    }
}