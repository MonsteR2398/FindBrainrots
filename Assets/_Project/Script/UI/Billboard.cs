using System.Collections.Generic;
using UnityEngine;

namespace Treasures.UI
{
    [DisallowMultipleComponent]
    public class Billboard : MonoBehaviour
    {
        [Tooltip("If true, the object will only rotate on the Y axis, aligning with the camera's horizontal heading.")]
        [SerializeField] private bool lockY = true;

        private static readonly List<Billboard> ActiveBillboards = new List<Billboard>(64);
        private static Transform _cameraTransform;
        private static bool _updaterInitialized;

        private void OnEnable()
        {
            ActiveBillboards.Add(this);
            EnsureUpdaterExists();
            
            if (_cameraTransform == null && Camera.main != null)
                _cameraTransform = Camera.main.transform;
        }

        private void OnDisable()
        {
            ActiveBillboards.Remove(this);
        }

        private static void EnsureUpdaterExists()
        {
            if (_updaterInitialized) return;
            _updaterInitialized = true;

            GameObject updaterGo = new GameObject("[BillboardUpdater]");
            DontDestroyOnLoad(updaterGo);
            updaterGo.AddComponent<BillboardUpdater>();
        }

        public static void UpdateAll()
        {
            if (_cameraTransform == null)
            {
                if (Camera.main != null)
                {
                    _cameraTransform = Camera.main.transform;
                }
                else
                {
                    return;
                }
            }

            Vector3 camForward = _cameraTransform.forward;
            Quaternion camRotation = _cameraTransform.rotation;

            Vector3 lockYForward = camForward;
            lockYForward.y = 0f;
            Quaternion lockYRotation = Quaternion.identity;
            bool hasValidLockY = lockYForward.sqrMagnitude > 0.0001f;
            if (hasValidLockY)
                lockYRotation = Quaternion.LookRotation(lockYForward.normalized);

            int count = ActiveBillboards.Count;
            for (int i = 0; i < count; i++)
            {
                Billboard b = ActiveBillboards[i];
                if (b == null) continue;

                if (b.lockY)
                {
                    if (hasValidLockY)
                        b.transform.rotation = lockYRotation;
                }
                else
                {
                    b.transform.rotation = camRotation;
                }
            }
        }
    }

    internal class BillboardUpdater : MonoBehaviour
    {
        private void LateUpdate()
        {
            Billboard.UpdateAll();
        }
    }
}

