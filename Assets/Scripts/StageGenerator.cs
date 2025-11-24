using UnityEngine;

public class StageGenerator : MonoBehaviour
{
    [Header("Platform Settings")]
    [SerializeField] private GameObject platformPrefab;
    [SerializeField] private int numberOfPlatforms = 10;
    [SerializeField] private float platformSpacing = 3f;
    [SerializeField] private float platformWidth = 2f;
    [SerializeField] private float platformHeight = 0.5f;
    
    [Header("Height Variation")]
    [SerializeField] private float minY = 0f;
    [SerializeField] private float maxY = 5f;
    [SerializeField] private float heightChangeChance = 0.3f;
    
    [Header("Starting Position")]
    [SerializeField] private Vector2 startPosition = Vector2.zero;
    
    void Start()
    {
        GenerateStage();
    }
    
    void GenerateStage()
    {
        // Create platform prefab if not assigned
        if (platformPrefab == null)
        {
            CreateDefaultPlatformPrefab();
        }
        
        float currentX = startPosition.x;
        float currentY = startPosition.y;
        
        for (int i = 0; i < numberOfPlatforms; i++)
        {
            // Randomly change height
            if (Random.value < heightChangeChance && i > 0)
            {
                currentY = Random.Range(minY, maxY);
            }
            
            // Instantiate platform
            Vector3 platformPosition = new Vector3(currentX, currentY, 0);
            GameObject platform = Instantiate(platformPrefab, platformPosition, Quaternion.identity);
            platform.transform.localScale = new Vector3(platformWidth, platformHeight, 1f);
            platform.name = $"Platform_{i}";
            
            // Move to next position
            currentX += platformSpacing;
        }
    }
    
    void CreateDefaultPlatformPrefab()
    {
        // Create a simple platform GameObject
        GameObject platform = new GameObject("Platform");
        
        // Add SpriteRenderer
        SpriteRenderer sr = platform.AddComponent<SpriteRenderer>();
        sr.color = Color.green;
        
        // Create a simple white sprite
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100);
        sr.sprite = sprite;
        
        // Add BoxCollider2D for physics
        BoxCollider2D collider = platform.AddComponent<BoxCollider2D>();
        
        platformPrefab = platform;
    }
    
    // Method to regenerate stage (can be called from editor or runtime)
    [ContextMenu("Regenerate Stage")]
    public void RegenerateStage()
    {
        // Destroy existing platforms
        foreach (Transform child in transform)
        {
            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }
        
        GenerateStage();
    }
}

