using System.Collections.Generic;
using UnityEngine;

namespace Treasures.Bots
{
    /// <summary>
    /// Container for a parkour navigation graph placed in a world scene.
    /// Collects all <see cref="ParkourNode"/> children, finds the closest node to a
    /// position and runs A* over the node edges (jump links cost more).
    /// </summary>
    public class ParkourGraph : MonoBehaviour
    {
        [Tooltip("Extra path cost multiplier applied to jump links so bots prefer walking when possible.")]
        public float jumpCostMultiplier = 1.5f;

        private readonly List<ParkourNode> _nodes = new List<ParkourNode>();
        private bool _collected;

        public IReadOnlyList<ParkourNode> Nodes
        {
            get
            {
                EnsureCollected();
                return _nodes;
            }
        }

        private void Awake() => Collect();

        public void Collect()
        {
            _nodes.Clear();
            GetComponentsInChildren(true, _nodes);
            _collected = true;
        }

        private void EnsureCollected()
        {
            if (!_collected || _nodes.Count == 0) Collect();
        }

        public ParkourNode ClosestNode(Vector3 position)
        {
            EnsureCollected();
            ParkourNode best = null;
            float bestSqr = float.MaxValue;
            for (int i = 0; i < _nodes.Count; i++)
            {
                ParkourNode n = _nodes[i];
                if (n == null) continue;
                float sqr = (n.Position - position).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = n;
                }
            }
            return best;
        }

        public ParkourNode RandomNode(ParkourNode exclude = null)
        {
            EnsureCollected();
            if (_nodes.Count == 0) return null;
            // Try a few times to avoid the excluded node.
            for (int attempt = 0; attempt < 8; attempt++)
            {
                ParkourNode candidate = _nodes[Random.Range(0, _nodes.Count)];
                if (candidate != null && candidate != exclude) return candidate;
            }
            return _nodes[Random.Range(0, _nodes.Count)];
        }

        /// <summary>
        /// A* over node links. Returns the list of nodes from <paramref name="from"/> to
        /// <paramref name="to"/> inclusive, or null if no path exists.
        /// </summary>
        public List<ParkourNode> FindPath(ParkourNode from, ParkourNode to)
        {
            EnsureCollected();
            if (from == null || to == null) return null;
            if (from == to) return new List<ParkourNode> { from };

            var open = new List<ParkourNode> { from };
            var cameFrom = new Dictionary<ParkourNode, ParkourNode>();
            var gScore = new Dictionary<ParkourNode, float> { [from] = 0f };
            var fScore = new Dictionary<ParkourNode, float> { [from] = Heuristic(from, to) };

            while (open.Count > 0)
            {
                // Node in open with lowest fScore.
                ParkourNode current = open[0];
                float currentF = fScore.TryGetValue(current, out float cf) ? cf : float.MaxValue;
                for (int i = 1; i < open.Count; i++)
                {
                    float f = fScore.TryGetValue(open[i], out float v) ? v : float.MaxValue;
                    if (f < currentF)
                    {
                        current = open[i];
                        currentF = f;
                    }
                }

                if (current == to) return Reconstruct(cameFrom, current);

                open.Remove(current);

                if (current.links == null) continue;
                for (int i = 0; i < current.links.Count; i++)
                {
                    ParkourLink link = current.links[i];
                    if (link == null || link.target == null) continue;
                    ParkourNode neighbour = link.target;

                    float edgeCost = Vector3.Distance(current.Position, neighbour.Position);
                    if (link.isJump) edgeCost *= jumpCostMultiplier;

                    float tentativeG = (gScore.TryGetValue(current, out float g) ? g : float.MaxValue) + edgeCost;
                    if (tentativeG < (gScore.TryGetValue(neighbour, out float ng) ? ng : float.MaxValue))
                    {
                        cameFrom[neighbour] = current;
                        gScore[neighbour] = tentativeG;
                        fScore[neighbour] = tentativeG + Heuristic(neighbour, to);
                        if (!open.Contains(neighbour)) open.Add(neighbour);
                    }
                }
            }

            return null; // No path.
        }

        private static float Heuristic(ParkourNode a, ParkourNode b)
            => Vector3.Distance(a.Position, b.Position);

        private static List<ParkourNode> Reconstruct(Dictionary<ParkourNode, ParkourNode> cameFrom, ParkourNode current)
        {
            var path = new List<ParkourNode> { current };
            while (cameFrom.TryGetValue(current, out ParkourNode prev))
            {
                current = prev;
                path.Add(current);
            }
            path.Reverse();
            return path;
        }
    }
}
