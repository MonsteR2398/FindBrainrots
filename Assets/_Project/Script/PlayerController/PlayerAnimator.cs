using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private PlayerController playerController;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int VerticalSpeedHash = Animator.StringToHash("VerticalSpeed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        // Try to find PlayerController on this object or parent
        playerController = GetComponentInParent<PlayerController>();
    }

    private void Update()
    {
        if (playerController == null || animator == null) return;

        animator.SetFloat(SpeedHash, playerController.HorizontalSpeed);
        animator.SetFloat(VerticalSpeedHash, playerController.VerticalSpeed);
        animator.SetBool(IsGroundedHash, playerController.IsGrounded);
    }
}
