using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float rotationSpeed = 15f;
    public float jumpHeight = 1.5f;
    public float gravity = -30f;
    public int maxJumps = 2;

    [Header("Momentum")]
    public float acceleration = 60f; 
    public float friction = 50f;     
    public float airDrag = 5f;      

    private CharacterController controller;
    private Vector3 velocity;
    private Transform mainCamera;
    
    private Vector2 moveInput;
    private int jumpsRemaining;
    private bool jumpRequested;
    private float externalSpeedMultiplier = 1f;
    private float externalJumpMultiplier = 1f;

    // ----------------------------------------
    public Vector3 CurrentVelocity => velocity;
    public float CurrentSpeed => new Vector3(velocity.x, 0, velocity.z).magnitude;
    public float HorizontalSpeed => CurrentSpeed;
    public float VerticalSpeed => velocity.y;
    public float MaxSpeed => moveSpeed * externalSpeedMultiplier;
    public float FallSpeed => velocity.y < 0f ? Mathf.Abs(velocity.y) : 0f;
    public bool IsGrounded => controller != null && controller.isGrounded;
    public bool IsFalling => velocity.y < 0f && !IsGrounded;
    public bool IsJumping => velocity.y > 0f && !IsGrounded;
    // ----------------------------------------

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        mainCamera = Camera.main.transform;
        jumpsRemaining = maxJumps;
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed && (controller.isGrounded || jumpsRemaining > 0))
        {
            jumpRequested = true;
        }
    }

    public void Launch(Vector3 force)
    {
        velocity = force;
        jumpsRemaining = maxJumps;
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        if (Mathf.Abs(externalSpeedMultiplier - multiplier) > 0.01f)
        {
            externalSpeedMultiplier = multiplier;
        }
    }

    public void SetJumpMultiplier(float multiplier)
    {
        externalJumpMultiplier = multiplier;
    }

    public void ApplySpeedBoost(float multiplier)
    {
        SetSpeedMultiplier(multiplier);
        
        float targetMax = moveSpeed * multiplier;
        Vector3 hVel = new Vector3(velocity.x, 0, velocity.z);
        if (hVel.magnitude < targetMax)
        {
            Vector3 boostDir = hVel.magnitude > 0.1f ? hVel.normalized : transform.forward;
            Vector3 boosted = boostDir * targetMax;
            velocity.x = boosted.x;
            velocity.z = boosted.z;
        }
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        bool isGrounded = controller.isGrounded;
        
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            jumpsRemaining = maxJumps;
        }

        Vector3 forward = mainCamera.forward;
        Vector3 right = mainCamera.right;
        forward.y = 0; right.y = 0;
        forward.Normalize(); right.Normalize();

        Vector3 rawInput = forward * moveInput.y + right * moveInput.x;
        Vector3 moveDir = rawInput.normalized;
        float targetSpeed = rawInput.magnitude * moveSpeed * externalSpeedMultiplier;

        Vector3 currentHorizontalVel = new Vector3(velocity.x, 0, velocity.z);
        float currentSpeed = currentHorizontalVel.magnitude;

        if (rawInput.magnitude > 0.01f)
        {
            if (currentSpeed > targetSpeed && currentSpeed > 0.1f)
            {
                float dragForce = isGrounded ? friction : airDrag;
                float speedAfterDrag = Mathf.MoveTowards(currentSpeed, targetSpeed, dragForce * Time.deltaTime);
                
                currentHorizontalVel = Vector3.MoveTowards(currentHorizontalVel, moveDir * speedAfterDrag, acceleration * Time.deltaTime);
                if (currentHorizontalVel.magnitude > 0.01f)
                    currentHorizontalVel = currentHorizontalVel.normalized * speedAfterDrag;
            }
            else
            {
                currentHorizontalVel = Vector3.MoveTowards(currentHorizontalVel, moveDir * targetSpeed, acceleration * Time.deltaTime);
            }
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
else
        {
            float dragForce = isGrounded ? friction : airDrag;
            currentHorizontalVel = Vector3.MoveTowards(currentHorizontalVel, Vector3.zero, dragForce * Time.deltaTime);
        }
        velocity.x = currentHorizontalVel.x;
        velocity.z = currentHorizontalVel.z;
        if (jumpRequested)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * externalJumpMultiplier * -2f * gravity);
            jumpsRemaining--;
            jumpRequested = false;
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}