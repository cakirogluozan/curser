using UnityEngine;

public class ParallaxFollow : MonoBehaviour
{
    public Transform target; // Assign your Cinemachine virtual camera's transform or the player
    public float parallaxMultiplier = 0.5f;

    private Vector3 lastTargetPos;

    private void Start()
    {
        if (target == null)
        {
            Debug.LogError("ParallaxFollow: Target not assigned!");
            enabled = false;
            return;
        }

        // Validate target position to prevent Infinity values
        if (float.IsInfinity(target.position.x) || float.IsInfinity(target.position.y) || float.IsInfinity(target.position.z))
        {
            Debug.LogError("ParallaxFollow: Target position contains Infinity values. Cannot initialize.");
            enabled = false;
            return;
        }

        lastTargetPos = target.position;
    }

    private void LateUpdate()
    {
        // Validate target position to prevent Infinity values
        if (float.IsInfinity(target.position.x) || float.IsInfinity(target.position.y) || float.IsInfinity(target.position.z))
        {
            Debug.LogWarning("ParallaxFollow: Target position contains Infinity values. Skipping update.");
            return;
        }

        Vector3 delta = target.position - lastTargetPos;
        
        // Validate delta to prevent Infinity values
        if (float.IsInfinity(delta.x) || float.IsInfinity(delta.y))
        {
            Debug.LogWarning("ParallaxFollow: Delta contains Infinity values. Skipping update.");
            return;
        }

        // Only modify X and Y, never touch Z
        Vector3 currentPos = transform.position;
        transform.position = new Vector3(
            currentPos.x + delta.x * parallaxMultiplier, 
            currentPos.y + delta.y * parallaxMultiplier, 
            currentPos.z  // Z remains completely unchanged
        );
        
        lastTargetPos = target.position;
    }
}
