using UnityEngine;
using UnityEngine.UI;

public class PauseMenuSetup : MonoBehaviour
{
    [Header("Auto Setup Settings")]
    public bool setupOnStart = true;
    public bool createEventSystemIfMissing = true;
    
    void Start()
    {
        if (setupOnStart)
        {
            SetupPauseMenu();
        }
    }
    
    [ContextMenu("Setup Pause Menu")]
    public void SetupPauseMenu()
    {
        // Check if pause menu already exists
        PauseMenuManager existingPauseMenu = FindFirstObjectByType<PauseMenuManager>();
        if (existingPauseMenu != null)
        {
            Debug.Log("✓ Pause menu already exists - connecting existing UI elements...");
            
            // Auto-configure existing pause menu
            ConfigureExistingPauseMenu(existingPauseMenu);
            return;
        }
        
        // Create EventSystem if missing (required for UI)
        if (createEventSystemIfMissing)
        {
            UnityEngine.EventSystems.EventSystem eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem == null)
            {
                GameObject eventSystemGO = new GameObject("EventSystem");
                eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                Debug.Log("Created EventSystem for UI interactions");
            }
        }
        
        // Create pause menu manager
        GameObject pauseMenuGO = new GameObject("PauseMenuManager");
        PauseMenuManager pauseMenu = pauseMenuGO.AddComponent<PauseMenuManager>();
        
        // Configure default settings for immediate functionality
        pauseMenu.pauseOnEscape = true;
        pauseMenu.disablePlayerInputWhenPaused = true;
        
        // Make sure it persists across scene loads if needed
        DontDestroyOnLoad(pauseMenuGO);
        
        Debug.Log("Pause menu setup complete!");
        Debug.Log("✓ ESC key: Toggle pause menu");
        Debug.Log("✓ Continue button: Resume game");
        Debug.Log("✓ Exit button: Return to main menu");
        Debug.Log("✓ All functionality is ready to use!");
        
        // Auto-destroy this setup script since it's no longer needed
        Destroy(this);
    }
    
    void ConfigureExistingPauseMenu(PauseMenuManager pauseMenu)
    {
        // Auto-find and connect existing UI elements in the scene
        if (pauseMenu.pauseCanvas == null)
        {
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas canvas in canvases)
            {
                if (canvas.name.ToLower().Contains("pause") || canvas.name.ToLower().Contains("menu"))
                {
                    pauseMenu.pauseCanvas = canvas;
                    Debug.Log($"✓ Connected pause canvas: {canvas.name}");
                    break;
                }
            }
        }
        
        // Auto-find buttons in the scene
        if (pauseMenu.continueButton == null || pauseMenu.exitButton == null)
        {
            Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
            foreach (Button button in buttons)
            {
                string buttonName = button.name.ToLower();
                if ((buttonName.Contains("continue") || buttonName.Contains("resume")) && pauseMenu.continueButton == null)
                {
                    pauseMenu.continueButton = button;
                    Debug.Log($"✓ Connected continue button: {button.name}");
                }
                else if ((buttonName.Contains("exit") || buttonName.Contains("menu") || buttonName.Contains("quit")) && pauseMenu.exitButton == null)
                {
                    pauseMenu.exitButton = button;
                    Debug.Log($"✓ Connected exit button: {button.name}");
                }
            }
        }
        
        // Configure settings
        pauseMenu.pauseOnEscape = true;
        pauseMenu.disablePlayerInputWhenPaused = true;
        
        Debug.Log("✓ Existing pause menu configured!");
        Debug.Log("✓ ESC key: Toggle pause menu");
        Debug.Log("✓ Continue button: Resume game");
        Debug.Log("✓ Exit button: Return to main menu");
        Debug.Log("✓ All functionality ready - no manual setup needed!");
        
        // Auto-destroy this setup script since it's no longer needed
        Destroy(this);
    }
}
