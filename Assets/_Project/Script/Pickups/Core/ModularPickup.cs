using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Treasures.Pickups
{
    [RequireComponent(typeof(Collider))]
    public class ModularPickup : MonoBehaviour
    {
        [Header("Persistence")]
        [Tooltip("Unique ID for this pickup instance. Used to remember if it was collected.")]
        [SerializeField] private string uniqueId;

        [Header("Effects")]
        [SerializeField] private List<BasePickupEffect> effects = new List<BasePickupEffect>();
        
        [Header("Feedback")]
        [SerializeField] private GameObject vfxPrefab;

        [Header("Events")]
        public UnityEvent OnPickedUp;

        private bool _isCollected = false;

        private void Awake()
        {
            Collider col = GetComponent<Collider>();
            if (col != null && !col.isTrigger)
            {
                col.isTrigger = true;
            }
        }

        private void Start()
        {
            if (PickupPersistenceManager.Instance.IsCollected(uniqueId))
            {
                gameObject.SetActive(false);
                // Alternatively: Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isCollected) return;

            if (other.CompareTag("Player"))
            {
                Collect(other.gameObject);
            }
        }

        public void Collect(GameObject picker)
        {
            if (_isCollected) return;
            _isCollected = true;

            foreach (var effect in effects)
            {
                if (effect != null)
                {
                    effect.Apply(picker);
                }
            }

            PickupPersistenceManager.Instance.MarkAsCollected(uniqueId);

            PlayFeedback();

            OnPickedUp?.Invoke();

            FinishPickup();
        }

        private void PlayFeedback()
        {
            if (vfxPrefab != null)
            {
                Instantiate(vfxPrefab, transform.position, Quaternion.identity);
            }
        }

        private void FinishPickup()
        {
            gameObject.SetActive(false);
            
            // If you prefer destruction:
            // Destroy(gameObject, 0.1f);
        }

        [ContextMenu("Generate Unique ID")]
        private void GenerateId()
        {
            uniqueId = Guid.NewGuid().ToString();
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(uniqueId))
            {
                GenerateId();
            }
        }
    }
}
