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

        private class SequenceRequest
        {
            public Transform Target;
            public Transform ViewPoint;
            public float Duration;
            public Action OnArrival;
            public bool WaitForManualRelease;
        }

        private readonly Queue<SequenceRequest> _sequenceQueue = new Queue<SequenceRequest>();
        private readonly List<CinemachineCamera> _activeTempCameras = new List<CinemachineCamera>();
        private bool _isSequenceActive;

        public void ReleaseCamera()
        {
            _manualReleaseReceived = true;
        }

        public void PlaySequence(Transform target, Transform viewPoint = null, float duration = -1f, Action onArrival = null, bool waitForManualRelease = false)
        {
            if (focusCamera == null || target == null) return;

            var request = new SequenceRequest
            {
                Target = target,
                ViewPoint = viewPoint,
                Duration = duration > 0 ? duration : defaultDuration,
                OnArrival = onArrival,
                WaitForManualRelease = waitForManualRelease
            };

            _sequenceQueue.Enqueue(request);

            if (!_isSequenceActive)
            {
                StartCoroutine(ProcessQueueRoutine());
            }
        }

        private IEnumerator ProcessQueueRoutine()
        {
            _isSequenceActive = true;
            int priority = 100;

            // Мы используем внешний цикл, чтобы гарантировать, что если во время блендинга назад 
            // в очередь упали новые запросы, они будут обработаны до того, как мы выключим флаг активности.
            while (_sequenceQueue.Count > 0)
            {
                while (_sequenceQueue.Count > 0)
                {
                    SequenceRequest request = _sequenceQueue.Dequeue();
                    
                    if (request.Target == null)
                    {
                        Debug.LogWarning("[CameraSequenceManager] Dequeued a request with a null target. Skipping.");
                        continue;
                    }

                    yield return StartCoroutine(SequenceRoutine(
                        request.Target,
                        request.ViewPoint,
                        request.Duration,
                        request.OnArrival,
                        request.WaitForManualRelease,
                        priority
                    ));
                    priority++;
                }

                // Временно снижаем приоритет всех созданных камер, чтобы начать блендинг назад к игроку
                foreach (var cam in _activeTempCameras)
                {
                    if (cam != null) cam.Priority = -1;
                }

                if (returnDelay > 0)
                    yield return new WaitForSeconds(returnDelay);

                if (brain != null)
                {
                    // Даем Cinemachine один кадр на старт блендинга
                    yield return null;
                    if (brain.IsBlending)
                    {
                        yield return new WaitWhile(() => brain.IsBlending);
                    }
                }
                else
                {
                    yield return new WaitForSeconds(1.0f);
                }
                
                // Если за время блендинга очередь снова наполнилась, цикл продолжится
            }

            // Окончательная очистка временных камер
            foreach (var cam in _activeTempCameras)
            {
                if (cam != null) Destroy(cam.gameObject);
            }
            _activeTempCameras.Clear();

            _isSequenceActive = false;
        }

        private IEnumerator SequenceRoutine(Transform target, Transform viewPoint, float duration, Action onArrival, bool waitForManualRelease, int priority)
        {
            if (target == null) yield break;

            _manualReleaseReceived = false;

            if (viewPoint == null && target.childCount > 0)
            {
                viewPoint = target.GetChild(0);
            }

            // Instantiate a temporary focus camera
            CinemachineCamera tempCam = Instantiate(focusCamera, focusCamera.transform.parent);
            tempCam.gameObject.name = "Temp_FocusCamera_" + target.name;
            tempCam.gameObject.SetActive(true);
            _activeTempCameras.Add(tempCam);

            if (viewPoint != null)
            {
                tempCam.LookAt = null;
                tempCam.transform.position = viewPoint.position;
                tempCam.transform.rotation = viewPoint.rotation;
            }
            else
            {
                tempCam.transform.position = target.position + target.forward * -5f + Vector3.up * 3f;
            }
            
            tempCam.transform.LookAt(target);
            tempCam.LookAt = target;

            tempCam.Priority = priority;

            if (brain != null)
            {
                // Даем Cinemachine один кадр на регистрацию изменения приоритета и старт блендинга
                yield return null;
                if (brain.IsBlending)
                {
                    yield return new WaitWhile(() => brain.IsBlending);
                }
            }
            else
            {
                yield return new WaitForSeconds(1.0f);
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
        }
    }
}