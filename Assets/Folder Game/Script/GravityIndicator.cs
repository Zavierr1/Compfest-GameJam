using UnityEngine;
using UnityEngine.UI;

public class GravityIndicator : MonoBehaviour
{
    [Header("UI References")]
    public Text gravityDirectionText;
    public Image gravityArrow;
    public Canvas indicatorCanvas;
    
    [Header("Visual Settings")]
    public Color[] directionColors = {
        Color.white,    // Down - Normal
        Color.red,      // Up - Upside down
        Color.green,    // Left - Left wall
        Color.blue      // Right - Right wall
    };
    
    private PlayerController playerController;
    private string[] directionNames = {"↓ DOWN", "↑ UP", "← LEFT", "→ RIGHT"};
    private Vector3[] arrowRotations = {
        new Vector3(0, 0, 0),     // Down
        new Vector3(0, 0, 180),   // Up  
        new Vector3(0, 0, 90),    // Left
        new Vector3(0, 0, -90)    // Right
    };
    
    void Start()
    {
        // Find player controller
        playerController = FindFirstObjectByType<PlayerController>();
        
        // Create UI if not assigned
        if (indicatorCanvas == null)
        {
            CreateGravityIndicatorUI();
        }
        
        // Initial update
        UpdateGravityIndicator();
    }
    
    void CreateGravityIndicatorUI()
    {
        // Create canvas
        GameObject canvasGO = new GameObject("GravityIndicatorCanvas");
        indicatorCanvas = canvasGO.AddComponent<Canvas>();
        indicatorCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        indicatorCanvas.sortingOrder = 100;
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // Create background panel
        GameObject panelGO = new GameObject("GravityPanel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        
        RectTransform panelRect = panelGO.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 1);
        panelRect.anchorMax = new Vector2(0, 1);
        panelRect.anchoredPosition = new Vector2(150, -80);
        panelRect.sizeDelta = new Vector2(250, 100); // Larger size for 1920x1080
        
        Image panelImage = panelGO.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.7f);
        
        // Create gravity text
        GameObject textGO = new GameObject("GravityText");
        textGO.transform.SetParent(panelGO.transform, false);
        
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0.5f);
        textRect.anchorMax = new Vector2(0.7f, 1);
        textRect.offsetMin = new Vector2(10, 0);
        textRect.offsetMax = new Vector2(-10, -5);
        
        gravityDirectionText = textGO.AddComponent<Text>();
        gravityDirectionText.text = "↓ DOWN";
        gravityDirectionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        gravityDirectionText.fontSize = 20; // Larger font for 1920x1080
        gravityDirectionText.color = Color.white;
        gravityDirectionText.alignment = TextAnchor.MiddleLeft;
        gravityDirectionText.fontStyle = FontStyle.Bold;
        
        // Create arrow indicator
        GameObject arrowGO = new GameObject("GravityArrow");
        arrowGO.transform.SetParent(panelGO.transform, false);
        
        RectTransform arrowRect = arrowGO.AddComponent<RectTransform>();
        arrowRect.anchorMin = new Vector2(0.7f, 0.1f);
        arrowRect.anchorMax = new Vector2(0.95f, 0.9f);
        arrowRect.offsetMin = Vector2.zero;
        arrowRect.offsetMax = Vector2.zero;
        
        gravityArrow = arrowGO.AddComponent<Image>();
        gravityArrow.color = Color.white;
        
        // Create a simple arrow sprite (triangle)
        CreateArrowSprite();
        
        // Create instruction text
        GameObject instructionGO = new GameObject("InstructionText");
        instructionGO.transform.SetParent(panelGO.transform, false);
        
        RectTransform instructionRect = instructionGO.AddComponent<RectTransform>();
        instructionRect.anchorMin = new Vector2(0, 0);
        instructionRect.anchorMax = new Vector2(1, 0.5f);
        instructionRect.offsetMin = new Vector2(10, 5);
        instructionRect.offsetMax = new Vector2(-10, 0);
        
        Text instructionText = instructionGO.AddComponent<Text>();
        instructionText.text = "Press E to rotate gravity";
        instructionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        instructionText.fontSize = 14; // Slightly larger for 1920x1080
        instructionText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        instructionText.alignment = TextAnchor.MiddleCenter;
    }
    
    void CreateArrowSprite()
    {
        // Create a simple triangle texture for the arrow
        Texture2D arrowTexture = new Texture2D(32, 32);
        Color[] pixels = new Color[32 * 32];
        
        // Fill with transparent
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }
        
        // Draw triangle pointing up
        for (int y = 8; y < 24; y++)
        {
            for (int x = 16 - (y - 8); x <= 16 + (y - 8) && x < 32; x++)
            {
                if (x >= 0 && x < 32)
                {
                    pixels[y * 32 + x] = Color.white;
                }
            }
        }
        
        arrowTexture.SetPixels(pixels);
        arrowTexture.Apply();
        
        Sprite arrowSprite = Sprite.Create(arrowTexture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        gravityArrow.sprite = arrowSprite;
    }
    
    void Update()
    {
        UpdateGravityIndicator();
    }
    
    void UpdateGravityIndicator()
    {
        if (playerController == null) return;
        
        // Get current gravity direction through reflection or public access
        int currentDirection = GetCurrentGravityDirection();
        
        if (currentDirection >= 0 && currentDirection < directionNames.Length)
        {
            // Update text
            gravityDirectionText.text = directionNames[currentDirection];
            gravityDirectionText.color = directionColors[currentDirection];
            
            // Update arrow rotation and color
            gravityArrow.transform.rotation = Quaternion.Euler(arrowRotations[currentDirection]);
            gravityArrow.color = directionColors[currentDirection];
        }
    }
    
    int GetCurrentGravityDirection()
    {
        if (playerController == null) return 0;
        
        // Try to get the current gravity direction from the player controller
        // This uses reflection to access the private field
        System.Reflection.FieldInfo field = typeof(PlayerController).GetField("currentGravityDirection", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            return (int)field.GetValue(playerController);
        }
        
        // Fallback: try to determine from current gravity vector
        Vector3 gravity = Physics.gravity.normalized;
        
        if (Vector3.Dot(gravity, Vector3.down) > 0.9f) return 0;      // Down
        if (Vector3.Dot(gravity, Vector3.up) > 0.9f) return 1;        // Up
        if (Vector3.Dot(gravity, Vector3.left) > 0.9f) return 2;      // Left
        if (Vector3.Dot(gravity, Vector3.right) > 0.9f) return 3;     // Right
        
        return 0; // Default to down
    }
}
