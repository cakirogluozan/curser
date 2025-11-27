using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Speed Settings")]
    public float patrolSpeed = 2f;
    public float alertedSpeed = 3f;
    
    [Header("Detection Settings")]
    public float detectionRange = 5f;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public LayerMask playerLayer;

    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    
    [Header("Damage Display")]
    [SerializeField] private GameObject damageNumberPrefab; // Optional prefab for damage numbers
    [SerializeField] private Vector3 damageNumberOffset = new Vector3(0f, 1f, 0f); // Offset above enemy

    private Rigidbody2D rb;
    private Transform player;
    private bool movingRight = true;
    private float currentHealth;
    private bool isDead = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentHealth = maxHealth;
    }

    private void Update()
    {
        // Don't update if enemy is dead
        if (isDead) return;
        
        if (PlayerInRange())
            ChasePlayer();
        else
            Patrol();
    }

    void Patrol()
    {
        rb.linearVelocity = new Vector2((movingRight ? 1 : -1) * patrolSpeed, rb.linearVelocity.y);
        
        // Update rotation based on movement direction
        UpdateRotation();

        // Check for edge
        if (!Physics2D.Raycast(groundCheck.position, Vector2.down, 1f, groundLayer))
            Flip();
    }

    void Flip()
    {
        movingRight = !movingRight;
        UpdateRotation();
    }
    
    void UpdateRotation()
    {
        // Rotate enemy to face movement direction (flip horizontally)
        // Y rotation: 0 degrees = facing right, 180 degrees = facing left
        float targetRotationY = movingRight ? 0f : 180f;
        transform.rotation = Quaternion.Euler(0f, targetRotationY, 0f);
    }

    bool PlayerInRange()
    {
        float dist = Vector2.Distance(transform.position, player.position);
        return dist <= detectionRange;
    }

    void ChasePlayer()
    {
        float direction = player.position.x > transform.position.x ? 1 : -1;
        rb.linearVelocity = new Vector2(direction * alertedSpeed, rb.linearVelocity.y);
        
        // Update facing direction based on chase direction
        bool shouldFaceRight = direction > 0;
        if (shouldFaceRight != movingRight)
        {
            movingRight = shouldFaceRight;
            UpdateRotation();
        }
    }

    public void OnAttacked(float damage)
    {
        // Don't process damage if already dead
        if (isDead) return;
        
        // Decrease health
        currentHealth -= damage;
        
        // Show damage number above enemy
        Vector3 damagePosition = transform.position + damageNumberOffset;
        DamageNumber.CreateDamageNumber(damage, damagePosition, damageNumberPrefab, false, transform);
        
        // Check if enemy is dead
        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Die();
        }
    }
    
    private void Die()
    {
        if (isDead) return; // Prevent multiple death calls
        
        isDead = true;
        
        // Stop movement
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        // Disable AI behavior
        enabled = false;
        
        // Destroy the enemy GameObject
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Turn when hitting walls or borders (check if collision is on ground layer)
        if (groundLayer.value != 0 && ((1 << collision.gameObject.layer) & groundLayer.value) != 0)
        {
            Flip();
        }
    }
}
