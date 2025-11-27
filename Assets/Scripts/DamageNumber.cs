using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DamageNumber : MonoBehaviour
{
    [Header("Text Settings")]
    [SerializeField] private Text damageText;
    
    [Header("Animation Settings")]
    [SerializeField] private float floatDistance = 1f;
    [SerializeField] private float lifetime = 1f;
    [SerializeField] private AnimationCurve fadeCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));
    [SerializeField] private AnimationCurve scaleCurve = new AnimationCurve(new Keyframe(0f, 1.2f), new Keyframe(1f, 1f));
    
    [Header("Color Settings")]
    [SerializeField] private Color normalDamageColor = Color.white;
    [SerializeField] private Color criticalDamageColor = Color.yellow;
    [SerializeField] private bool useCriticalColors = false;
    [SerializeField] private float criticalThreshold = 20f; // Damage above this shows as critical
    
    private float damageValue;
    private Vector3 startPosition;
    private Vector3 targetRelativeOffset = Vector3.zero;
    private float timer = 0f;
    private Canvas canvas;
    private RectTransform rectTransform;
    private Camera mainCamera;
    private Transform followTarget;
    private Vector3 initialScale = Vector3.one;
    private Vector3 baseScaleVector = Vector3.one;

    private const float TopOffsetPadding = 0.1f;
    private const float ReferenceHeight = 1f;
    private const float MinScaleMultiplier = 0.5f;
    private const float MaxScaleMultiplier = 2.5f;
    private const float HorizontalOffsetPerHit = 0.2f; // Horizontal spacing between multiple damage numbers
    
    // Static dictionary to track total hit count per enemy (for horizontal offset)
    private static Dictionary<Transform, int> totalHitCounts = new Dictionary<Transform, int>();

    private void Awake()
    {
        // Find canvas (should be on this GameObject)
        canvas = GetComponent<Canvas>();
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
            rectTransform = gameObject.AddComponent<RectTransform>();

        if (rectTransform != null)
        {
            if (rectTransform.localScale == Vector3.zero)
                rectTransform.localScale = new Vector3(0.00125f, 0.00125f, 0.00125f);

            initialScale = rectTransform.localScale;
            baseScaleVector = initialScale;
        }

        mainCamera = Camera.main;
        if (mainCamera == null)
            mainCamera = FindFirstObjectByType<Camera>();

        // Find text component
        if (damageText == null)
            damageText = GetComponentInChildren<Text>();
    }

    public void Initialize(float damage, Vector3 worldPosition, bool isCritical = false, Transform target = null)
    {
        damageValue = damage;
        followTarget = target;
        targetRelativeOffset = Vector3.zero;
        baseScaleVector = initialScale == Vector3.zero ? new Vector3(0.00125f, 0.00125f, 0.00125f) : initialScale;
        
        // Calculate horizontal offset for multiple hits on same enemy
        float horizontalOffset = CalculateHorizontalOffset(target);
        
        ConfigureTargetTracking(worldPosition);
        
        // Apply horizontal offset AFTER ConfigureTargetTracking (so it doesn't get overwritten)
        targetRelativeOffset.x = horizontalOffset;
        startPosition.x += horizontalOffset;
        
        // Debug: Verify damage value is received
        Debug.Log($"DamageNumber initialized with damage: {damage} at position: {worldPosition}");
        
        // Set text
        string damageString = Mathf.RoundToInt(damage).ToString();
        if (damageText != null)
        {
            damageText.text = damageString;
            // Set color based on critical or normal
            if (useCriticalColors && (isCritical || damage >= criticalThreshold))
            {
                damageText.color = criticalDamageColor;
                damageText.fontSize = Mathf.RoundToInt(damageText.fontSize * 1.2f); // Make critical damage bigger
            }
            else
            {
                damageText.color = normalDamageColor;
            }
        }

        // Position the canvas at world position
        Vector3 initialWorldPosition = startPosition;

        if (canvas != null)
        {
            canvas.transform.position = initialWorldPosition;
            canvas.transform.rotation = Quaternion.identity;
        }
        else if (rectTransform != null)
        {
            rectTransform.position = initialWorldPosition;
            rectTransform.rotation = Quaternion.identity;
        }

        // Start animation
        StartCoroutine(AnimateDamageNumber());
    }

    private IEnumerator AnimateDamageNumber()
    {
        timer = 0f;

        while (timer < lifetime)
        {
            timer += Time.deltaTime;
            float normalizedTime = timer / lifetime;

            // Calculate position (float upward)
            float floatOffset = floatDistance * normalizedTime;
            Vector3 basePosition = followTarget != null ? followTarget.position + targetRelativeOffset : startPosition;
            if (followTarget != null)
                startPosition = basePosition;

            Vector3 currentPosition = basePosition + Vector3.up * floatOffset;

            // Update canvas position
            if (canvas != null)
            {
                canvas.transform.position = currentPosition;
                
                // Face camera
                if (mainCamera != null)
                {
                    canvas.transform.LookAt(canvas.transform.position + mainCamera.transform.rotation * Vector3.forward,
                                          mainCamera.transform.rotation * Vector3.up);
                }
            }

            // Apply fade
            float alpha = fadeCurve.Evaluate(normalizedTime);
            if (damageText != null)
            {
                Color color = damageText.color;
                color.a = alpha;
                damageText.color = color;
            }

            // Apply scale
            float scale = scaleCurve.Evaluate(normalizedTime);
            if (rectTransform != null)
            {
                rectTransform.localScale = baseScaleVector * scale;
            }

            yield return null;
        }
        
        // Destroy after animation (no need to remove from tracking since we track total hits, not active ones)
        Destroy(gameObject);
    }
    
    private float CalculateHorizontalOffset(Transform target)
    {
        if (target == null)
            return 0f;
        
        // Get current total hit count for this enemy (all hits, not just active ones)
        int hitCount = 0;
        if (totalHitCounts.ContainsKey(target))
        {
            hitCount = totalHitCounts[target];
        }
        
        // Increment total hit count for this enemy
        totalHitCounts[target] = hitCount + 1;
        
        // Return random offset between -HorizontalOffsetPerHit and +HorizontalOffsetPerHit
        return Random.Range(-HorizontalOffsetPerHit, HorizontalOffsetPerHit);
    }

    // Static method to create damage number easily
    public static void CreateDamageNumber(float damage, Vector3 worldPosition, GameObject prefab = null, bool isCritical = false)
    {
        CreateDamageNumber(damage, worldPosition, prefab, isCritical, null);
    }

    public static void CreateDamageNumber(float damage, Vector3 worldPosition, GameObject prefab, bool isCritical, Transform target)
    {
        GameObject damageObj;

        if (prefab != null)
        {
            damageObj = Instantiate(prefab, worldPosition, Quaternion.identity);
        }
        else
        {
            // Create default damage number if no prefab provided
            damageObj = CreateDefaultDamageNumber();
        }

        DamageNumber damageNumber = damageObj.GetComponent<DamageNumber>();
        if (damageNumber == null)
            damageNumber = damageObj.AddComponent<DamageNumber>();

        damageNumber.Initialize(damage, worldPosition, isCritical, target);
    }

    private void ConfigureTargetTracking(Vector3 desiredPosition)
    {
        if (followTarget == null)
        {
            startPosition = desiredPosition;
            return;
        }

        Vector3 targetPosition = followTarget.position;
        targetRelativeOffset = desiredPosition - targetPosition;

        if (TryGetTargetBounds(followTarget, out Bounds targetBounds))
        {
            float topOffset = targetBounds.extents.y + TopOffsetPadding;
            targetRelativeOffset.y = Mathf.Max(targetRelativeOffset.y, topOffset);
            ApplyScaleFromHeight(targetBounds.size.y);
        }
        else
        {
            float approximateHeight = Mathf.Max(followTarget.lossyScale.y, 0.1f);
            float topOffset = (approximateHeight * 0.5f) + TopOffsetPadding;
            targetRelativeOffset.y = Mathf.Max(targetRelativeOffset.y, topOffset);
            ApplyScaleFromHeight(approximateHeight);
        }

        startPosition = targetPosition + targetRelativeOffset;
    }

    private static bool TryGetTargetBounds(Transform target, out Bounds bounds)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length > 0)
        {
            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);
            return true;
        }

        Collider[] colliders = target.GetComponentsInChildren<Collider>(true);
        if (colliders.Length > 0)
        {
            bounds = colliders[0].bounds;
            for (int i = 1; i < colliders.Length; i++)
                bounds.Encapsulate(colliders[i].bounds);
            return true;
        }

        bounds = default;
        return false;
    }

    private void ApplyScaleFromHeight(float height)
    {
        float safeHeight = Mathf.Max(height, 0.1f);
        float scaleMultiplier = Mathf.Clamp(safeHeight / ReferenceHeight, MinScaleMultiplier, MaxScaleMultiplier);
        Vector3 safeInitialScale = initialScale == Vector3.zero ? new Vector3(0.00125f, 0.00125f, 0.00125f) : initialScale;
        baseScaleVector = safeInitialScale * scaleMultiplier;
    }

    private static GameObject CreateDefaultDamageNumber()
    {
        // Create canvas
        GameObject canvasObj = new GameObject("DamageNumber");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 100; // Ensure it renders on top
        
        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        // Set canvas size (in pixels)
        canvasRect.sizeDelta = new Vector2(80f, 40f);
        // Scale down for world space - 2x smaller
        canvasRect.localScale = new Vector3(0.00125f, 0.00125f, 0.00125f);

        // Create text object
        GameObject textObj = new GameObject("DamageText");
        textObj.transform.SetParent(canvasObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        // Add Text component
        Text text = textObj.AddComponent<Text>();
        text.text = "0";
        text.fontSize = 200; // Font size (will appear smaller due to reduced scale)
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontStyle = FontStyle.Bold;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        // Add DamageNumber component
        DamageNumber damageNumber = canvasObj.AddComponent<DamageNumber>();
        damageNumber.damageText = text;

        return canvasObj;
    }
}
