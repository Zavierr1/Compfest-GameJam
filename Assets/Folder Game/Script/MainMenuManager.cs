using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Button playButton;
    public Button exitButton;
    
    [Header("Game Settings")]
    public string gameSceneName = "InGameScene"; // Name of your main game scene
    
    [Header("Audio (Optional)")]
    public AudioSource audioSource;
    public AudioClip buttonClickSound;
    public AudioClip backgroundMusic;
    
    void Start()
    {
        // Setup button listeners
        if (playButton != null)
        {
            playButton.onClick.AddListener(PlayGame);
        }
        else
        {
            Debug.LogError("MainMenuManager: Play button not assigned!");
        }
        
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ExitGame);
        }
        else
        {
            Debug.LogError("MainMenuManager: Exit button not assigned!");
        }
        
        // Setup audio
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        
        // Play background music if available
        if (audioSource != null && backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
        
        // Set cursor to be visible and unlocked
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void PlayGame()
    {
        PlayButtonSound();
        
        // Load the game scene with transition
        if (!string.IsNullOrEmpty(gameSceneName))
        {
            // Use scene transition manager if available, otherwise load directly
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadScene(gameSceneName);
            }
            else
            {
                SceneManager.LoadScene(gameSceneName);
            }
        }
        else
        {
            Debug.LogError("MainMenuManager: Game scene name not set! Please assign the scene name in the inspector.");
        }
    }
    
    public void ExitGame()
    {
        PlayButtonSound();
        
        // Quit the application
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
        
        Debug.Log("Game Exit Requested");
    }
    
    void PlayButtonSound()
    {
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }
    
    // Optional: Method to be called from buttons directly (alternative to listener setup)
    public void OnPlayButtonClick()
    {
        PlayGame();
    }
    
    public void OnExitButtonClick()
    {
        ExitGame();
    }
}
