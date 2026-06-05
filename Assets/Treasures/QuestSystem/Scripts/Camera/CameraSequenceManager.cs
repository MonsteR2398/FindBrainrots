using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace ModularTreasures.Quests
{
    public class CameraSequenceManager : MonoBehaviour
    {
        public static CameraSequenceManager Instance { get; private set; }

        [SerializeField] private CinemachineCamera focusCamera;
        [SerializeField] private float defaultDuration = 2f;
        [SerializeField] private float returnDelay = 0.5f;
        [SerializeField] private CinemachineBrain brain;

        private readonly Dictionary<string, Transform> _targets = new Dictionary<string, Transform>();
        private bool _manualReleaseReceived;

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

        public void RegisterTarget(string id, Transform target)
        {
            if (string.IsNullOrEmpty(id) || target == null) return;
            _targets[id] = target;
        }

        public void UnregisterTarget(string id, Transform target)
        {
            if (string.IsNullOrEmpty(id)) return;
            if (_targets.TryGetValue(id, out var current) && current == target)
                _targets.Remove(id);
        }

        public Transform GetTarget(string id)
        {
            return _targets.TryGetValue(id, out var target) ? target : null;
        }

        public void ReleaseCamera()
        {
            _manualReleaseReceived = true;
        }

        public void PlaySequence(Transform target, Transform viewPoint = null, float duration = -1f, Action onArrival = null, bool waitForManualRelease = false)
        {
            if (focusCamera == null || target == null) return;

            StopAllCoroutines();
            StartCoroutine(SequenceRoutine(target, viewPoint, duration > 0 ? duration : defaultDuration, onArrival, waitForManualRelease));
        }

        private IEnumerator SequenceRoutine(Transform target, Transform viewPoint, float duration, Action onArrival, bool waitForManualRelease)
        {
            _manualReleaseReceived = false;

            if (viewPoint == null)
            {
                viewPoint = target.Find("CameraView");
            }

            if (viewPoint != null)
            {
                focusCamera.LookAt = null;
                focusCamera.transform.position = viewPoint.position;
                focusCamera.transform.rotation = viewPoint.rotation;
            }
            else
            {
                focusCamera.LookAt = target;
                focusCamera.transform.position = target.position + target.forward * -5f + Vector3.up * 3f;
                focusCamera.transform.LookAt(target);
            }

            focusCamera.Priority = 100;

            if (brain != null)
            {
                yield return new WaitUntil(() => brain.IsBlending);
                yield return new WaitWhile(() => brain.IsBlending);
            }
            else
            {
                yield return new WaitForSeconds(1.5f);
            }

            onArrival?.Invoke();

            if (waitForManualRelease)
            {
                // Ждем сигнала ReleaseCamera() ИЛИ пока не выйдет время duration (таймаут)
                float timeout = Time.time + duration;
                yield return new WaitUntil(() => _manualReleaseReceived || Time.time > timeout);
            }
            else
            {
                yield return new WaitForSeconds(duration);
            }

            yield return new WaitForSeconds(returnDelay);
            focusCamera.Priority = -1;
        }
    }
}