using UnityEngine;

namespace Treasures.WorldSystem
{
    /// <summary>
    /// Marks where the player should appear when a world scene is loaded.
    /// Place exactly one per world scene.
    /// </summary>
    public class WorldSpawnPoint : MonoBehaviour
    {
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.9f);
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            Gizmos.DrawRay(transform.position, transform.forward * 1.5f);
        }
#endif
    }
}
