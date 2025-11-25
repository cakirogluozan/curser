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

    private Rigidbody2D rb;
    private Transform player;
    private bool movingRight = true;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        if (PlayerInRange())
            ChasePlayer();
        else
            Patrol();
    }

    void Patrol()
    {
        rb.linearVelocity = new Vector2((movingRight ? 1 : -1) * patrolSpeed, rb.linearVelocity.y);

        // Check for edge
        if (!Physics2D.Raycast(groundCheck.position, Vector2.down, 1f, groundLayer))
            Flip();
    }

    void Flip()
    {
        movingRight = !movingRight;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
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
            transform.localScale = new Vector3(movingRight ? Mathf.Abs(transform.localScale.x) : -Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    public void OnAttacked(float damage)
    {
        // Rotate enemy 90 degrees when attacked
        transform.Rotate(0f, 0f, 90f);
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
