using System.Collections.Generic;
using UnityEngine;

namespace Treasures.Bots
{
    /// <summary>
    /// Drives a bot through a <see cref="ParkourGraph"/>: asks its <see cref="IBotGoal"/>
    /// for a destination, runs A* to get a path, then follows it node-by-node,
    /// jumping at the right moment on jump links (mirroring the player's jump physics).
    /// </summary>
    [RequireComponent(typeof(BotLocomotion))]
    public class BotBrain : MonoBehaviour
    {
        [Header("Graph")]
        [Tooltip("Optional explicit graph. If null, the bot finds one in the loaded scene.")]
        public ParkourGraph graph;

        [Header("Following")]
        [Tooltip("Horizontal distance at which a node counts as reached (walk links).")]
        public float arriveDistance = 0.6f;

        [Header("Fall Recovery")]
        [Tooltip("Extra drop below the lowest graph node before the bot is rescued back onto the graph.")]
        public float fallRescueMargin = 10f;

        private BotLocomotion _loco;
        private BotContext _ctx;
        private IBotGoal _goal;

        private readonly List<ParkourNode> _path = new List<ParkourNode>();
        private int _pathIndex;
        private ParkourNode _currentNode;
        private bool _jumpedThisEdge;
        private bool _doubleJumpedThisEdge;
        private float _stuckTimer;
        private float _lowestNodeY = float.MaxValue;

        public IBotGoal Goal
        {
            get => _goal;
            set => _goal = value;
        }

        private void Awake()
        {
            _loco = GetComponent<BotLocomotion>();
            if (_goal == null) _goal = new PatrolGoal();
        }

        /// <summary>Call after spawning to bind the bot to a graph (or it auto-finds one).</summary>
        public void Initialize(ParkourGraph targetGraph = null)
        {
            if (targetGraph != null) graph = targetGraph;
            if (graph == null) graph = FindFirstObjectByType<ParkourGraph>();

            if (graph == null)
            {
                Debug.LogWarning($"[BotBrain] No ParkourGraph found for '{name}'. Bot will idle.");
                enabled = false;
                return;
            }

            _ctx = new BotContext(graph, _loco, transform);
            _currentNode = graph.ClosestNode(transform.position);
            _ctx.CurrentNode = _currentNode;

            foreach (ParkourNode n in graph.Nodes)
                if (n != null) _lowestNodeY = Mathf.Min(_lowestNodeY, n.Position.y);

            _path.Clear();
            _pathIndex = 0;
        }

        private void Start()
        {
            if (_ctx == null) Initialize();
        }

        private void Update()
        {
            if (graph == null || _currentNode == null) return;

            CheckFallRecovery();

            // Need a (new) path?
            if (_pathIndex >= _path.Count - 1 || _path.Count == 0)
            {
                AcquireNewPath();
                if (_path.Count < 2)
                {
                    _loco.Stop();
                    return;
                }
            }

            FollowPath();
        }

        private void AcquireNewPath()
        {
            _ctx.CurrentNode = _currentNode;
            ParkourNode target = _goal != null ? _goal.SelectTarget(_ctx) : null;
            if (target == null || target == _currentNode)
            {
                _path.Clear();
                return;
            }

            List<ParkourNode> newPath = graph.FindPath(_currentNode, target);
            _path.Clear();
            if (newPath != null) _path.AddRange(newPath);
            _pathIndex = 0;
            ResetEdgeJumpState();
        }

        private void FollowPath()
        {
            ParkourNode from = _path[_pathIndex];
            ParkourNode to = _path[_pathIndex + 1];
            if (to == null) { _path.Clear(); return; }

            _loco.MoveTowards(to.Position);

            // Determine if this edge requires a jump.
            from.TryGetLinkTo(to, out ParkourLink link);
            bool isJump = link != null && link.isJump;
            bool needsDouble = link != null && link.requiresDoubleJump;

            float horizDist = _loco.HorizontalDistanceTo(to.Position);

            if (isJump)
                HandleJump(to, needsDouble);

            // Reached the next node?
            bool reached = horizDist <= arriveDistance;
            if (reached)
            {
                _currentNode = to;
                _ctx.CurrentNode = _currentNode;
                _pathIndex++;
                ResetEdgeJumpState();
                _stuckTimer = 0f;
            }
            else
            {
                DetectStuck();
            }
        }

        private void HandleJump(ParkourNode to, bool needsDouble)
        {
            // First jump: launch as soon as we're grounded on the takeoff node (placed
            // at the platform edge), so the bot leaps over the gap instead of walking off it.
            if (!_jumpedThisEdge)
            {
                if (_loco.IsGrounded)
                {
                    _loco.Jump();
                    _jumpedThisEdge = true;
                }
            }
            // Second (air) jump near the apex of the first jump, to extend the leap
            // over long/high gaps for maximum distance and height.
            else if (needsDouble && !_doubleJumpedThisEdge && !_loco.IsGrounded)
            {
                float vSpeed = _loco.Controller != null ? _loco.Controller.VerticalSpeed : 0f;
                if (vSpeed <= 1.0f && _loco.JumpsRemaining > 0)
                {
                    _loco.Jump();
                    _doubleJumpedThisEdge = true;
                }
            }
        }

        private void DetectStuck()
        {
            _stuckTimer += Time.deltaTime;
            // If we've been chasing the same node too long, force a re-path.
            if (_stuckTimer > 4f)
            {
                _stuckTimer = 0f;
                _currentNode = graph.ClosestNode(transform.position);
                _path.Clear();
            }
        }

        public void RecoverToClosestNode()
        {
            if (graph == null) return;
            ParkourNode safe = graph.ClosestNode(transform.position) ?? _currentNode;
            if (safe != null)
            {
                _loco.Recover(safe.Position + Vector3.up * 0.5f);
                _currentNode = safe;
                _ctx.CurrentNode = safe;
                _path.Clear();
                ResetEdgeJumpState();
                _stuckTimer = 0f;
            }
        }

        private void CheckFallRecovery()
        {
            if (_lowestNodeY == float.MaxValue) return;
            if (transform.position.y < _lowestNodeY - fallRescueMargin)
            {
                RecoverToClosestNode();
                Debug.LogWarning($"[BotBrain] '{name}' fell off and was recovered.");
            }
        }

        private void ResetEdgeJumpState()
        {
            _jumpedThisEdge = false;
            _doubleJumpedThisEdge = false;
        }
    }
}
