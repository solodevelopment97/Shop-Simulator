using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Transform playerBody;
    [SerializeField] private float mouseSensitivityX = 200f;
    [SerializeField] private float mouseSensitivityY = 200f;
    [SerializeField] private bool invertY = false;
    [SerializeField] private float maxVerticalAngle = 90f;

    [Header("Bobbing Settings")]
    [SerializeField] private float walkingBobbingSpeed = 12f;
    [SerializeField] private float runningBobbingSpeed = 16f;
    [SerializeField] private float bobbingAmount = 0.03f;
    [SerializeField] private float midpoint = 1.5f;

    private float xRotation = 0f;
    private float defaultPosY = 0f;
    private float timer = 0f;
    private PlayerMovement playerMovement;

    private void Awake()
    {
        // Ensure playerBody is assigned
        if (playerBody == null)
        {
            Debug.LogError("PlayerBody is not assigned in PlayerCamera.");
            enabled = false;
            return;
        }

        playerMovement = playerBody.GetComponent<PlayerMovement>();
        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovement component not found on PlayerBody.");
            enabled = false;
            return;
        }

        defaultPosY = transform.localPosition.y;

        // Lock the cursor for better camera control
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleMouseLook();
        HandleHeadBobbing();
    }

    /// <summary>
    /// Handles the camera rotation based on mouse input.
    /// </summary>
    private void HandleMouseLook()
    {
        // Cache input values for performance
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivityX * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivityY * Time.deltaTime;

        // Apply Y-axis inversion if enabled
        mouseY = invertY ? mouseY : -mouseY;

        // Adjust vertical rotation and clamp it
        xRotation += mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxVerticalAngle, maxVerticalAngle);

        // Apply rotation to the camera and player body
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }

    /// <summary>
    /// Handles the head bobbing effect during movement.
    /// </summary>
    private void HandleHeadBobbing()
    {
        // Check if the player is moving
        bool isMoving = Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f || Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f;

        if (!isMoving)
        {
            // Reset bobbing when idle
            timer = 0f;
            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                new Vector3(transform.localPosition.x, midpoint, transform.localPosition.z),
                Time.deltaTime * 10f);
            return;
        }

        // Calculate bobbing effect
        float bobbingSpeed = playerMovement.IsRunning ? runningBobbingSpeed : walkingBobbingSpeed;
        timer += bobbingSpeed * Time.deltaTime;

        if (timer > Mathf.PI * 2) timer -= Mathf.PI * 2;

        float waveslice = Mathf.Sin(timer);
        float translateChange = waveslice * bobbingAmount;
        float totalAxes = Mathf.Clamp01(Mathf.Abs(Input.GetAxis("Horizontal")) + Mathf.Abs(Input.GetAxis("Vertical")));
        translateChange *= totalAxes;

        // Apply bobbing effect
        Vector3 localPos = transform.localPosition;
        localPos.y = midpoint + translateChange;
        transform.localPosition = localPos;
    }

    /// <summary>
    /// Allows runtime adjustment of mouse sensitivity.
    /// </summary>
    public void SetMouseSensitivity(float sensitivityX, float sensitivityY)
    {
        mouseSensitivityX = sensitivityX;
        mouseSensitivityY = sensitivityY;
    }
}
