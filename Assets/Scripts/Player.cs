using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
    [SerializeField] private string triggerDashing = "TriggerDashing";
    
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

    [Header("Dash Settings")]
    [SerializeField] private float dashForce = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 0.5f;
    [SerializeField] private float speedDecayRate = 5f; // How fast the speed decays after dash (units per second)

    [Header("Restart Settings")]
    [SerializeField] private float restartHoldDuration = 2f;

    private Rigidbody2D rb;
    private bool isCrouching;
    private bool wasCrouching;
    private bool isSprinting;
    private float horizontalInput;
    private float currentMoveSpeed;

    // Jump states
    private enum JumpState { Grounded, Jumping, Falling, DoubleJumping }
    private JumpState jumpState = JumpState.Grounded;

    // Dash state
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private float dashDirection = 0f;
    
    // Dynamic speed system
    private float currentHorizontalSpeed = 0f; // Dynamic speed that carries over from dash

    // Restart state
    private float restartHoldTimer = 0f;

    // Animation states
    private enum AnimationState { Idle, Walking, Running, Crouching, WalkCrouching, Jumping, DoubleJumping, Falling, Dashing }
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

        // Verify AnimatorController is assigned
        if (animator != null && animator.runtimeAnimatorController == null)
        {
            Debug.LogError($"Animator on '{animator.gameObject.name}' has no AnimatorController assigned! Please assign a controller in the Inspector.");
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
        UpdateDash();
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

        // Dashing (Ctrl key)
        if ((Keyboard.current.leftCtrlKey.wasPressedThisFrame || Keyboard.current.rightCtrlKey.wasPressedThisFrame) 
            && !isDashing && dashCooldownTimer <= 0f)
        {
            Dash();
        }

        // Restart (Hold R for 2 seconds)
        if (Keyboard.current.rKey.isPressed)
        {
            restartHoldTimer += Time.deltaTime;
            if (restartHoldTimer >= restartHoldDuration)
            {
                RestartGame();
            }
        }
        else
        {
            restartHoldTimer = 0f; // Reset timer when R is released
        }
    }

    private void MovePlayer()
    {
        if (isDashing)
        {
            // During dash, set and maintain dash velocity
            currentHorizontalSpeed = dashDirection * dashForce;
            rb.linearVelocity = new Vector2(currentHorizontalSpeed, rb.linearVelocity.y);
        }
        else
        {
            // Apply speed decay when not dashing
            if (Mathf.Abs(currentHorizontalSpeed) > 0.1f)
            {
                // Decay speed towards zero
                float decayAmount = speedDecayRate * Time.fixedDeltaTime;
                if (currentHorizontalSpeed > 0)
                {
                    currentHorizontalSpeed = Mathf.Max(0f, currentHorizontalSpeed - decayAmount);
                }
                else
                {
                    currentHorizontalSpeed = Mathf.Min(0f, currentHorizontalSpeed + decayAmount);
                }
            }
            else
            {
                currentHorizontalSpeed = 0f;
            }
            
            // Combine dynamic speed with input-based movement
            float targetSpeed = horizontalInput * currentMoveSpeed;
            
            // If we have dynamic speed, add it to the target speed (but don't exceed max dash speed)
            if (Mathf.Abs(currentHorizontalSpeed) > 0.1f)
            {
                // Preserve dynamic speed direction, but allow input to influence it
                float combinedSpeed = currentHorizontalSpeed + targetSpeed;
                // Clamp to prevent exceeding dash speed in the same direction
                if (Mathf.Sign(combinedSpeed) == Mathf.Sign(currentHorizontalSpeed))
                {
                    combinedSpeed = Mathf.Clamp(combinedSpeed, -Mathf.Abs(dashForce), Mathf.Abs(dashForce));
                }
                rb.linearVelocity = new Vector2(combinedSpeed, rb.linearVelocity.y);
            }
            else
            {
                // Normal movement when no dynamic speed
                rb.linearVelocity = new Vector2(targetSpeed, rb.linearVelocity.y);
            }
        }
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

    private void UpdateDash()
    {
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false;
                dashCooldownTimer = dashCooldown;
            }
        }
        else if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
    }

    private void UpdateJumpState()
    {
        bool groundedNow = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayerMask);

        switch (jumpState)
        {
            case JumpState.Grounded:
                if (!groundedNow) jumpState = JumpState.Falling;
                else
                {
                    // Reset dynamic speed when landing (optional - you can remove this if you want speed to persist)
                    // currentHorizontalSpeed = 0f;
                }
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
        // Use dynamic speed if available, otherwise use current velocity
        float jumpHorizontalSpeed = Mathf.Abs(currentHorizontalSpeed) > 0.1f ? currentHorizontalSpeed : rb.linearVelocity.x;
        rb.linearVelocity = new Vector2(jumpHorizontalSpeed, jumpForce);
        if (IsAnimatorValid())
            animator.SetTrigger(triggerJumping);
    }

    private void DoubleJump()
    {
        // Use dynamic speed if available, otherwise use current velocity
        float jumpHorizontalSpeed = Mathf.Abs(currentHorizontalSpeed) > 0.1f ? currentHorizontalSpeed : rb.linearVelocity.x;
        rb.linearVelocity = new Vector2(jumpHorizontalSpeed, doubleJumpForce);
        if (IsAnimatorValid())
            animator.SetTrigger(triggerDoubleJumping);
    }

    private void Dash()
    {
        // Determine dash direction: use input direction, or facing direction if no input
        dashDirection = Mathf.Abs(horizontalInput) > 0.1f ? Mathf.Sign(horizontalInput) : 
                       (modelTransform != null && modelTransform.localScale.x < 0 ? -1f : 1f);
        
        isDashing = true;
        dashTimer = dashDuration;
        
        if (IsAnimatorValid())
            animator.SetTrigger(triggerDashing);
    }

    private void RestartGame()
    {
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
        if (!IsAnimatorValid()) return;

        AnimationState targetState = DetermineAnimationState();

        if (targetState != currentState)
        {
            TriggerAnimation(targetState);
            currentState = targetState;
        }
    }

    /// <summary>
    /// Checks if the animator exists and has a valid AnimatorController assigned
    /// </summary>
    private bool IsAnimatorValid()
    {
        if (animator == null) return false;
        if (animator.runtimeAnimatorController == null) return false;
        return true;
    }


    private AnimationState DetermineAnimationState()
    {
        float absSpeed = Mathf.Abs(horizontalInput * currentMoveSpeed);

        // Dashing takes highest priority
        if (isDashing) return AnimationState.Dashing;

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
        if (!IsAnimatorValid()) return;
        
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
            case AnimationState.Dashing: animator.SetTrigger(triggerDashing); break;
        }
    }
    
    private void ResetAllTriggers()
    {
        if (!IsAnimatorValid()) return;
        
        animator.ResetTrigger(triggerIdle);
        animator.ResetTrigger(triggerWalking);
        animator.ResetTrigger(triggerRunning);
        animator.ResetTrigger(triggerCrouching);
        animator.ResetTrigger(triggerWalkCrouching);
        animator.ResetTrigger(triggerJumping);
        animator.ResetTrigger(triggerDoubleJumping);
        animator.ResetTrigger(triggerFalling);
        animator.ResetTrigger(triggerDashing);
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
