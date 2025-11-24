using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Helper component to create animations from sprite sequences
/// Attach this to your Player GameObject to help set up sprite animations
/// </summary>
public class SpriteAnimationHelper : MonoBehaviour
{
    [Header("Animation Clips")]
    [SerializeField] private List<Sprite> idleSprites = new List<Sprite>();
    [SerializeField] private List<Sprite> walkSprites = new List<Sprite>();
    [SerializeField] private List<Sprite> jumpSprites = new List<Sprite>();
    [SerializeField] private List<Sprite> fallSprites = new List<Sprite>();
    
    [Header("Animation Settings")]
    [SerializeField] private float framesPerSecond = 12f;
    [SerializeField] private bool loopAnimations = true;
    
    private SpriteRenderer spriteRenderer;
    private int currentFrame = 0;
    private float timer = 0f;
    private List<Sprite> currentAnimation;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        
        // Start with idle animation
        if (idleSprites.Count > 0)
        {
            PlayAnimation(idleSprites);
        }
    }
    
    void Update()
    {
        if (currentAnimation == null || currentAnimation.Count == 0) return;
        if (spriteRenderer == null) return;
        
        timer += Time.deltaTime;
        float frameTime = 1f / framesPerSecond;
        
        if (timer >= frameTime)
        {
            timer = 0f;
            currentFrame++;
            
            if (currentFrame >= currentAnimation.Count)
            {
                if (loopAnimations)
                {
                    currentFrame = 0;
                }
                else
                {
                    currentFrame = currentAnimation.Count - 1;
                }
            }
            
            spriteRenderer.sprite = currentAnimation[currentFrame];
        }
    }
    
    public void PlayAnimation(List<Sprite> sprites)
    {
        if (sprites == null || sprites.Count == 0) return;
        
        currentAnimation = sprites;
        currentFrame = 0;
        timer = 0f;
        
        if (spriteRenderer != null && sprites.Count > 0)
        {
            spriteRenderer.sprite = sprites[0];
        }
    }
    
    public void PlayIdle()
    {
        if (idleSprites.Count > 0)
        {
            PlayAnimation(idleSprites);
        }
    }
    
    public void PlayWalk()
    {
        if (walkSprites.Count > 0)
        {
            PlayAnimation(walkSprites);
        }
    }
    
    public void PlayJump()
    {
        if (jumpSprites.Count > 0)
        {
            PlayAnimation(jumpSprites);
        }
    }
    
    public void PlayFall()
    {
        if (fallSprites.Count > 0)
        {
            PlayAnimation(fallSprites);
        }
    }
}

