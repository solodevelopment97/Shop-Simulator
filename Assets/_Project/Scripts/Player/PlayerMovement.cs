using TMPro;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] public float runSpeed = 6f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 15f;

    [Header("Advanced Settings")]
    [SerializeField] private float gravity = -15f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundMask;

    [Header("Stamina")]
    [SerializeField] private PlayerStamina staminaSystem;

    [Header("Debug")]
    [SerializeField] private TextMeshProUGUI movementsDebug;
    [SerializeField] private bool enableDebugLogs = true;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float currentSpeed;
    private Vector3 currentVelocity;
    private bool isRunning;

    public bool IsRunning => isRunning;
    public bool IsMoving => Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f;
    // Public property to expose currentSpeed
    public float CurrentSpeed => currentSpeed;

    private void Awake()
    {
        InitializeComponents();
        currentSpeed = walkSpeed;

        if (enableDebugLogs)
        {
            Debug.Log("PlayerMovement initialized");
            Debug.Log($"Initial speed: {currentSpeed}");
        }
    }

    private void Update()
    {
        HandleGravity();
        HandleMovement();
        UpdateDebugDisplay();
    }

    /// <summary>
    /// Initializes required components and logs errors if any are missing.
    /// </summary>
    private void InitializeComponents()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.LogError("CharacterController component is missing on PlayerMovement.");
            enabled = false;
        }

        if (staminaSystem == null)
        {
            Debug.LogWarning("StaminaSystem is not assigned. Running will not be restricted by stamina.");
        }
    }

    /// <summary>
    /// Updates the debug display with movement information.
    /// </summary>
    private void UpdateDebugDisplay()
    {
        if (movementsDebug != null)
        {
            movementsDebug.text = $"Speed: {currentSpeed:F2}\n" +
                                  $"Grounded: {isGrounded}\n" +
                                  $"Running: {isRunning}\n" +
                                  $"Velocity: {currentVelocity}\n" +
                                  $"Input: ({Input.GetAxisRaw("Horizontal"):F2}, {Input.GetAxisRaw("Vertical"):F2})";
        }
    }

    /// <summary>
    /// Toggles the running state based on stamina availability.
    /// </summary>
    /// <param name="enable">True to enable running, false to disable.</param>
    public void ToggleRun(bool enable)
    {
        if (!enable || (staminaSystem == null || staminaSystem.CanRun()))
        {
            isRunning = enable;

            if (enableDebugLogs)
            {
                Debug.Log($"Run toggled: {enable}");
                if (staminaSystem != null) Debug.Log($"Stamina available: {staminaSystem.CanRun()}");
            }
        }
    }

    /// <summary>
    /// Handles gravity and ground detection.
    /// </summary>
    private void HandleGravity()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundMask);

        if (enableDebugLogs && wasGrounded != isGrounded)
        {
            Debug.Log(isGrounded ? "Player grounded" : "Player in air");
        }

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -1f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    /// <summary>
    /// Handles player movement, including walking, running, and acceleration.
    /// </summary>
    private void HandleMovement()
    {
        // Cache input values for performance
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        bool wasRunning = isRunning;

        bool wantToRun = Input.GetKey(KeyCode.LeftShift);
        bool canRun = staminaSystem == null || staminaSystem.CanRun();
        isRunning = wantToRun && canRun;

        if (wasRunning && !canRun)
        {
            isRunning = false;
            if (enableDebugLogs)
            {
                Debug.Log("Forced to stop running due to stamina depletion");
            }
        }

        if (enableDebugLogs && wasRunning != isRunning)
        {
            Debug.Log(isRunning ? "Started running" : "Stopped running");
        }

        float targetSpeed = isRunning ? runSpeed : walkSpeed;

        if (!canRun && wantToRun)
        {
            targetSpeed = walkSpeed;
        }

        float previousSpeed = currentSpeed;

        // Smooth acceleration/deceleration
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed,
            (horizontal != 0 || vertical != 0) ? acceleration * Time.deltaTime : deceleration * Time.deltaTime);

        if (enableDebugLogs && Mathf.Abs(previousSpeed - currentSpeed) > 0.1f)
        {
            Debug.Log($"Speed changed from {previousSpeed:F2} to {currentSpeed:F2}");
        }

        Vector3 moveDirection = transform.TransformDirection(new Vector3(horizontal, 0, vertical)).normalized;
        Vector3 targetVelocity = moveDirection * currentSpeed;

        // Log input changes
        if (enableDebugLogs && (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f))
        {
            Debug.Log($"Movement input - Horizontal: {horizontal:F2}, Vertical: {vertical:F2}");
            Debug.Log($"Move direction: {moveDirection}");
        }

        // Smooth movement transitions
        Vector3 previousVelocity = currentVelocity;
        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, acceleration * Time.deltaTime);

        if (enableDebugLogs && (currentVelocity - previousVelocity).magnitude > 0.1f)
        {
            Debug.Log($"Velocity changed from {previousVelocity} to {currentVelocity}");
        }

        controller.Move(currentVelocity * Time.deltaTime);
    }
}
