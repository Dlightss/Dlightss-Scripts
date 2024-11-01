using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    public float walkSpeed = 10f; // Speed when walking
    public float jumpForce = 10f; // Initial force applied when jumping
    public float variableJumpForce = 5f; // Additional force for variable jump height
    public float sprintSpeed = 15f; // Speed when sprinting

    [Header("Jump Settings")]
    public float jumpBufferTime = 0.2f; // Time window to register a jump input
    public float coyoteTime = 0.2f; // Time window after leaving the ground to still jump
    public float maxJumpHoldTime = 0.25f; // Maximum time the jump button can be held for variable jump

    [Header("Ground Check Settings")]
    public LayerMask groundLayer; // Layer to check for ground
    public Transform groundCheck; // Position to check if player is grounded
    public Vector2 groundCheckSize = new Vector2(0.2f, 0.1f); // Size of the ground check box

    [Header("Debug Stuff")]
    public float horizontalSpeed; // Current horizontal speed of the player
    public float verticalSpeed; // Current vertical speed of the player

    private Rigidbody2D rb; // Reference to the Rigidbody2D component
    private float moveInput; // Input value for horizontal movement
    private float jumpBufferCounter; // Counter for jump buffer time
    private float coyoteTimeCounter; // Counter for coyote time
    public bool isGrounded; // Boolean to check if the player is on the ground

    // Timer for jump button hold
    private float jumpHoldTime; // Time the jump button has been held down

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // Get the Rigidbody2D component attached to the player
    }

    private void Update()
    {
        HandleMovement(); // Call the method to handle player movement
    }

    private void FixedUpdate()
    {
        MovePlayer(); // Apply movement to the player
        HandleRotation(); // Handle player rotation based on movement direction
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red; // Set gizmo color to red
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize); // Draw a wireframe cube for ground check
    }

    private void HandleMovement()
    {
        GroundCheck(); // Check if the player is on the ground
        HandleJumpInput(); // Handle input for jumping
        DetectSpeed(); // Detect current speed of the player
    }

    private void GroundCheck()
    {
        // Check if the player is overlapping with the ground layer
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
        // Update coyote time counter
        coyoteTimeCounter = isGrounded ? coyoteTime : Mathf.Max(0, coyoteTimeCounter - Time.deltaTime);
    }

    private void MovePlayer()
    {
        // Get horizontal input (left/right movement)
        moveInput = Input.GetAxisRaw("Horizontal");
        // Set the player's velocity based on input and modified speed
        rb.velocity = new Vector2(moveInput * GetModifiedSpeed(), rb.velocity.y);
    }

    private void HandleJumpInput()
    {
        // Check for jump input
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime; // Reset jump buffer when button is pressed
            jumpHoldTime = 0; // Reset jump hold time
        }

        // Decrease jump buffer counter over time
        jumpBufferCounter = Mathf.Max(0, jumpBufferCounter - Time.deltaTime);

        // Check if we can jump (either grounded or within coyote time)
        if ((isGrounded || coyoteTimeCounter > 0) && jumpBufferCounter > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0); // Reset vertical velocity before jumping
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse); // Apply jump force
            jumpBufferCounter = 0; // Reset jump buffer after jumping
        }

        // Handle variable jump (holding the jump button)
        if (rb.velocity.y > 0 && Input.GetButton("Jump"))
        {
            // Increase the jump hold time
            jumpHoldTime += Time.deltaTime;

            // Only apply variable jump force if within max hold time
            if (jumpHoldTime < maxJumpHoldTime)
            {
                rb.AddForce(Vector2.up * variableJumpForce, ForceMode2D.Force); // Apply additional force for variable jump
            }
        }
    }

    private void HandleRotation()
    {
        // Rotate the player based on movement direction
        if (moveInput < 0)
        {
            transform.eulerAngles = new Vector3(0, 180, 0); // Face left
        }
        else if (moveInput > 0)
        {
            transform.eulerAngles = new Vector3(0, 0, 0); // Face right
        }
        // Do nothing if moveInput is 0 (no rotation)
    }

    private float GetModifiedSpeed()
    {
        // Return sprint speed if the Left Shift key is held down, otherwise return walk speed
        if (Input.GetKey(KeyCode.LeftShift)) return sprintSpeed;
        return walkSpeed;
    }

    private void DetectSpeed()
    {
        // Store the absolute value of horizontal and vertical speed for debugging
        horizontalSpeed = Mathf.Abs(rb.velocity.x);
        verticalSpeed = rb.velocity.y;
    }
}