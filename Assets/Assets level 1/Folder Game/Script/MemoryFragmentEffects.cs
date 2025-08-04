using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MemoryFragmentEffects : MonoBehaviour
{
    private static MemoryFragmentEffects instance;
    public static MemoryFragmentEffects Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<MemoryFragmentEffects>();
                if (instance == null)
                {
                    GameObject go = new GameObject("MemoryFragmentEffects");
                    instance = go.AddComponent<MemoryFragmentEffects>();
                }
            }
            return instance;
        }
    }

    [Header("Vision Effect Settings")]
    public float visionEffectDuration = 10f;
    public float visionRadius = 15f;
    public GameObject visionHighlightPrefab;
    public Material visionMaterial;
    public Color visionColor = new Color(0.5f, 0.8f, 1f, 0.5f);

    [Header("Sound Effect Settings")]
    public AudioClip[] memoryWhispers;
    public float soundEffectDuration = 5f;
    public GameObject soundWavePrefab;
    private AudioSource audioSource;

    [Header("Time Effect Settings")]
    public float timeSlowFactor = 0.3f;
    public float timeEffectDuration = 8f;
    public AnimationCurve timeTransitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Visual Feedback")]
    public GameObject fragmentCollectParticles;
    public Light fragmentLight;

    private Coroutine activeTimeEffect;
    private Coroutine activeVisionEffect;
    private GameObject[] hiddenObjects;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D sound
        }
    }

    public void ApplyVisionEffect()
    {
        if (activeVisionEffect != null)
            StopCoroutine(activeVisionEffect);
        
        activeVisionEffect = StartCoroutine(VisionEffectCoroutine());
    }

    public void ApplySoundEffect()
    {
        StartCoroutine(SoundEffectCoroutine());
    }

    public void ApplyTimeEffect()
    {
        if (activeTimeEffect != null)
            StopCoroutine(activeTimeEffect);
        
        activeTimeEffect = StartCoroutine(TimeEffectCoroutine());
    }

    IEnumerator VisionEffectCoroutine()
    {
        // Find all hidden objects in range and make them visible
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        hiddenObjects = new GameObject[0];
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.CompareTag("HiddenPath") || obj.CompareTag("HiddenClue"))
            {
                float distance = Vector3.Distance(obj.transform.position, Camera.main.transform.position);
                if (distance <= visionRadius)
                {
                    // Make object visible with glow effect
                    Renderer renderer = obj.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        Material originalMat = renderer.material;
                        renderer.material = visionMaterial != null ? visionMaterial : originalMat;
                        renderer.material.SetColor("_EmissionColor", visionColor * 2f);
                        renderer.material.EnableKeyword("_EMISSION");
                        
                        // Fade in effect
                        StartCoroutine(FadeInObject(renderer, 1f));
                    }
                    
                    // Add highlight particle effect
                    if (visionHighlightPrefab != null)
                    {
                        GameObject highlight = Instantiate(visionHighlightPrefab, obj.transform.position, Quaternion.identity);
                        highlight.transform.SetParent(obj.transform);
                        Destroy(highlight, visionEffectDuration);
                    }
                }
            }
        }

        // Create vision field indicator
        GameObject visionField = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visionField.transform.position = Camera.main.transform.position;
        visionField.transform.localScale = Vector3.one * visionRadius * 2f;
        
        // Configure vision field material
        Renderer visionRenderer = visionField.GetComponent<Renderer>();
        visionRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        visionRenderer.material.SetFloat("_Surface", 1); // Transparent
        visionRenderer.material.SetColor("_BaseColor", visionColor);
        visionRenderer.material.SetFloat("_Blend", 0);
        visionRenderer.material.renderQueue = 3000;
        
        // Remove collider
        Destroy(visionField.GetComponent<Collider>());

        // Pulse effect
        float elapsed = 0f;
        while (elapsed < visionEffectDuration)
        {
            float scale = visionRadius * 2f * (1f + Mathf.Sin(elapsed * 2f) * 0.1f);
            visionField.transform.localScale = Vector3.one * scale;
            visionRenderer.material.SetColor("_BaseColor", visionColor * (1f - elapsed / visionEffectDuration));
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(visionField);
        activeVisionEffect = null;
    }

    IEnumerator SoundEffectCoroutine()
    {
        // Play memory whisper
        if (memoryWhispers != null && memoryWhispers.Length > 0 && audioSource != null)
        {
            AudioClip whisper = memoryWhispers[Random.Range(0, memoryWhispers.Length)];
            audioSource.clip = whisper;
            audioSource.volume = 0f;
            audioSource.Play();
            
            // Fade in
            float fadeTime = 1f;
            float elapsed = 0f;
            while (elapsed < fadeTime)
            {
                audioSource.volume = Mathf.Lerp(0f, 0.7f, elapsed / fadeTime);
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        // Create sound wave visual effects
        if (soundWavePrefab != null)
        {
            for (int i = 0; i < 3; i++)
            {
                GameObject wave = Instantiate(soundWavePrefab, Camera.main.transform.position, Quaternion.identity);
                StartCoroutine(ExpandSoundWave(wave, 2f + i * 0.5f));
                yield return new WaitForSeconds(0.5f);
            }
        }
        else
        {
            // Create default sound wave effect
            for (int i = 0; i < 3; i++)
            {
                GameObject wave = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                wave.transform.position = Camera.main.transform.position;
                Renderer waveRenderer = wave.GetComponent<Renderer>();
                waveRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                waveRenderer.material.SetFloat("_Surface", 1);
                waveRenderer.material.SetColor("_BaseColor", new Color(1f, 1f, 1f, 0.3f));
                Destroy(wave.GetComponent<Collider>());
                StartCoroutine(ExpandSoundWave(wave, 2f + i * 0.5f));
                yield return new WaitForSeconds(0.5f);
            }
        }

        yield return new WaitForSeconds(soundEffectDuration - 1.5f);

        // Fade out audio
        if (audioSource != null && audioSource.isPlaying)
        {
            float fadeTime = 1f;
            float elapsed = 0f;
            float startVolume = audioSource.volume;
            while (elapsed < fadeTime)
            {
                audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeTime);
                elapsed += Time.deltaTime;
                yield return null;
            }
            audioSource.Stop();
        }
    }

    IEnumerator ExpandSoundWave(GameObject wave, float duration)
    {
        float elapsed = 0f;
        Vector3 initialScale = Vector3.one * 0.1f;
        Vector3 targetScale = Vector3.one * 20f;
        Renderer renderer = wave.GetComponent<Renderer>();
        
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            wave.transform.localScale = Vector3.Lerp(initialScale, targetScale, t);
            
            if (renderer != null)
            {
                Color color = renderer.material.GetColor("_BaseColor");
                color.a = Mathf.Lerp(0.3f, 0f, t);
                renderer.material.SetColor("_BaseColor", color);
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        Destroy(wave);
    }

    IEnumerator TimeEffectCoroutine()
    {
        // Slow down time
        float originalTimeScale = Time.timeScale;
        float originalFixedDeltaTime = Time.fixedDeltaTime;
        
        // Create time distortion visual effect
        GameObject timeDistortion = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        timeDistortion.transform.position = Camera.main.transform.position;
        timeDistortion.transform.localScale = Vector3.one * 30f;
        
        Renderer distortionRenderer = timeDistortion.GetComponent<Renderer>();
        distortionRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        distortionRenderer.material.SetFloat("_Surface", 1);
        distortionRenderer.material.SetColor("_BaseColor", new Color(0.5f, 0f, 1f, 0.2f));
        Destroy(timeDistortion.GetComponent<Collider>());

        float elapsed = 0f;
        
        // Transition in
        float transitionDuration = 0.5f;
        while (elapsed < transitionDuration)
        {
            float t = timeTransitionCurve.Evaluate(elapsed / transitionDuration);
            Time.timeScale = Mathf.Lerp(originalTimeScale, timeSlowFactor, t);
            Time.fixedDeltaTime = originalFixedDeltaTime * Time.timeScale;
            
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        
        Time.timeScale = timeSlowFactor;
        Time.fixedDeltaTime = originalFixedDeltaTime * Time.timeScale;
        
        // Maintain effect
        elapsed = 0f;
        while (elapsed < timeEffectDuration)
        {
            // Pulse effect on distortion sphere
            float scale = 30f * (1f + Mathf.Sin(elapsed * 3f) * 0.1f);
            timeDistortion.transform.localScale = Vector3.one * scale;
            
            // Rotate distortion sphere
            timeDistortion.transform.Rotate(Vector3.up, 30f * Time.unscaledDeltaTime);
            
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        
        // Transition out
        elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            float t = timeTransitionCurve.Evaluate(1f - (elapsed / transitionDuration));
            Time.timeScale = Mathf.Lerp(originalTimeScale, timeSlowFactor, t);
            Time.fixedDeltaTime = originalFixedDeltaTime * Time.timeScale;
            
            Color color = distortionRenderer.material.GetColor("_BaseColor");
            color.a = Mathf.Lerp(0f, 0.2f, 1f - (elapsed / transitionDuration));
            distortionRenderer.material.SetColor("_BaseColor", color);
            
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        
        // Reset time
        Time.timeScale = originalTimeScale;
        Time.fixedDeltaTime = originalFixedDeltaTime;
        
        Destroy(timeDistortion);
        activeTimeEffect = null;
    }

    IEnumerator FadeInObject(Renderer renderer, float duration)
    {
        float elapsed = 0f;
        Color originalColor = renderer.material.color;
        Color targetColor = originalColor;
        targetColor.a = 1f;
        
        while (elapsed < duration)
        {
            Color currentColor = Color.Lerp(new Color(originalColor.r, originalColor.g, originalColor.b, 0f), targetColor, elapsed / duration);
            renderer.material.color = currentColor;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        renderer.material.color = targetColor;
    }

    public void PlayCollectionParticles(Vector3 position)
    {
        if (fragmentCollectParticles != null)
        {
            Instantiate(fragmentCollectParticles, position, Quaternion.identity);
        }
    }
}
