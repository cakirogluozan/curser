using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "SampleScene"; // Name of your game scene
    
    [Header("Audio (Optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip backgroundMusic;

    private void Start()
    {
        // Ensure time scale is normal when menu loads
        Time.timeScale = 1f;
        
        // Play background music if assigned
        if (audioSource != null && backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    /// <summary>
    /// Called when the "Start Game" button is clicked
    /// </summary>
    public void StartGame()
    {
        PlayButtonSound();
        
        // Ensure time scale is normal before loading game scene
        Time.timeScale = 1f;
        
        // Load the game scene
        if (!string.IsNullOrEmpty(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError("MainMenu: Game scene name is not set! Please assign it in the Inspector.");
        }
    }

    /// <summary>
    /// Called when the "Quit Game" button is clicked
    /// </summary>
    public void QuitGame()
    {
        PlayButtonSound();
        
        Debug.Log("Quitting game...");
        
        // In the Unity Editor, this will stop play mode
        // In a built game, this will close the application
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    /// <summary>
    /// Loads a scene by name (useful for multiple scenes or settings menu)
    /// </summary>
    /// <param name="sceneName">Name of the scene to load</param>
    public void LoadScene(string sceneName)
    {
        PlayButtonSound();
        
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("MainMenu: Scene name is empty!");
        }
    }

    /// <summary>
    /// Plays button click sound effect if available
    /// </summary>
    private void PlayButtonSound()
    {
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }
}

