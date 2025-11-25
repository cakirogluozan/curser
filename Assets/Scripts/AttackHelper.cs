using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class AttackHelper : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private Transform playerTransform;

    [Header("Position Settings")]
    [SerializeField] private Vector2 offsetPosition = new Vector2(-0.5f, 0.5f); // Top left offset relative to player

    [Header("Attack Settings")]
    [SerializeField] private bool useMouseForAttack = true; // Use mouse button for attack
    [SerializeField] private Key attackKey = Key.F; // Keyboard key for attack (if not using mouse)
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private LayerMask enemyLayerMask; // Layer mask for enemies
    [SerializeField] private bool automaticFire = true; // If true, attacks continuously while holding button
    [SerializeField] private float fireRate = 10f; // Attacks per second (for automatic fire)
    
    [Header("Projectile Settings")]
    [SerializeField] private bool useProjectiles = true; // Use projectiles instead of instant hit
    [SerializeField] private GameObject projectilePrefab; // Prefab for projectile (should have Projectile component)
    [SerializeField] private Transform projectileSpawnPoint; // Where to spawn projectiles (if null, uses transform.position)
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private bool trackTarget = true; // If true, projectile tracks enemy movement

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string attackStateName = "Attacking"; // Name of the attack state in animator
    [SerializeField] private string triggerAttacking = "TriggerAttacking";

    [Header("Model Settings")]
    [SerializeField] private Transform modelTransform;
    [SerializeField] private bool flipModel = true;
    [SerializeField] private bool preserveOriginalScale = true;
    private Vector3 originalModelScale;

    [Header("Particle Effects")]
    [SerializeField] private FairyAttackFX fairyAttackFX; // Reference to FairyAttackFX component

    // Attack state
    private bool isAttacking = false;
    private float attackCooldownTimer = 0f;
    private float fireRateTimer = 0f; // Timer for automatic fire rate

    private void Awake()
    {
        // Find player if not assigned
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerTransform = playerObj.transform;
        }

        // Find animator if not assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
                animator = GetComponentInChildren<Animator>(true);
        }

        // Find model transform if not assigned
        if (modelTransform == null)
        {
            MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
            modelTransform = renderers.Length > 0 ? renderers[0].transform : transform;
        }

        if (modelTransform != null && preserveOriginalScale)
            originalModelScale = modelTransform.localScale;

        // Find FairyAttackFX if not assigned
        if (fairyAttackFX == null)
        {
            fairyAttackFX = GetComponent<FairyAttackFX>();
            if (fairyAttackFX == null)
                fairyAttackFX = GetComponentInChildren<FairyAttackFX>(true);
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;

        // Follow player with offset
        UpdatePosition();

        // Handle attack input
        HandleAttackInput();

        // Update attack state
        UpdateAttack();
    }

    private void UpdatePosition()
    {
        // Calculate position based on player's facing direction
        float playerFacingDirection = 1f; // Default right
        if (playerTransform.localScale.x < 0)
            playerFacingDirection = -1f;

        // Apply offset (top left relative to player)
        Vector2 offset = new Vector2(offsetPosition.x * playerFacingDirection, offsetPosition.y);
        transform.position = (Vector2)playerTransform.position + offset;
    }

    private void HandleAttackInput()
    {
        // Check if attack button is being held/pressed
        bool attackInputHeld = false;
        bool attackInputPressed = false;

        if (useMouseForAttack && Mouse.current != null)
        {
            if (automaticFire)
            {
                attackInputHeld = Mouse.current.leftButton.isPressed;
            }
            else
            {
                attackInputPressed = Mouse.current.leftButton.wasPressedThisFrame;
            }
        }
        else if (!useMouseForAttack && attackKey != Key.None && Keyboard.current != null)
        {
            if (automaticFire)
            {
                attackInputHeld = Keyboard.current[attackKey].isPressed;
            }
            else
            {
                attackInputPressed = Keyboard.current[attackKey].wasPressedThisFrame;
            }
        }

        // Handle automatic fire (continuous attacks while holding)
        if (automaticFire && attackInputHeld)
        {
            // Check fire rate timer
            if (fireRateTimer <= 0f && attackCooldownTimer <= 0f)
            {
                Attack();
                fireRateTimer = 1f / fireRate; // Set timer based on fire rate
            }
        }
        // Handle single shot (one attack per press)
        else if (!automaticFire && attackInputPressed)
        {
            if (!isAttacking && attackCooldownTimer <= 0f)
            {
                Attack();
            }
        }

        // Update fire rate timer
        if (fireRateTimer > 0f)
        {
            fireRateTimer -= Time.deltaTime;
        }
    }

    private void UpdateAttack()
    {
        if (attackCooldownTimer > 0f)
        {
            attackCooldownTimer -= Time.deltaTime;
        }

        // Check if attack animation is still playing
        if (isAttacking && IsAnimatorValid())
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            
            // Check if we're in the attacking state (check by name or tag)
            bool isInAttackState = !string.IsNullOrEmpty(attackStateName) && stateInfo.IsName(attackStateName) ||
                                   stateInfo.IsTag("Attack");
            
            // If we're in attack state, keep isAttacking true until animation finishes
            if (isInAttackState)
            {
                // Animation finished when normalizedTime >= 1
                // normalizedTime represents progress: 0 = start, 1 = end, >1 = looping
                if (stateInfo.normalizedTime >= 1f && !stateInfo.loop)
                {
                    isAttacking = false;
                }
            }
            else if (!isInAttackState && stateInfo.normalizedTime > 0.1f)
            {
                // If we've transitioned out of attack state (and some time has passed), reset
                // Small delay (0.1f) prevents immediate reset during transition
                isAttacking = false;
            }
        }
    }

    private void Attack()
    {
        isAttacking = true;
        attackCooldownTimer = attackCooldown;

        // Find closest enemy within attack range
        EnemyAI closestEnemy = FindClosestEnemy();

        if (closestEnemy != null)
        {
            // Face the enemy
            bool enemyIsRight = closestEnemy.transform.position.x > transform.position.x;
            if (flipModel && modelTransform != null)
            {
                Vector3 scale = preserveOriginalScale ? originalModelScale : modelTransform.localScale;
                scale.x = enemyIsRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
                modelTransform.localScale = scale;
            }

            // Play magic burst particle effect (pass enemy target and damage so it moves toward enemy and triggers rotation)
            if (fairyAttackFX != null)
            {
                fairyAttackFX.PlayMagicFX(closestEnemy.transform, attackDamage);
            }

            if (useProjectiles && projectilePrefab != null)
            {
                // Spawn projectile
                SpawnProjectile(closestEnemy.transform);
            }
            else
            {
                // Instant hit (original behavior) - damage only, rotation handled by particle effect
                Debug.Log($"Attacked enemy at {closestEnemy.transform.position} for {attackDamage} damage!");
                // Note: Rotation is handled by the particle effect when it hits, not here
            }
        }

        if (IsAnimatorValid())
            animator.SetTrigger(triggerAttacking);
    }

    private void SpawnProjectile(Transform target)
    {
        // Determine spawn position
        Vector3 spawnPos = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position;
        
        // Instantiate projectile
        GameObject projectileObj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        Projectile projectile = projectileObj.GetComponent<Projectile>();
        
        if (projectile != null)
        {
            // Configure projectile
            projectile.SetDamage(attackDamage);
            
            if (trackTarget && target != null)
            {
                // Set target for tracking
                projectile.SetTarget(target);
            }
            else
            {
                // Set direction toward target
                Vector2 direction = (target.position - spawnPos).normalized;
                projectile.SetDirection(direction);
            }
        }
        else
        {
            Debug.LogWarning("Projectile prefab doesn't have Projectile component! Adding basic movement.");
            // Fallback: Add simple projectile behavior
            StartCoroutine(MoveProjectileToTarget(projectileObj, target));
        }
    }

    private System.Collections.IEnumerator MoveProjectileToTarget(GameObject projectile, Transform target)
    {
        while (projectile != null && target != null)
        {
            Vector2 direction = (target.position - projectile.transform.position).normalized;
            projectile.transform.position += (Vector3)(direction * projectileSpeed * Time.deltaTime);
            
            // Rotate to face direction
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            projectile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            
            // Check if reached target
            if (Vector2.Distance(projectile.transform.position, target.position) < 0.2f)
            {
                EnemyAI enemy = target.GetComponent<EnemyAI>();
                if (enemy != null)
                {
                    enemy.OnAttacked(attackDamage);
                }
                Destroy(projectile);
                yield break;
            }
            
            yield return null;
        }
    }

    private EnemyAI FindClosestEnemy()
    {
        EnemyAI closestEnemy = null;
        float closestDistance = float.MaxValue;

        // Find all enemies using the EnemyAI component
        EnemyAI[] allEnemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);

        foreach (EnemyAI enemy in allEnemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy)
                continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);

            // Check if enemy is within attack range
            if (distance <= attackRange && distance < closestDistance)
            {
                // Optional: Check layer mask if specified
                if (enemyLayerMask.value == 0 || ((1 << enemy.gameObject.layer) & enemyLayerMask.value) != 0)
                {
                    closestEnemy = enemy;
                    closestDistance = distance;
                }
            }
        }

        return closestEnemy;
    }

    private bool IsAnimatorValid()
    {
        if (animator == null) return false;
        if (animator.runtimeAnimatorController == null) return false;
        return true;
    }

    void OnDrawGizmosSelected()
    {
        // Draw attack range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

