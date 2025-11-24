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

        lastTargetPos = target.position;
    }

    private void LateUpdate()
    {
        Vector3 delta = target.position - lastTargetPos;
        transform.position += new Vector3(delta.x * parallaxMultiplier, delta.y * parallaxMultiplier, transform.position.z);
        lastTargetPos = target.position;
    }
}
