using UnityEngine;
using System.Collections;

public class PlayerLevel2 : MonoBehaviour
{
    [Header("Movement Settings")]
    public float kecepatanGerak = 5f;

    [Header("Jump Settings")]
    public float jumpForce = 7f;

    [Header("Gravity Settings")]
    public float gravityStrength = 9.81f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    // Private fields
    private Rigidbody rb;
    private Vector3 arahGerak;
    private bool isGrounded;

    void Start()
    {
        // Get the Rigidbody component
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("PlayerController: Rigidbody component not found! Please add a Rigidbody to the player.");
        }

        // Ensure we have a ground check transform
        if (groundCheck == null)
        {
            // Create a default ground check object if one is not assigned
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -1f, 0); // Position it below the player center
            groundCheck = groundCheckObj.transform;
            Debug.LogWarning("GroundCheck transform was not assigned. A default one has been created.");
        }

        // Apply the initial gravity setting. Note: This sets the global physics gravity.
        Physics.gravity = Vector3.down * gravityStrength;
    }

    void Update()
    {
        // --- Ground Check ---
        // Create a small invisible sphere at the groundCheck position to detect the ground
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // --- Movement Input ---
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Store movement direction based on input
        arahGerak = new Vector3(horizontal, 0, vertical).normalized;

        // --- Jump Input ---
        // Check for the "Jump" button (default is Spacebar) and if the player is on the ground
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }
    }

    void FixedUpdate()
    {
        // --- Apply Movement ---
        // We handle movement in FixedUpdate because it involves physics
        MovePlayer();
    }

    private void MovePlayer()
    {
        // Calculate the desired movement vector based on input and speed
        Vector3 move = arahGerak * kecepatanGerak;

        // Apply the calculated movement to the Rigidbody's velocity
        // We preserve the current vertical velocity (for jumping and falling)
        // and only change the horizontal (x) and depth (z) velocity.
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
    }

    private void Jump()
    {
        // To ensure a consistent jump height, it's good practice to reset the vertical velocity before applying the jump force.
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // Add a vertical force upwards in an instant, simulating a jump.
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    // Optional: To help visualize the ground check sphere in the editor
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }

}






