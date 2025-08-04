using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    [Header("Transition Settings")]
    public float fadeTime = 1f;
    public Color fadeColor = Color.black;
    
    [Header("Loading Screen")]
    public bool showLoadingScreen = true;
    public string loadingText = "Loading...";
    
    private static SceneTransitionManager instance;
    private Canvas transitionCanvas;
    private Image fadeImage;
    private Text loadingTextComponent;
    private bool isTransitioning = false;
    
    public static SceneTransitionManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("SceneTransitionManager");
                instance = go.AddComponent<SceneTransitionManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            CreateTransitionUI();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    void CreateTransitionUI()
    {
        // Create canvas for transition
        GameObject canvasGO = new GameObject("TransitionCanvas");
        canvasGO.transform.SetParent(transform);
        
        transitionCanvas = canvasGO.AddComponent<Canvas>();
        transitionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        transitionCanvas.sortingOrder = 999; // Make sure it's on top
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // Create fade image
        GameObject fadeGO = new GameObject("FadeImage");
        fadeGO.transform.SetParent(canvasGO.transform, false);
        
        RectTransform fadeRect = fadeGO.AddComponent<RectTransform>();
        fadeRect.anchorMin = Vector2.zero;
        fadeRect.anchorMax = Vector2.one;
        fadeRect.offsetMin = Vector2.zero;
        fadeRect.offsetMax = Vector2.zero;
        
        fadeImage = fadeGO.AddComponent<Image>();
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        
        // Create loading text
        if (showLoadingScreen)
        {
            GameObject loadingGO = new GameObject("LoadingText");
            loadingGO.transform.SetParent(canvasGO.transform, false);
            
            RectTransform loadingRect = loadingGO.AddComponent<RectTransform>();
            loadingRect.anchorMin = new Vector2(0.5f, 0.3f);
            loadingRect.anchorMax = new Vector2(0.5f, 0.3f);
            loadingRect.anchoredPosition = Vector2.zero;
            loadingRect.sizeDelta = new Vector2(400f, 50f);
            
            loadingTextComponent = loadingGO.AddComponent<Text>();
            loadingTextComponent.text = loadingText;
            loadingTextComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            loadingTextComponent.fontSize = 24;
            loadingTextComponent.color = Color.white;
            loadingTextComponent.alignment = TextAnchor.MiddleCenter;
            loadingTextComponent.color = new Color(1f, 1f, 1f, 0f); // Start transparent
        }
        
        // Start with canvas disabled
        transitionCanvas.gameObject.SetActive(false);
    }
    
    public void LoadScene(string sceneName)
    {
        if (!isTransitioning)
        {
            StartCoroutine(TransitionToScene(sceneName));
        }
    }
    
    public void LoadScene(int sceneIndex)
    {
        if (!isTransitioning)
        {
            StartCoroutine(TransitionToScene(sceneIndex));
        }
    }
    
    IEnumerator TransitionToScene(string sceneName)
    {
        isTransitioning = true;
        transitionCanvas.gameObject.SetActive(true);
        
        // Fade out
        yield return StartCoroutine(FadeOut());
        
        // Show loading text
        if (showLoadingScreen && loadingTextComponent != null)
        {
            yield return StartCoroutine(FadeInText());
        }
        
        // Load scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        
        // Wait for scene to load
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        // Small delay to ensure everything is ready
        yield return new WaitForSeconds(0.5f);
        
        // Hide loading text
        if (showLoadingScreen && loadingTextComponent != null)
        {
            yield return StartCoroutine(FadeOutText());
        }
        
        // Fade in
        yield return StartCoroutine(FadeIn());
        
        transitionCanvas.gameObject.SetActive(false);
        isTransitioning = false;
    }
    
    IEnumerator TransitionToScene(int sceneIndex)
    {
        isTransitioning = true;
        transitionCanvas.gameObject.SetActive(true);
        
        // Fade out
        yield return StartCoroutine(FadeOut());
        
        // Show loading text
        if (showLoadingScreen && loadingTextComponent != null)
        {
            yield return StartCoroutine(FadeInText());
        }
        
        // Load scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        
        // Wait for scene to load
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        // Small delay to ensure everything is ready
        yield return new WaitForSeconds(0.5f);
        
        // Hide loading text
        if (showLoadingScreen && loadingTextComponent != null)
        {
            yield return StartCoroutine(FadeOutText());
        }
        
        // Fade in
        yield return StartCoroutine(FadeIn());
        
        transitionCanvas.gameObject.SetActive(false);
        isTransitioning = false;
    }
    
    IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        Color startColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        Color endColor = fadeColor;
        
        while (elapsedTime < fadeTime)
        {
            float t = elapsedTime / fadeTime;
            fadeImage.color = Color.Lerp(startColor, endColor, t);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        
        fadeImage.color = endColor;
    }
    
    IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        Color startColor = fadeColor;
        Color endColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        
        while (elapsedTime < fadeTime)
        {
            float t = elapsedTime / fadeTime;
            fadeImage.color = Color.Lerp(startColor, endColor, t);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        
        fadeImage.color = endColor;
    }
    
    IEnumerator FadeInText()
    {
        float elapsedTime = 0f;
        Color startColor = new Color(1f, 1f, 1f, 0f);
        Color endColor = Color.white;
        
        while (elapsedTime < fadeTime * 0.5f)
        {
            float t = elapsedTime / (fadeTime * 0.5f);
            loadingTextComponent.color = Color.Lerp(startColor, endColor, t);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        
        loadingTextComponent.color = endColor;
    }
    
    IEnumerator FadeOutText()
    {
        float elapsedTime = 0f;
        Color startColor = Color.white;
        Color endColor = new Color(1f, 1f, 1f, 0f);
        
        while (elapsedTime < fadeTime * 0.5f)
        {
            float t = elapsedTime / (fadeTime * 0.5f);
            loadingTextComponent.color = Color.Lerp(startColor, endColor, t);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        
        loadingTextComponent.color = endColor;
    }
}
