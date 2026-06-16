using UnityEngine;

namespace Treasures.WorldSystem
{
    /// <summary>
    /// Place on a trigger collider in a world scene. When the player walks in,
    /// it opens the world-selection window.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PortalTrigger : MonoBehaviour
    {
        [Tooltip("If true the portal only opens the window once until reloaded.")]
        [SerializeField] private bool openOnce = false;

        private bool _used;

        private void Reset()
        {
            Collider c = GetComponent<Collider>();
            if (c != null) c.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                if (openOnce && _used) return;

                PlayerController pc = other.GetComponentInParent<PlayerController>();
                if (pc == null) return;

                if (PortalWindowUI.Instance != null)
                {
                    PortalWindowUI.Instance.Open();
                    _used = true;
                }
                else
                {
                    Debug.LogWarning("[PortalTrigger] No PortalWindowUI in the scene.");
                }
            }

        }
    }
}
