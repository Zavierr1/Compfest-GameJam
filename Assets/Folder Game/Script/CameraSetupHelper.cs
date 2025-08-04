using UnityEngine;

[System.Serializable]
public class CameraSetupHelper : MonoBehaviour
{
    [Header("Auto Setup Settings")]
    public bool autoSetupOnStart = true;
    public Vector3 cameraOffset = new Vector3(0f, 1.6f, 0f); // Eye level height
    public float mouseSensitivity = 2f;
    
    [Header("References")]
    public Transform playerTransform;
    public Camera playerCamera;
    
    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupFirstPersonCamera();
        }
    }
    
    [ContextMenu("Setup First Person Camera")]
    public void SetupFirstPersonCamera()
    {
        // Find player if not assigned
        if (playerTransform == null)
        {
            PlayerController playerController = FindFirstObjectByType<PlayerController>();
            if (playerController != null)
            {
                playerTransform = playerController.transform;
            }
            else
            {
                Debug.LogError("CameraSetupHelper: Could not find PlayerController in scene!");
                return;
            }
        }
        
        // Find or create camera
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                // Create new camera
                GameObject cameraObj = new GameObject("FirstPersonCamera");
                playerCamera = cameraObj.AddComponent<Camera>();
                playerCamera.tag = "MainCamera";
            }
        }
        
        // Setup camera hierarchy
        playerCamera.transform.SetParent(playerTransform);
        playerCamera.transform.localPosition = cameraOffset;
        playerCamera.transform.localRotation = Quaternion.identity;
        
        // Add FirstPersonCamera component if not present
        FirstPersonCamera fpsCam = playerCamera.GetComponent<FirstPersonCamera>();
        if (fpsCam == null)
        {
            fpsCam = playerCamera.gameObject.AddComponent<FirstPersonCamera>();
        }
        
        // Configure FirstPersonCamera
        fpsCam.playerBody = playerTransform;
        fpsCam.mouseSensitivity = mouseSensitivity;
        fpsCam.lockCursor = true;
        
        // Configure camera settings
        playerCamera.fieldOfView = 75f; // Good FPS FOV
        playerCamera.nearClipPlane = 0.1f;
        
        Debug.Log("First Person Camera setup complete!");
        Debug.Log("Camera position: " + playerCamera.transform.position);
        Debug.Log("Camera parent: " + playerCamera.transform.parent.name);
    }
    
    [ContextMenu("Reset Camera Position")]
    public void ResetCameraPosition()
    {
        if (playerCamera != null && playerTransform != null)
        {
            playerCamera.transform.localPosition = cameraOffset;
            playerCamera.transform.localRotation = Quaternion.identity;
            Debug.Log("Camera position reset to: " + cameraOffset);
        }
    }
    
    void OnValidate()
    {
        // Update camera position in editor when offset changes
        if (playerCamera != null && playerTransform != null && Application.isPlaying)
        {
            playerCamera.transform.localPosition = cameraOffset;
        }
    }
}
