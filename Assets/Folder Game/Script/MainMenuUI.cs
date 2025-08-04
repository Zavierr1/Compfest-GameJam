using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Menu Configuration")]
    public string gameTitle = "Memory Fragments";
    public Color titleColor = Color.white;
    public Color buttonColor = new Color(0.2f, 0.4f, 0.8f, 0.9f); // Match pause menu
    public Color buttonHoverColor = new Color(0.3f, 0.5f, 0.9f, 1f); // Match pause menu
    
    [Header("Layout Settings")]
    public float titleFontSize = 60f;
    public float buttonFontSize = 24f;
    public Vector2 buttonSize = new Vector2(200f, 50f);
    public float buttonSpacing = 20f;
    
    private Canvas canvas;
    private MainMenuManager menuManager;
    private bool uiCreated = false;
    
    void Start()
    {
        // Ensure we don't create UI multiple times
        if (!uiCreated)
        {
            CreateMainMenuUI();
            uiCreated = true;
        }
    }
    
    void CreateMainMenuUI()
    {
        // Get or create canvas - prioritize existing one
        canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("MainMenuCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // Higher than game UI
            
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            
            canvasGO.AddComponent<GraphicRaycaster>();
        }
        
        // Clear existing menu content to avoid duplicates
        Transform existingMenu = canvas.transform.Find("MenuContainer");
        if (existingMenu != null)
        {
            DestroyImmediate(existingMenu.gameObject);
        }
        
        // Create background overlay like pause menu
        CreateBackground();
        
        // Create main container
        GameObject menuContainer = new GameObject("MenuContainer");
        menuContainer.transform.SetParent(canvas.transform, false);
        
        RectTransform containerRect = menuContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.sizeDelta = new Vector2(400f, 500f);
        
        // Create panel background like pause menu
        CreateMenuPanel(menuContainer);
        
        // Create title
        CreateTitle(menuContainer);
        
        // Create buttons
        CreatePlayButton(menuContainer);
        CreateExitButton(menuContainer);
        
        // Get menu manager reference and auto-assign buttons
        menuManager = FindFirstObjectByType<MainMenuManager>();
        if (menuManager != null)
        {
            // Auto-assign buttons if manager doesn't have them
            Button[] buttons = menuContainer.GetComponentsInChildren<Button>();
            foreach (Button btn in buttons)
            {
                if (btn.name == "PlayButton" && menuManager.playButton == null)
                    menuManager.playButton = btn;
                else if (btn.name == "ExitButton" && menuManager.exitButton == null)
                    menuManager.exitButton = btn;
            }
        }
    }
    
    void CreateBackground()
    {
        // Create background overlay like pause menu
        GameObject backgroundGO = new GameObject("Background");
        backgroundGO.transform.SetParent(canvas.transform, false);
        
        RectTransform backgroundRect = backgroundGO.AddComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.sizeDelta = Vector2.zero;
        backgroundRect.anchoredPosition = Vector2.zero;
        
        Image backgroundImage = backgroundGO.AddComponent<Image>();
        backgroundImage.color = new Color(0, 0, 0, 0.7f); // Semi-transparent black like pause menu
    }
    
    void CreateMenuPanel(GameObject parent)
    {
        // Create panel background like pause menu
        GameObject panelGO = new GameObject("MenuPanel");
        panelGO.transform.SetParent(parent.transform, false);
        
        RectTransform panelRect = panelGO.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;
        
        Image panelImage = panelGO.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f); // Dark panel like pause menu
    }
    
    void CreateTitle(GameObject parent)
    {
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(parent.transform, false);
        
        RectTransform titleRect = titleGO.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -80f);
        titleRect.sizeDelta = new Vector2(380f, 80f);
        
        Text titleText = titleGO.AddComponent<Text>();
        titleText.text = gameTitle;
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = (int)titleFontSize;
        titleText.color = titleColor;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.fontStyle = FontStyle.Bold;
    }
    
    void CreatePlayButton(GameObject parent)
    {
        GameObject playButtonGO = new GameObject("PlayButton");
        playButtonGO.transform.SetParent(parent.transform, false);
        
        RectTransform playRect = playButtonGO.AddComponent<RectTransform>();
        playRect.anchorMin = new Vector2(0.5f, 0.5f);
        playRect.anchorMax = new Vector2(0.5f, 0.5f);
        playRect.anchoredPosition = new Vector2(0f, 30f);
        playRect.sizeDelta = buttonSize;
        
        Image playImage = playButtonGO.AddComponent<Image>();
        playImage.color = buttonColor;
        
        Button playButton = playButtonGO.AddComponent<Button>();
        
        // Create button text
        GameObject playTextGO = new GameObject("Text");
        playTextGO.transform.SetParent(playButtonGO.transform, false);
        
        RectTransform playTextRect = playTextGO.AddComponent<RectTransform>();
        playTextRect.anchorMin = Vector2.zero;
        playTextRect.anchorMax = Vector2.one;
        playTextRect.offsetMin = Vector2.zero;
        playTextRect.offsetMax = Vector2.zero;
        
        Text playText = playTextGO.AddComponent<Text>();
        playText.text = "PLAY";
        playText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        playText.fontSize = (int)buttonFontSize;
        playText.color = Color.white;
        playText.alignment = TextAnchor.MiddleCenter;
        playText.fontStyle = FontStyle.Bold;
        
        // Setup button colors to match pause menu exactly
        ColorBlock colors = playButton.colors;
        colors.normalColor = new Color(0.2f, 0.4f, 0.8f, 0.9f);
        colors.highlightedColor = new Color(0.3f, 0.5f, 0.9f, 1f);
        colors.pressedColor = new Color(0.15f, 0.3f, 0.6f, 1f);
        playButton.colors = colors;
    }
    
    void CreateExitButton(GameObject parent)
    {
        GameObject exitButtonGO = new GameObject("ExitButton");
        exitButtonGO.transform.SetParent(parent.transform, false);
        
        RectTransform exitRect = exitButtonGO.AddComponent<RectTransform>();
        exitRect.anchorMin = new Vector2(0.5f, 0.5f);
        exitRect.anchorMax = new Vector2(0.5f, 0.5f);
        exitRect.anchoredPosition = new Vector2(0f, -30f);
        exitRect.sizeDelta = buttonSize;
        
        Image exitImage = exitButtonGO.AddComponent<Image>();
        exitImage.color = buttonColor;
        
        Button exitButton = exitButtonGO.AddComponent<Button>();
        
        // Create button text
        GameObject exitTextGO = new GameObject("Text");
        exitTextGO.transform.SetParent(exitButtonGO.transform, false);
        
        RectTransform exitTextRect = exitTextGO.AddComponent<RectTransform>();
        exitTextRect.anchorMin = Vector2.zero;
        exitTextRect.anchorMax = Vector2.one;
        exitTextRect.offsetMin = Vector2.zero;
        exitTextRect.offsetMax = Vector2.zero;
        
        Text exitText = exitTextGO.AddComponent<Text>();
        exitText.text = "EXIT";
        exitText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        exitText.fontSize = (int)buttonFontSize;
        exitText.color = Color.white;
        exitText.alignment = TextAnchor.MiddleCenter;
        exitText.fontStyle = FontStyle.Bold;
        
        // Setup button colors to match pause menu exactly
        ColorBlock colors = exitButton.colors;
        colors.normalColor = new Color(0.2f, 0.4f, 0.8f, 0.9f);
        colors.highlightedColor = new Color(0.3f, 0.5f, 0.9f, 1f);
        colors.pressedColor = new Color(0.15f, 0.3f, 0.6f, 1f);
        exitButton.colors = colors;
    }
}
