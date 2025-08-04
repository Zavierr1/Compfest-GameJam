using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    [Header("Mouse Look Settings")]
    public float mouseSensitivity = 2f;
    public float verticalLookLimit = 80f;
    
    [Header("Camera Setup")]
    public Transform playerBody;
    public bool lockCursor = true;
    
    [Header("Smooth Look (Optional)")]
    public bool smoothLook = false;
    public float smoothSpeed = 10f;
    
    private float mouseX;
    private float mouseY;
    private float currentXRotation = 0f;
    private Vector2 targetRotation;
    private Vector2 currentRotation;
    
    void Start()
    {
        // Auto-find player body if not assigned
        if (playerBody == null)
        {
            // Try to find parent with PlayerController
            Transform parent = transform.parent;
            while (parent != null)
            {
                if (parent.GetComponent<PlayerController>() != null)
                {
                    playerBody = parent;
                    break;
                }
                parent = parent.parent;
            }
            
            // If still not found, use direct parent
            if (playerBody == null && transform.parent != null)
            {
                playerBody = transform.parent;
            }
        }
        
        // Lock cursor to center of screen
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        // Initialize rotation values
        currentXRotation = transform.localEulerAngles.x;
        if (currentXRotation > 180f)
            currentXRotation -= 360f;
            
        currentRotation = new Vector2(currentXRotation, playerBody != null ? playerBody.eulerAngles.y : 0f);
        targetRotation = currentRotation;
    }
    
    void Update()
    {
        // Handle cursor lock toggle (ESC to unlock, click to lock again)
        HandleCursorLock();
        
        // Only process mouse look if cursor is locked
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            ProcessMouseLook();
        }
    }
    
    void ProcessMouseLook()
    {
        // Get mouse input
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        if (smoothLook)
        {
            // Smooth mouse look
            targetRotation.x -= mouseY;
            targetRotation.x = Mathf.Clamp(targetRotation.x, -verticalLookLimit, verticalLookLimit);
            targetRotation.y += mouseX;
            
            currentRotation = Vector2.Lerp(currentRotation, targetRotation, smoothSpeed * Time.deltaTime);
            
            // Apply rotations
            transform.localRotation = Quaternion.Euler(currentRotation.x, 0f, 0f);
            if (playerBody != null)
            {
                playerBody.rotation = Quaternion.Euler(0f, currentRotation.y, playerBody.eulerAngles.z);
            }
        }
        else
        {
            // Instant mouse look
            currentXRotation -= mouseY;
            currentXRotation = Mathf.Clamp(currentXRotation, -verticalLookLimit, verticalLookLimit);
            
            // Apply vertical rotation to camera
            transform.localRotation = Quaternion.Euler(currentXRotation, 0f, 0f);
            
            // Apply horizontal rotation to player body
            if (playerBody != null)
            {
                playerBody.Rotate(Vector3.up * mouseX);
            }
        }
    }
    
    void HandleCursorLock()
    {
        // ESC to unlock cursor
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        
        // Click to lock cursor again
        if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None)
        {
            if (lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
    
    // Public method to shake camera (for PlayerController effects)
    public void ShakeCamera(float intensity, float duration)
    {
        StartCoroutine(CameraShakeCoroutine(intensity, duration));
    }
    
    System.Collections.IEnumerator CameraShakeCoroutine(float intensity, float duration)
    {
        Vector3 originalPosition = transform.localPosition;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;
            
            transform.localPosition = originalPosition + new Vector3(x, y, 0);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        transform.localPosition = originalPosition;
    }
    
    // Public method to set sensitivity (useful for settings menu)
    public void SetSensitivity(float newSensitivity)
    {
        mouseSensitivity = newSensitivity;
    }
    
    // Method to temporarily disable mouse look (useful for UI interactions)
    public void SetMouseLookEnabled(bool enabled)
    {
        this.enabled = enabled;
        if (enabled && lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
