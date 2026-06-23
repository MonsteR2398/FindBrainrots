using UnityEngine;

namespace Treasures.Bots
{
    /// <summary>
    /// Translates high-level "go to point" / "jump" commands into the same
    /// <see cref="PlayerController"/> inputs a human would produce, so a bot moves
    /// with identical physics (momentum, double jump, gravity).
    /// Requires the controller to have useCameraRelativeMovement = false.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class BotLocomotion : MonoBehaviour
    {
        private PlayerController _controller;

        public PlayerController Controller
        {
            get
            {
                if (_controller == null) _controller = GetComponent<PlayerController>();
                return _controller;
            }
        }

        private void Awake()
        {
            _controller = GetComponent<PlayerController>();
            if (_controller != null && _controller.useCameraRelativeMovement)
            {
                // Bots must use world-space movement (no camera).
                _controller.useCameraRelativeMovement = false;
            }
        }

        private void Start()
        {
            IgnoreBarrierCollisions();
        }

        private void IgnoreBarrierCollisions()
        {
            var charController = GetComponent<CharacterController>();
            if (charController == null) return;

            var allColliders = Object.FindObjectsByType<Collider>(FindObjectsInactive.Include);
            foreach (var col in allColliders)
            {
                if (col == null) continue;

                bool isBarrier = col.name.Contains("Barrier", System.StringComparison.OrdinalIgnoreCase) ||
                                 col.name.Contains("барьер", System.StringComparison.OrdinalIgnoreCase);

                if (!isBarrier && col.transform.parent != null)
                {
                    isBarrier = col.transform.parent.name.Contains("Barrier", System.StringComparison.OrdinalIgnoreCase) ||
                                col.transform.parent.name.Contains("барьер", System.StringComparison.OrdinalIgnoreCase);
                }

                if (isBarrier)
                {
                    Physics.IgnoreCollision(charController, col, true);
                }
            }
        }

        public bool IsGrounded => Controller != null && Controller.IsGrounded;
        public int JumpsRemaining => Controller != null ? Controller.JumpsRemaining : 0;

        /// <summary>Drives the controller toward a world target (XZ plane).</summary>
        public void MoveTowards(Vector3 worldTarget)
        {
            if (Controller == null) return;
            Vector3 dir = worldTarget - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f) dir.Normalize();
            // World axes: x -> world right, y(input) -> world forward (Z).
            Controller.SetMoveInput(new Vector2(dir.x, dir.z));
        }

        /// <summary>Drives the controller in an explicit world-space direction.</summary>
        public void MoveInDirection(Vector3 worldDir)
        {
            if (Controller == null) return;
            worldDir.y = 0f;
            if (worldDir.sqrMagnitude > 0.0001f) worldDir.Normalize();
            Controller.SetMoveInput(new Vector2(worldDir.x, worldDir.z));
        }

        public void Stop()
        {
            if (Controller != null) Controller.SetMoveInput(Vector2.zero);
        }

        public void Jump()
        {
            if (Controller != null) Controller.RequestJump();
        }

        /// <summary>Horizontal distance from the bot to a world point.</summary>
        public float HorizontalDistanceTo(Vector3 worldPoint)
        {
            Vector3 d = worldPoint - transform.position;
            d.y = 0f;
            return d.magnitude;
        }

        /// <summary>Recovers a bot that fell off the level back to a safe position.</summary>
        public void Recover(Vector3 safePosition)
        {
            if (Controller != null)
                Controller.Teleport(safePosition, transform.rotation);
        }
    }
}
