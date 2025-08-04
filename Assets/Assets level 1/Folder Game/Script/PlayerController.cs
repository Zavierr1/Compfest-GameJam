using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float kecepatanGerak = 5f;
    
    [Header("Gravity System")]
    public float gravityStrength = 9.81f;
    public float rotationSpeed = 2f; // Speed of player rotation during gravity change
    
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
    private Camera playerCamera; // Keep simple camera reference
    private Coroutine colorChangeCoroutine;
    
    // Gravity system variables
    [Header("Gravity Debug Info")]
    public int currentGravityDirection = 0; // Made public for UI access: 0=down, 1=up, 2=left, 3=right
    private bool isRotating = false;
    private Vector3[] gravityDirections = {
        Vector3.down,     // 0 - Normal gravity (down)
        Vector3.up,       // 1 - Upside down (up) 
        Vector3.left,     // 2 - Left wall
        Vector3.right     // 3 - Right wall
    };
    private Vector3[] playerRotations = {
        Vector3.zero,           // 0 - Normal (0, 0, 0) - standing on floor
        new Vector3(180, 0, 0), // 1 - Upside down - standing on ceiling  
        new Vector3(0, 0, 90),  // 2 - Left wall - rotated so left becomes down
        new Vector3(0, 0, -90)  // 3 - Right wall - rotated so right becomes down
    };

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
        
        // Initialize gravity system after start
        StartCoroutine(InitializeGravityAfterStart());
    }
    
    System.Collections.IEnumerator InitializeGravityAfterStart()
    {
        yield return null; // Wait one frame
        SetGravityDirection(currentGravityDirection, false); // Set initial gravity without effects
    }

    void Update()
    {
        // Input gerakan
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        arahGerak = new Vector3(horizontal, 0, vertical).normalized;

        // Input perubahan gravitasi - cycle through all 6 directions with E key
        if (Input.GetKeyDown(KeyCode.E) && !isRotating)
        {
            CycleGravityDirection();
        }
    }

    void FixedUpdate()
    {
        // Get movement input in world space based on current gravity orientation
        Vector3 worldMovement = CalculateWorldMovement(arahGerak);
        
        // Get current velocity and gravity
        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 gravityDirection = Physics.gravity.normalized;
        
        // Separate velocity into gravity component and movement component
        Vector3 gravityVelocity = Vector3.Project(currentVelocity, gravityDirection);
        
        // Apply new movement while preserving gravity velocity
        Vector3 newVelocity = worldMovement * kecepatanGerak + gravityVelocity;
        rb.linearVelocity = newVelocity;
    }
    
    Vector3 CalculateWorldMovement(Vector3 inputDirection)
    {
        if (inputDirection.magnitude < 0.01f) return Vector3.zero;
        
        // Get the player's current orientation
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        
        // Calculate movement in world space
        Vector3 moveDirection = (forward * inputDirection.z + right * inputDirection.x).normalized;
        
        // Ensure movement is perpendicular to gravity
        Vector3 gravityDirection = Physics.gravity.normalized;
        moveDirection = Vector3.ProjectOnPlane(moveDirection, gravityDirection);
        
        return moveDirection;
    }

    void CycleGravityDirection()
    {
        // Move to next gravity direction
        currentGravityDirection = (currentGravityDirection + 1) % gravityDirections.Length;
        SetGravityDirection(currentGravityDirection, true);
    }
    
    void SetGravityDirection(int directionIndex, bool playEffects)
    {
        if (directionIndex < 0 || directionIndex >= gravityDirections.Length)
            return;
            
        // Start rotation coroutine
        StartCoroutine(RotateToGravityDirection(directionIndex, playEffects));
    }
    
    System.Collections.IEnumerator RotateToGravityDirection(int directionIndex, bool playEffects)
    {
        isRotating = true;
        
        // Get target rotation
        Vector3 targetRotation = playerRotations[directionIndex];
        Vector3 targetGravity = gravityDirections[directionIndex] * gravityStrength;
        
        // Store initial rotation
        Vector3 initialRotation = transform.eulerAngles;
        
        // Normalize angles to handle 360-degree wrapping
        initialRotation = NormalizeAngles(initialRotation);
        targetRotation = NormalizeAngles(targetRotation);
        
        // Choose shortest rotation path
        Vector3 deltaRotation = targetRotation - initialRotation;
        deltaRotation = NormalizeAngles(deltaRotation);
        
        float rotationTime = 0f;
        float totalRotationTime = 1f / rotationSpeed;
        
        // Play effects if requested
        if (playEffects)
        {
            PlayGravityChangeEffects();
        }
        
        // Smoothly rotate player and change gravity
        while (rotationTime < totalRotationTime)
        {
            rotationTime += Time.deltaTime;
            float t = rotationTime / totalRotationTime;
            
            // Use smooth curve for rotation
            t = Mathf.SmoothStep(0f, 1f, t);
            
            // Interpolate rotation
            Vector3 currentRotation = initialRotation + deltaRotation * t;
            transform.eulerAngles = currentRotation;
            
            // Interpolate gravity
            Vector3 currentGravity = Vector3.Lerp(Physics.gravity, targetGravity, t);
            Physics.gravity = currentGravity;
            
            yield return null;
        }
        
        // Ensure final values are exact
        transform.eulerAngles = targetRotation;
        Physics.gravity = targetGravity;
        
        isRotating = false;
        
        // Debug log the current state
        string[] directionNames = {"Down", "Up", "Left", "Right"};
        Debug.Log($"Gravity changed to: {directionNames[directionIndex]} | Rotation: {targetRotation} | Gravity: {targetGravity}");
    }
    
    Vector3 NormalizeAngles(Vector3 angles)
    {
        angles.x = NormalizeAngle(angles.x);
        angles.y = NormalizeAngle(angles.y);
        angles.z = NormalizeAngle(angles.z);
        return angles;
    }
    
    float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }

    void UbahGravitasi(float sudut)
    {
        // Legacy method - keeping for compatibility but redirecting to new system
        if (!isRotating)
        {
            if (sudut > 0)
                CycleGravityDirection();
        }
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