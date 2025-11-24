using UnityEngine;

/// <summary>
/// Handles player animations based on movement state
/// Works with both 2D sprites and 3D models
/// Requires an Animator component with the following parameters:
/// - "Speed" (float): Horizontal movement speed
/// - "IsGrounded" (bool): Whether player is on ground
/// - "Jump" (trigger): Triggered when player jumps
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    [Header("Model Type")]
    [SerializeField] private bool use3DModel = false;
    
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Transform modelTransform;
    
    // Animation parameter names
    private const string SPEED_PARAM = "Speed";
    private const string IS_GROUNDED_PARAM = "IsGrounded";
    private const string JUMP_TRIGGER = "Jump";
    
    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // For 3D models, find the model child or use this transform
        if (use3DModel)
        {
            // Try to find a child with a mesh renderer (the actual 3D model)
            MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
            if (renderers.Length > 0)
            {
                modelTransform = renderers[0].transform;
            }
            else
            {
                modelTransform = transform;
            }
        }
        
        if (animator == null)
        {
            Debug.LogError("PlayerAnimator requires an Animator component!");
        }
    }
    
    public void UpdateAnimations(float speed, bool isGrounded, bool isJumping)
    {
        if (animator == null) return;
        
        // Set speed parameter (absolute value for animation)
        animator.SetFloat(SPEED_PARAM, Mathf.Abs(speed));
        
        // Set grounded state
        animator.SetBool(IS_GROUNDED_PARAM, isGrounded);
        
        // Trigger jump animation
        if (isJumping)
        {
            animator.SetTrigger(JUMP_TRIGGER);
        }
        
        // Flip character based on movement direction
        if (use3DModel && modelTransform != null)
        {
            // Flip 3D model using scale
            Vector3 scale = modelTransform.localScale;
            if (speed > 0.1f)
            {
                scale.x = Mathf.Abs(scale.x); // Face right
            }
            else if (speed < -0.1f)
            {
                scale.x = -Mathf.Abs(scale.x); // Face left
            }
            modelTransform.localScale = scale;
        }
        else if (spriteRenderer != null)
        {
            // Flip 2D sprite
            if (speed > 0.1f)
            {
                spriteRenderer.flipX = false;
            }
            else if (speed < -0.1f)
            {
                spriteRenderer.flipX = true;
            }
        }
    }
}

