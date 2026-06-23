using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float rotationSpeed = 15f;
    public float jumpHeight = 1.5f;
    public float gravity = -30f;
    public int maxJumps = 2;

    [Tooltip("When true, movement input is interpreted relative to the main camera (human player). " +
             "When false, input is interpreted in world space (used by bots that have no camera).")]
    public bool useCameraRelativeMovement = true;

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

    private Vector3 lastSafePosition;
    private Quaternion lastSafeRotation;

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
    public int JumpsRemaining => jumpsRemaining;
    // ----------------------------------------

    public UnityEvent Jumped;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        if (useCameraRelativeMovement && Camera.main != null)
            mainCamera = Camera.main.transform;
        jumpsRemaining = maxJumps;

        lastSafePosition = transform.position;
        lastSafeRotation = transform.rotation;
    }

    public void OnMove(InputValue value)
    {
        SetMoveInput(value.Get<Vector2>());
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            RequestJump();
            Jumped?.Invoke();
        }
    }

    /// <summary>
    /// Sets the desired movement input (-1..1 on each axis). Used by human input (OnMove)
    /// and by bots driving this controller programmatically.
    /// </summary>
    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    /// <summary>
    /// Requests a jump for the next movement update, if grounded or air-jumps remain.
    /// Shared entry point for human input (OnJump) and bots.
    /// </summary>
    public void RequestJump()
    {
        if (controller == null) controller = GetComponent<CharacterController>();
        if (controller.isGrounded || jumpsRemaining > 0)
        {
            jumpRequested = true;
        }
    }

    public void Launch(Vector3 force)
    {
        velocity = force;
        jumpsRemaining = maxJumps;
    }

    /// <summary>
    /// Safely moves the player to a new position/rotation. The CharacterController must
    /// be disabled while repositioning, otherwise its internal collision state fights
    /// the transform change. Also resets accumulated velocity.
    /// </summary>
    public void Teleport(Vector3 position, Quaternion rotation)
    {
        if (controller == null) controller = GetComponent<CharacterController>();

        bool wasEnabled = controller != null && controller.enabled;
        if (controller != null) controller.enabled = false;

        transform.SetPositionAndRotation(position, rotation);

        if (controller != null) controller.enabled = wasEnabled;

        velocity = Vector3.zero;
        jumpsRemaining = maxJumps;
    }

    public void RespawnAtLastSafePosition()
    {
        Teleport(lastSafePosition, lastSafeRotation);
    }

    public void AddSpeedMultiplier(float multiplier)
    {
            externalSpeedMultiplier += multiplier;
    }

    public void RemoveSpeedMultiplayer(float multiplier)
    {
        externalSpeedMultiplier -= multiplier;
    }

    public void RemoveJumpMultiplayer(float multiplier)
    {
        externalJumpMultiplier -= multiplier;
    }

    public void AddJumpMultiplier(float multiplier)
    {
        externalJumpMultiplier += multiplier;
    }

    public void ApplySpeedBoost(float multiplier)
    {
        AddSpeedMultiplier(multiplier);
        
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
        if (IsGrounded)
        {
            lastSafePosition = transform.position;
            lastSafeRotation = transform.rotation;
        }
    }

    private void HandleMovement()
    {
        bool isGrounded = controller.isGrounded;
        
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            jumpsRemaining = maxJumps;
        }

        Vector3 forward, right;
        if (useCameraRelativeMovement && mainCamera != null)
        {
            forward = mainCamera.forward;
            right = mainCamera.right;
        }
        else
        {
            // World-space axes for bots (no camera dependency)
            forward = Vector3.forward;
            right = Vector3.right;
        }
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