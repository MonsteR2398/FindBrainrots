using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using System;

namespace ModularTreasures.Quests
{
    public class CameraSequenceManager : MonoBehaviour
    {
        public static CameraSequenceManager Instance { get; private set; }

        [SerializeField] private CinemachineCamera focusCamera;
        [SerializeField] private float defaultDuration = 2f;
        [SerializeField] private float returnDelay = 0.5f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                if (focusCamera != null) focusCamera.Priority = 0;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void PlaySequence(Transform target, float duration = -1f, Action onArrival = null)
        {
            if (focusCamera == null)
            {
                Debug.LogWarning("[CameraSequenceManager] Focus Camera not assigned!");
                return;
            }

            if (target == null)
            {
                Debug.LogWarning("[CameraSequenceManager] Target is null!");
                return;
            }

            StopAllCoroutines();
            StartCoroutine(SequenceRoutine(target, duration > 0 ? duration : defaultDuration, onArrival));
        }

        private IEnumerator SequenceRoutine(Transform target, float duration, Action onArrival)
        {
            // 1. Configure camera
            focusCamera.LookAt = target;
            // Move camera to a decent viewing position relative to target
            focusCamera.transform.position = target.position + target.forward * -5f + Vector3.up * 3f;
            focusCamera.transform.LookAt(target);

            // 2. Activate focus camera
            focusCamera.Priority = 100;

            // 3. Wait for blend to finish (arrival)
            var brain = UnityEngine.Object.FindAnyObjectByType<CinemachineBrain>();
            if (brain != null)
            {
                // Wait for blend to start then finish
                yield return new WaitUntil(() => brain.IsBlending);
                yield return new WaitWhile(() => brain.IsBlending);
            }
            else
            {
                yield return new WaitForSeconds(2f); // Fallback
            }

            // 4. Trigger on arrival action
            onArrival?.Invoke();

            // 5. Hold position
            yield return new WaitForSeconds(duration);

            // 6. Extra delay before returning
            yield return new WaitForSeconds(returnDelay);

            // 7. Deactivate
            focusCamera.Priority = 0;
        }
    }
}