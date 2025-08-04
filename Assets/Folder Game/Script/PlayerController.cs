using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float kecepatanGerak = 5f;
    
    [Header("Gravity Change Effects")]
    [SerializeField] private ParticleSystem gravityChangeParticles;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip gravityChangeSound;
    [SerializeField] private float gravityChangeSoundVolume = 0.7f;
    
    [Header("Visual Feedback Settings")]
    [SerializeField] private Color gravityChangeColor = Color.cyan;
    [SerializeField] private float colorChangeDuration = 0.3f;
    [SerializeField] private AnimationCurve colorFadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    
    [Header("Camera Shake (Optional)")]
    [SerializeField] private bool enableCameraShake = true;
    [SerializeField] private float shakeIntensity = 0.3f;
    [SerializeField] private float shakeDuration = 0.2f;
    
    // Private fields
    private Rigidbody rb;
    private Vector3 arahGerak;
    private Renderer playerRenderer;
    private Color originalColor;
    private Camera playerCamera;
    private Coroutine colorChangeCoroutine;

    void Start()
    {
        // Get required components
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("PlayerController: Rigidbody component not found!");
        }
        
        playerRenderer = GetComponent<Renderer>();
        if (playerRenderer != null)
        {
            originalColor = playerRenderer.material.color;
        }
        
        // Setup audio source if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }
        
        // Setup particle system if not assigned
        if (gravityChangeParticles == null)
        {
            // Try to find particle system in children
            gravityChangeParticles = GetComponentInChildren<ParticleSystem>();
            if (gravityChangeParticles == null)
            {
                CreateDefaultParticleSystem();
            }
        }
        
        // Get camera reference for shake effect
        playerCamera = Camera.main;
    }

    void Update()
    {
        // Input gerakan
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        arahGerak = new Vector3(horizontal, 0, vertical).normalized;

        // Input perubahan gravitasi
        if (Input.GetKeyDown(KeyCode.Q)) UbahGravitasi(-90);
        if (Input.GetKeyDown(KeyCode.E)) UbahGravitasi(90);
    }

    void FixedUpdate()
    {
        // Terapkan gerakan
        Vector3 gerakan = transform.TransformDirection(arahGerak) * kecepatanGerak;
        rb.linearVelocity = new Vector3(gerakan.x, rb.linearVelocity.y, gerakan.z);
    }

    void UbahGravitasi(float sudut)
    {
        // Rotate player and change gravity
        transform.Rotate(Vector3.forward, sudut);
        Physics.gravity = -transform.up * 9.81f;
        
        // Trigger all feedback effects
        PlayGravityChangeEffects();
    }
    
    void PlayGravityChangeEffects()
    {
        // Play particle effect
        if (gravityChangeParticles != null)
        {
            gravityChangeParticles.transform.rotation = Quaternion.identity; // Keep particles world-aligned
            gravityChangeParticles.Play();
        }
        
        // Play sound effect
        if (audioSource != null && gravityChangeSound != null)
        {
            audioSource.PlayOneShot(gravityChangeSound, gravityChangeSoundVolume);
        }
        
        // Visual color change effect
        if (playerRenderer != null)
        {
            if (colorChangeCoroutine != null)
            {
                StopCoroutine(colorChangeCoroutine);
            }
            colorChangeCoroutine = StartCoroutine(ColorChangeEffect());
        }
        
        // Camera shake effect
        if (enableCameraShake && playerCamera != null)
        {
            StartCoroutine(CameraShake());
        }
    }
    
    IEnumerator ColorChangeEffect()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < colorChangeDuration)
        {
            float t = elapsedTime / colorChangeDuration;
            float curveValue = colorFadeCurve.Evaluate(t);
            
            playerRenderer.material.color = Color.Lerp(originalColor, gravityChangeColor, curveValue);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        playerRenderer.material.color = originalColor;
    }
    
    IEnumerator CameraShake()
    {
        Vector3 originalPosition = playerCamera.transform.localPosition;
        float elapsedTime = 0f;
        
        while (elapsedTime < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeIntensity;
            float y = Random.Range(-1f, 1f) * shakeIntensity;
            
            playerCamera.transform.localPosition = originalPosition + new Vector3(x, y, 0);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        playerCamera.transform.localPosition = originalPosition;
    }
    
    void CreateDefaultParticleSystem()
    {
        // Create a child GameObject for the particle system
        GameObject particleGO = new GameObject("GravityChangeParticles");
        particleGO.transform.SetParent(transform);
        particleGO.transform.localPosition = Vector3.zero;
        
        // Add and configure particle system
        gravityChangeParticles = particleGO.AddComponent<ParticleSystem>();
        var main = gravityChangeParticles.main;
        main.duration = 0.5f;
        main.startLifetime = 0.8f;
        main.startSpeed = 5f;
        main.startSize = 0.3f;
        main.startColor = new Color(0.5f, 0.8f, 1f, 0.8f); // Light blue
        main.maxParticles = 50;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        // Configure emission
        var emission = gravityChangeParticles.emission;
        emission.enabled = true;
        emission.SetBursts(new ParticleSystem.Burst[] 
        {
            new ParticleSystem.Burst(0.0f, 30)
        });
        
        // Configure shape
        var shape = gravityChangeParticles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;
        
        // Configure velocity over lifetime for outward burst
        var velocityOverLifetime = gravityChangeParticles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(3f);
        
        // Configure size over lifetime
        var sizeOverLifetime = gravityChangeParticles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0f);
        sizeCurve.AddKey(0.1f, 1f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        // Configure color over lifetime
        var colorOverLifetime = gravityChangeParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.cyan, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = gradient;
        
        // Disable looping
        main.loop = false;
        main.playOnAwake = false;
        
        Debug.Log("Created default particle system for gravity change effects");
    }
}