using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float damage = 10f;
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem trailParticles;
    [SerializeField] private ParticleSystem hitParticles;
    [SerializeField] private GameObject hitEffectPrefab;
    
    [Header("Collision")]
    [SerializeField] private LayerMask enemyLayerMask;
    [SerializeField] private float collisionRadius = 0.2f;
    
    private Transform target;
    private Vector2 direction;
    private bool hasTarget = false;
    private float lifetimeTimer;
    private bool hasHit = false;

    private void Start()
    {
        lifetimeTimer = lifetime;
        
        // Start trail particles if available
        if (trailParticles != null && !trailParticles.isPlaying)
        {
            trailParticles.Play();
        }
    }

    private void Update()
    {
        if (hasHit) return;

        lifetimeTimer -= Time.deltaTime;
        if (lifetimeTimer <= 0f)
        {
            DestroyProjectile();
            return;
        }

        // Move toward target if we have one, otherwise use direction
        if (hasTarget && target != null)
        {
            // Update direction to track moving target
            direction = (target.position - transform.position).normalized;
            transform.position += (Vector3)(direction * speed * Time.deltaTime);
            
            // Rotate projectile to face movement direction
            if (direction != Vector2.zero)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
            
            // Check if we've reached the target
            float distanceToTarget = Vector2.Distance(transform.position, target.position);
            if (distanceToTarget < collisionRadius)
            {
                HitTarget(target);
            }
        }
        else if (direction != Vector2.zero)
        {
            // Move in fixed direction
            transform.position += (Vector3)(direction * speed * Time.deltaTime);
            
            // Rotate projectile to face movement direction
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        // Check for collisions with enemies
        CheckCollisions();
    }

    public void SetTarget(Transform targetTransform)
    {
        target = targetTransform;
        hasTarget = true;
        if (target != null)
        {
            direction = (target.position - transform.position).normalized;
        }
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
        hasTarget = false;
    }

    public void SetDamage(float dmg)
    {
        damage = dmg;
    }

    private void CheckCollisions()
    {
        // Check for enemies in range
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, collisionRadius, enemyLayerMask);
        
        foreach (Collider2D hit in hits)
        {
            EnemyAI enemy = hit.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                HitTarget(enemy.transform);
                break; // Only hit one enemy at a time
            }
        }
    }

    private void HitTarget(Transform targetTransform)
    {
        if (hasHit) return;
        hasHit = true;

        // Deal damage to enemy
        EnemyAI enemy = targetTransform.GetComponent<EnemyAI>();
        if (enemy != null)
        {
            enemy.OnAttacked(damage);
        }

        // Spawn hit particles
        if (hitParticles != null)
        {
            ParticleSystem particles = Instantiate(hitParticles, transform.position, Quaternion.identity);
            particles.Play();
            Destroy(particles.gameObject, particles.main.duration + particles.main.startLifetime.constantMax);
        }

        // Spawn hit effect prefab if available
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 2f); // Clean up after 2 seconds
        }

        DestroyProjectile();
    }

    private void DestroyProjectile()
    {
        // Stop trail particles
        if (trailParticles != null)
        {
            trailParticles.Stop();
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, collisionRadius);
    }
}

