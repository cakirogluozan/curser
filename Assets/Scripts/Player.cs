using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;

    [Header("Starting Position")]
    [SerializeField] private Vector2 startPosition = Vector2.zero;

    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayerMask;

    [Header("3D Model (Child GameObject)")]
    [SerializeField] private Transform modelTransform;
    [SerializeField] private Animator animator;

    [Header("3D Model Settings")]
    [SerializeField] private bool lockZPosition = true;
    [SerializeField] private float fixedZPosition = 0f;
    [SerializeField] private bool preserveOriginalScale = true;
    [SerializeField] private bool flipModel = true;
    private Vector3 originalModelScale;

    [Header("Animation Triggers")]
    [SerializeField] private string triggerIdle = "TriggerIdle";
    [SerializeField] private string triggerWalking = "TriggerWalking";
    [SerializeField] private string triggerRunning = "TriggerRunning";
    [SerializeField] private string triggerCrouching = "TriggerCrouching";
    [SerializeField] private string triggerWalkCrouching = "TriggerWalkCrouching";
    [SerializeField] private string triggerJumping = "TriggerJumping";
    [SerializeField] private string triggerDoubleJumping = "TriggerDoubleJumping";
    [SerializeField] private string triggerFalling = "TriggerFalling";
    
    [Header("Animation Options")]
    [SerializeField] private bool useRunningAnimationForWalking = false;

    [Header("Movement Thresholds")]
    [SerializeField] private float walkSpeedThreshold = 0.1f;

    [Header("Crouch Input")]
    [SerializeField] private Key crouchKey = Key.C;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float doubleJumpForce = 10f;
    [SerializeField] private Key jumpKey = Key.Space;

    private Rigidbody2D rb;
    private bool isCrouching;
    private bool wasCrouching;
    private bool isSprinting;
    private float horizontalInput;
    private float currentMoveSpeed;

    // Jump states
    private enum JumpState { Grounded, Jumping, Falling, DoubleJumping }
    private JumpState jumpState = JumpState.Grounded;

    // Animation states
    private enum AnimationState { Idle, Walking, Running, Crouching, WalkCrouching, Jumping, DoubleJumping, Falling }
    private AnimationState currentState = AnimationState.Idle;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Set start position
        transform.position = new Vector3(startPosition.x, startPosition.y, transform.position.z);
        if (rb != null) rb.position = startPosition;

        // Assign model
        if (modelTransform == null)
        {
            MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
            modelTransform = renderers.Length > 0 ? renderers[0].transform : transform;
        }

        if (modelTransform != null && preserveOriginalScale)
            originalModelScale = modelTransform.localScale;

        // Find animator (prefer child model, then any children)
        if (animator == null && modelTransform != null)
        {
            animator = modelTransform.GetComponent<Animator>();
            if (animator == null)
                animator = modelTransform.GetComponentInChildren<Animator>(true);
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>(true);
            if (animator == null)
                Debug.LogWarning("Animator not found! Assign an Animator component on the player or its child model.");
        }

        // Create ground check if missing
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = groundCheckObj.transform;
        }
    }

    void Update()
    {
        HandleInput();
        UpdateJumpState();
        UpdateAnimations();
        RotatePlayer();
        wasCrouching = isCrouching;
    }

    void FixedUpdate()
    {
        MovePlayer();
        LockZPosition();
    }

    private void HandleInput()
    {
        if (Keyboard.current == null)
        {
            horizontalInput = 0f;
            isSprinting = false;
            isCrouching = false;
            return;
        }

        // Horizontal movement
        horizontalInput = Keyboard.current.aKey.isPressed ? -1 :
                          Keyboard.current.dKey.isPressed ? 1 : 0;

        // Sprinting
        isSprinting = (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed)
                      && !isCrouching && Mathf.Abs(horizontalInput) > 0.1f;
        currentMoveSpeed = isSprinting ? sprintSpeed : moveSpeed;

        // Crouching
        isCrouching = Keyboard.current.cKey.isPressed || (crouchKey != Key.None && Keyboard.current[crouchKey].isPressed);

        // Jumping
        if (Keyboard.current.spaceKey.wasPressedThisFrame || 
            (jumpKey != Key.None && Keyboard.current[jumpKey].wasPressedThisFrame))
        {
            if (jumpState == JumpState.Grounded)
            {
                jumpState = JumpState.Jumping; // mark jump immediately
                Jump();
            }
            else if (jumpState == JumpState.Jumping)
            {
                jumpState = JumpState.DoubleJumping;
                DoubleJump();
            }
        }
    }

    private void MovePlayer()
    {
        rb.linearVelocity = new Vector2(horizontalInput * currentMoveSpeed, rb.linearVelocity.y);
    }

    private void LockZPosition()
    {
        if (lockZPosition && modelTransform != null)
        {
            Vector3 pos = modelTransform.position;
            pos.z = fixedZPosition;
            modelTransform.position = pos;
        }
    }

    private void UpdateJumpState()
    {
        bool groundedNow = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayerMask);

        switch (jumpState)
        {
            case JumpState.Grounded:
                if (!groundedNow) jumpState = JumpState.Falling;
                break;
            case JumpState.Jumping:
            case JumpState.DoubleJumping:
                if (rb.linearVelocity.y < 0) jumpState = JumpState.Falling; // enter falling when descending
                break;
            case JumpState.Falling:
                if (groundedNow) jumpState = JumpState.Grounded;
                break;
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        if (animator != null)
            animator.SetTrigger(triggerJumping);
    }

    private void DoubleJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, doubleJumpForce);
        if (animator != null)
            animator.SetTrigger(triggerDoubleJumping);
    }

    private void RotatePlayer()
    {
        if (Mathf.Abs(horizontalInput) < 0.1f) return;
        if (flipModel && modelTransform != null)
        {
            Vector3 scale = preserveOriginalScale ? originalModelScale : modelTransform.localScale;
            scale.x = horizontalInput > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            modelTransform.localScale = scale;
        }
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        AnimationState targetState = DetermineAnimationState();

        if (targetState != currentState)
        {
            TriggerAnimation(targetState);
            currentState = targetState;
        }
    }


    private AnimationState DetermineAnimationState()
    {
        float absSpeed = Mathf.Abs(horizontalInput * currentMoveSpeed);

        // Jumping states take priority
        if (jumpState == JumpState.Jumping) return AnimationState.Jumping;
        if (jumpState == JumpState.DoubleJumping) return AnimationState.DoubleJumping;
        if (jumpState == JumpState.Falling) return AnimationState.Falling;

        // Crouching
        if (isCrouching)
            return absSpeed > walkSpeedThreshold ? AnimationState.WalkCrouching : AnimationState.Crouching;

        // Movement
        if (absSpeed > walkSpeedThreshold)
            return isSprinting ? AnimationState.Running : AnimationState.Walking;

        return AnimationState.Idle;
    }
    private void TriggerAnimation(AnimationState state)
    {
        ResetAllTriggers();
        switch (state)
        {
            case AnimationState.Idle: animator.SetTrigger(triggerIdle); break;
            case AnimationState.Walking:
                animator.SetTrigger(useRunningAnimationForWalking ? triggerRunning : triggerWalking);
                break;
            case AnimationState.Running: animator.SetTrigger(triggerRunning); break;
            case AnimationState.Crouching: animator.SetTrigger(triggerCrouching); break;
            case AnimationState.WalkCrouching: animator.SetTrigger(triggerWalkCrouching); break;
            case AnimationState.Jumping: animator.SetTrigger(triggerJumping); break;
            case AnimationState.DoubleJumping: animator.SetTrigger(triggerDoubleJumping); break;
            case AnimationState.Falling: animator.SetTrigger(triggerFalling); break;
        }
    }
    private void ResetAllTriggers()
    {
        if (animator == null) return;
        animator.ResetTrigger(triggerIdle);
        animator.ResetTrigger(triggerWalking);
        animator.ResetTrigger(triggerRunning);
        animator.ResetTrigger(triggerCrouching);
        animator.ResetTrigger(triggerWalkCrouching);
        animator.ResetTrigger(triggerJumping);
        animator.ResetTrigger(triggerDoubleJumping);
        animator.ResetTrigger(triggerFalling);

    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayerMask) ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
