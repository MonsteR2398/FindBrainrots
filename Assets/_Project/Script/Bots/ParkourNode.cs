using System.Collections.Generic;
using UnityEngine;

namespace Treasures.Bots
{
    /// <summary>
    /// A waypoint placed on a platform. Bots travel from node to node.
    /// Edges to neighbours are stored in <see cref="links"/>; some are jump links.
    /// </summary>
    public class ParkourNode : MonoBehaviour
    {
        [Tooltip("Outgoing edges to neighbouring nodes.")]
        public List<ParkourLink> links = new List<ParkourLink>();

        public Vector3 Position => transform.position;

        public bool TryGetLinkTo(ParkourNode other, out ParkourLink link)
        {
            for (int i = 0; i < links.Count; i++)
            {
                if (links[i] != null && links[i].target == other)
                {
                    link = links[i];
                    return true;
                }
            }
            link = null;
            return false;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.2f, 0.7f, 1f, 0.9f);
            Gizmos.DrawSphere(transform.position, 0.25f);

            if (links == null) return;
            for (int i = 0; i < links.Count; i++)
            {
                ParkourLink link = links[i];
                if (link == null || link.target == null) continue;

                if (link.isJump)
                {
                    // Arc for jump links (orange/yellow).
                    Gizmos.color = link.requiresDoubleJump
                        ? new Color(1f, 0.4f, 0.1f, 0.9f)
                        : new Color(1f, 0.85f, 0.1f, 0.9f);
                    DrawArc(transform.position, link.target.Position);
                }
                else
                {
                    Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
                    Gizmos.DrawLine(transform.position, link.target.Position);
                }
            }
        }

        private static void DrawArc(Vector3 from, Vector3 to)
        {
            const int segments = 12;
            float height = Mathf.Max(0.5f, Vector3.Distance(from, to) * 0.25f);
            Vector3 prev = from;
            for (int s = 1; s <= segments; s++)
            {
                float t = s / (float)segments;
                Vector3 point = Vector3.Lerp(from, to, t);
                point.y += Mathf.Sin(t * Mathf.PI) * height;
                Gizmos.DrawLine(prev, point);
                prev = point;
            }
        }
#endif
    }
}
