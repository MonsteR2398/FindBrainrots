using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private PlayerController playerController;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AnimSpeedHash = Animator.StringToHash("AnimSpeed");
    private static readonly int VerticalSpeedHash = Animator.StringToHash("VerticalSpeed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerController = GetComponentInParent<PlayerController>();
    }

    [Header("Settings")]
    public float speedDampTime = 0.1f;

    private void Update()
    {
        if (playerController == null || animator == null) return;

        float targetSpeed = playerController.HorizontalSpeed / playerController.moveSpeed;
        animator.SetFloat(SpeedHash, targetSpeed, speedDampTime, Time.deltaTime);
        
        float animSpeed = targetSpeed > 0.01f ? targetSpeed : 1f;
        animator.SetFloat(AnimSpeedHash, animSpeed, speedDampTime, Time.deltaTime);
        
        animator.SetFloat(VerticalSpeedHash, playerController.VerticalSpeed, speedDampTime, Time.deltaTime);
        animator.SetBool(IsGroundedHash, playerController.IsGrounded);
    }
}
