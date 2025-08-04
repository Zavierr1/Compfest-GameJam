using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI References")]
    public Canvas pauseCanvas;
    public Button continueButton;
    public Button exitButton;
    public GameObject pausePanel;
    
    [Header("Pause Settings")]
    public bool pauseOnEscape = true;
    public bool disablePlayerInputWhenPaused = true;
    
    [Header("Audio (Optional)")]
    public AudioSource audioSource;
    public AudioClip buttonClickSound;
    public AudioClip pauseSound;
    public AudioClip unpauseSound;
    
    private bool isPaused = false;
    private PlayerController playerController;
    private FirstPersonCamera firstPersonCamera;
    private float originalTimeScale;
    
    // Store original cursor state for restoration
    private CursorLockMode originalCursorLockState;
    private bool originalCursorVisible;
    
    // Singleton pattern for easy access
    public static PauseMenuManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        originalTimeScale = Time.timeScale;
    }
    
    void OnEnable()
    {
        // Ensure pause menu is properly initialized when this object is enabled
        // This helps when returning from other scenes
        if (pauseCanvas == null)
        {
            Debug.Log("PauseMenuManager OnEnable: Pause canvas is null, will recreate in Start()");
        }
    }
    
    void Start()
    {
        // Find player components
        playerController = FindFirstObjectByType<PlayerController>();
        firstPersonCamera = FindFirstObjectByType<FirstPersonCamera>();
        
        // Setup audio
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f; // 2D sound
            }
        }
        
        // Create pause menu if not assigned or if it was destroyed
        if (pauseCanvas == null)
        {
            Debug.Log("Creating pause menu UI in Start()...");
            CreatePauseMenuUI();
        }
        
        // Setup button listeners
        SetupButtonListeners();
        
        // Ensure pause menu starts hidden and game is not paused
        isPaused = false;
        Time.timeScale = originalTimeScale;
        SetPauseMenuActive(false);
        
        // Initialize cursor state for game scene
        if (firstPersonCamera != null && firstPersonCamera.lockCursor)
        {
            originalCursorLockState = CursorLockMode.Locked;
            originalCursorVisible = false;
        }
        else
        {
            originalCursorLockState = Cursor.lockState;
            originalCursorVisible = Cursor.visible;
        }
    }
    
    void Update()
    {
        // Handle pause input
        if (pauseOnEscape && Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }
    
    void SetupButtonListeners()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(ResumeGame);
        }
        
        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(ExitToMainMenu);
        }
    }
    
    public void PauseGame()
    {
        if (isPaused) return;
        
        // Ensure pause menu UI exists before trying to show it
        if (pauseCanvas == null)
        {
            Debug.LogWarning("Pause canvas is null, recreating pause menu UI...");
            CreatePauseMenuUI();
            SetupButtonListeners();
        }
        
        isPaused = true;
        Time.timeScale = 0f;
        
        // Store original cursor state before changing it
        originalCursorLockState = Cursor.lockState;
        originalCursorVisible = Cursor.visible;
        
        // Show pause menu
        SetPauseMenuActive(true);
        
        // Disable player input
        if (disablePlayerInputWhenPaused)
        {
            DisablePlayerInput();
        }
        
        // Enable cursor for menu interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Play pause sound
        PlaySound(pauseSound);
        
        Debug.Log("Game Paused");
    }
    
    public void ResumeGame()
    {
        if (!isPaused) return;
        
        PlayButtonSound();
        
        isPaused = false;
        Time.timeScale = originalTimeScale;
        
        // Hide pause menu
        SetPauseMenuActive(false);
        
        // Re-enable player input
        if (disablePlayerInputWhenPaused)
        {
            EnablePlayerInput();
        }
        
        // Restore original cursor state
        Cursor.lockState = originalCursorLockState;
        Cursor.visible = originalCursorVisible;
        
        // Play unpause sound
        PlaySound(unpauseSound);
        
        Debug.Log("Game Resumed");
    }
    
    public void ExitToMainMenu()
    {
        PlayButtonSound();
        
        // Immediately hide pause menu
        if (pauseCanvas != null)
        {
            pauseCanvas.gameObject.SetActive(false);
        }
        
        // Reset time scale before changing scenes
        Time.timeScale = originalTimeScale;
        
        // Load main menu scene
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene("MainMenu");
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
        
        Debug.Log("Returning to Main Menu");
    }
    
    void SetPauseMenuActive(bool active)
    {
        if (pauseCanvas != null)
        {
            pauseCanvas.gameObject.SetActive(active);
            Debug.Log($"Pause canvas set to: {active}");
        }
        else
        {
            Debug.LogError("Pause canvas is null! Cannot show/hide pause menu.");
        }
        
        if (pausePanel != null)
        {
            pausePanel.SetActive(active);
        }
    }
    
    // Public method to force recreate pause menu UI - useful for debugging
    public void RecreatePauseMenuUI()
    {
        Debug.Log("Manually recreating pause menu UI...");
        
        // Destroy existing UI if any
        if (pauseCanvas != null && pauseCanvas.gameObject != null)
        {
            DestroyImmediate(pauseCanvas.gameObject);
        }
        
        // Reset references
        pauseCanvas = null;
        pausePanel = null;
        continueButton = null;
        exitButton = null;
        
        // Recreate UI
        CreatePauseMenuUI();
        SetupButtonListeners();
        SetPauseMenuActive(false);
        
        Debug.Log("Pause menu UI recreated successfully!");
    }
    
    void DisablePlayerInput()
    {
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        if (firstPersonCamera != null)
        {
            firstPersonCamera.SetMouseLookEnabled(false);
        }
    }
    
    void EnablePlayerInput()
    {
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        
        if (firstPersonCamera != null)
        {
            firstPersonCamera.SetMouseLookEnabled(true);
        }
    }
    
    void PlayButtonSound()
    {
        PlaySound(buttonClickSound);
    }
    
    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    void CreatePauseMenuUI()
    {
        // Create main canvas
        GameObject canvasGO = new GameObject("PauseMenuCanvas");
        pauseCanvas = canvasGO.AddComponent<Canvas>();
        pauseCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        pauseCanvas.sortingOrder = 1000; // High sorting order to appear above other UI
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // Create semi-transparent background
        GameObject backgroundGO = new GameObject("Background");
        backgroundGO.transform.SetParent(canvasGO.transform, false);
        
        RectTransform backgroundRect = backgroundGO.AddComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        
        Image backgroundImage = backgroundGO.AddComponent<Image>();
        backgroundImage.color = new Color(0, 0, 0, 0.7f); // Semi-transparent black
        
        // Create main pause panel
        GameObject panelGO = new GameObject("PausePanel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        pausePanel = panelGO;
        
        RectTransform panelRect = panelGO.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(600, 400); // Larger size for 1920x1080
        
        Image panelImage = panelGO.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f); // Dark panel
        
        // Create title
        CreateTitle(panelGO);
        
        // Create buttons
        CreateContinueButton(panelGO);
        CreateExitButton(panelGO);
        
        Debug.Log("Pause menu UI created successfully for 1920x1080 resolution");
    }
    
    void CreateTitle(GameObject parent)
    {
        GameObject titleGO = new GameObject("PauseTitle");
        titleGO.transform.SetParent(parent.transform, false);
        
        RectTransform titleRect = titleGO.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.8f);
        titleRect.anchorMax = new Vector2(0.5f, 0.9f);
        titleRect.anchoredPosition = Vector2.zero;
        titleRect.sizeDelta = new Vector2(500, 60);
        
        Text titleText = titleGO.AddComponent<Text>();
        titleText.text = "GAME PAUSED";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 48; // Larger font for 1920x1080
        titleText.color = Color.white;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.fontStyle = FontStyle.Bold;
    }
    
    void CreateContinueButton(GameObject parent)
    {
        GameObject buttonGO = new GameObject("ContinueButton");
        buttonGO.transform.SetParent(parent.transform, false);
        
        RectTransform buttonRect = buttonGO.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.6f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.6f);
        buttonRect.anchoredPosition = Vector2.zero;
        buttonRect.sizeDelta = new Vector2(300, 80); // Larger button size
        
        Image buttonImage = buttonGO.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.4f, 0.8f, 0.9f);
        
        continueButton = buttonGO.AddComponent<Button>();
        
        // Button text
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);
        
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        Text buttonText = textGO.AddComponent<Text>();
        buttonText.text = "CONTINUE";
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = 32; // Larger font
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.fontStyle = FontStyle.Bold;
        
        // Button colors
        ColorBlock colors = continueButton.colors;
        colors.normalColor = new Color(0.2f, 0.4f, 0.8f, 0.9f);
        colors.highlightedColor = new Color(0.3f, 0.5f, 0.9f, 1f);
        colors.pressedColor = new Color(0.15f, 0.3f, 0.6f, 1f);
        continueButton.colors = colors;
    }
    
    void CreateExitButton(GameObject parent)
    {
        GameObject buttonGO = new GameObject("ExitButton");
        buttonGO.transform.SetParent(parent.transform, false);
        
        RectTransform buttonRect = buttonGO.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.4f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.4f);
        buttonRect.anchoredPosition = Vector2.zero;
        buttonRect.sizeDelta = new Vector2(300, 80); // Larger button size
        
        Image buttonImage = buttonGO.AddComponent<Image>();
        buttonImage.color = new Color(0.8f, 0.2f, 0.2f, 0.9f);
        
        exitButton = buttonGO.AddComponent<Button>();
        
        // Button text
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);
        
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        Text buttonText = textGO.AddComponent<Text>();
        buttonText.text = "EXIT TO MENU";
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = 32; // Larger font
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.fontStyle = FontStyle.Bold;
        
        // Button colors
        ColorBlock colors = exitButton.colors;
        colors.normalColor = new Color(0.8f, 0.2f, 0.2f, 0.9f);
        colors.highlightedColor = new Color(0.9f, 0.3f, 0.3f, 1f);
        colors.pressedColor = new Color(0.6f, 0.15f, 0.15f, 1f);
        exitButton.colors = colors;
    }
    
    // Public methods for external access
    public bool IsPaused => isPaused;
    
    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }
}
