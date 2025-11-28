using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Assign the Pause Menu Panel GameObject here")]
    public GameObject pauseMenuUI;

    private bool isPaused = false;

    private void Update()
    {
        // Check for Escape key using the New Input System
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    private void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    // Methods to be wired to buttons
    public void OnResumeButtonClicked()
    {
        Resume();
    }

    public void ReturnToMainMenu()
    {
        // Ensure time is running before loading the next scene
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}

