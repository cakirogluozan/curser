using UnityEngine;
using System.Collections;

public class FairyAttackFX : MonoBehaviour
{
    public GameObject magicBurstPrefab;
    public Transform firePoint;
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public bool moveToTarget = true; // If true, particle moves toward target

    public void PlayMagicFX()
    {
        PlayMagicFX(null);
    }

    public void PlayMagicFX(Transform target)
    {
        PlayMagicFX(target, 10f); // Default damage
    }

    public void PlayMagicFX(Transform target, float damage)
    {
        if (magicBurstPrefab != null)
        {
            Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
            GameObject fxInstance = Instantiate(magicBurstPrefab, spawnPosition, Quaternion.identity);
            
            // If target is provided and we want to move to target, start coroutine
            if (target != null && moveToTarget)
            {
                StartCoroutine(MoveFXToTarget(fxInstance, target, damage));
            }
        }
    }

    private IEnumerator MoveFXToTarget(GameObject fxInstance, Transform target, float damage)
    {
        if (fxInstance == null || target == null) yield break;

        // Get the particle system if it exists
        ParticleSystem particles = fxInstance.GetComponent<ParticleSystem>();
        bool hasParticles = particles != null;

        // Move toward target
        while (fxInstance != null && target != null)
        {
            Vector3 direction = (target.position - fxInstance.transform.position).normalized;
            fxInstance.transform.position += direction * moveSpeed * Time.deltaTime;

            // Rotate to face direction
            if (direction != Vector3.zero)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                fxInstance.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }

            // Check if reached target
            float distanceToTarget = Vector3.Distance(fxInstance.transform.position, target.position);
            if (distanceToTarget < 0.3f)
            {
                // Hit the enemy - trigger rotation and damage
                EnemyAI enemy = target.GetComponent<EnemyAI>();
                if (enemy != null)
                {
                    enemy.OnAttacked(damage);
                }

                // If particle system exists, stop it and let it finish
                if (hasParticles && particles != null)
                {
                    particles.Stop();
                    // Wait for particles to finish before destroying
                    yield return new WaitForSeconds(particles.main.duration + particles.main.startLifetime.constantMax);
                }
                Destroy(fxInstance);
                yield break;
            }

            yield return null;
        }
    }
}

