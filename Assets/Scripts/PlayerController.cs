using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    
    [Header("Starting Position")]
    [SerializeField] private bool resetPositionOnStart = true;
    [SerializeField] private Vector2 startPosition = Vector2.zero;
    
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayerMask;
    
    [Header("Animation (Optional)")]
    [SerializeField] private PlayerAnimator playerAnimator;
    
    private Rigidbody2D rb;
    private bool isGrounded;
    private float horizontalInput;
    private bool jumpPressed;
    private bool justJumped;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Reset position immediately in Awake (before physics updates)
        if (resetPositionOnStart)
        {
            Vector3 newPos = new Vector3(startPosition.x, startPosition.y, transform.position.z);
            transform.position = newPos;
            if (rb != null)
            {
                rb.position = startPosition;
            }
        }
    }
    
    void Start()
    {
        // Ensure position is set (in case Awake didn't run or Rigidbody wasn't ready)
        if (resetPositionOnStart && rb != null)
        {
            rb.position = startPosition;
        }
        
        // Get PlayerAnimator if not assigned
        if (playerAnimator == null)
        {
            playerAnimator = GetComponent<PlayerAnimator>();
        }
        
        // Create ground check point if not assigned
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
        // Get horizontal input from new Input System
        horizontalInput = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                horizontalInput = -1f;
            }
            else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                horizontalInput = 1f;
            }
            
            // Check jump input
            jumpPressed = Keyboard.current.spaceKey.wasPressedThisFrame;
        }
        
        // Check if grounded
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayerMask);
        
        // Jump input
        if (jumpPressed && isGrounded)
        {
            Jump();
            justJumped = true;
        }
        else
        {
            justJumped = false;
        }
        
        // Update animations
        if (playerAnimator != null)
        {
            playerAnimator.UpdateAnimations(horizontalInput * moveSpeed, isGrounded, justJumped);
        }
    }
    
    void FixedUpdate()
    {
        // Apply horizontal movement
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }
    
    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw ground check radius in editor
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}

