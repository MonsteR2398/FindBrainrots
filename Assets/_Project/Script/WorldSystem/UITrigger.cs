using UnityEngine;

namespace Treasures.WorldSystem
{
    /// <summary>
    /// Generic trigger that opens any UI component when the player enters the trigger.
    /// The UI component must be registered in UIRegistry with the specified name.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class UITrigger : MonoBehaviour
    {
        [Tooltip("Name of the UI window to open. Must match the registration name in UIRegistry.")]
        [SerializeField] private string uiWindowName;

        [Tooltip("If true, the UI only opens once until the scene is reloaded.")]
        [SerializeField] private bool openOnce = false;

        private bool _used;

        private void Reset()
        {
            Collider c = GetComponent<Collider>();
            if (c != null) c.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                if (openOnce && _used) return;

                PlayerController pc = other.GetComponentInParent<PlayerController>();
                if (pc == null) return;

                if (string.IsNullOrEmpty(uiWindowName))
                {
                    Debug.LogWarning("[UITrigger] UI Window Name is not set in the inspector.");
                    return;
                }

                IUIOpenable openableUI = UIRegistry.Get(uiWindowName);
                if (openableUI != null)
                {
                    openableUI.Open();
                    _used = true;
                }
                else
                {
                    Debug.LogWarning($"[UITrigger] UI window '{uiWindowName}' not found in UIRegistry. Make sure the UI component is registered.");
                }
            }
        }
    }
}
