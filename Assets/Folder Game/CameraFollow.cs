using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // The object the camera will follow (the player)
    public Transform target;

    // How fast the camera will follow
    public float smoothSpeed = 0.125f;
    
    // The distance and angle from the player
    public Vector3 offset;

    void LateUpdate()
    {
        if (target != null)
        {
            // The position the camera is trying to reach
            Vector3 desiredPosition = target.position + offset;
            
            // Smoothly move from the current position to the desired position
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            
            // Apply the new position
            transform.position = smoothedPosition;

            // Make the camera look at the player
            transform.LookAt(target);
        }
    }
}