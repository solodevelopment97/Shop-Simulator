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
        playerMovement = playerBody.GetComponent<PlayerMovement>();
        defaultPosY = transform.localPosition.y;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleMouseLook();
        HandleHeadBobbing();
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivityX * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivityY * Time.deltaTime;

        mouseY = invertY ? mouseY : -mouseY;

        xRotation += mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxVerticalAngle, maxVerticalAngle);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }

    private void HandleHeadBobbing()
    {
        if (Mathf.Abs(Input.GetAxis("Horizontal")) < 0.1f &&
            Mathf.Abs(Input.GetAxis("Vertical")) < 0.1f)
        {
            timer = 0f;
            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                new Vector3(transform.localPosition.x, midpoint, transform.localPosition.z),
                Time.deltaTime * 10f);
            return;
        }

        float waveslice = Mathf.Sin(timer);
        float bobbingSpeed = playerMovement.IsRunning ? runningBobbingSpeed : walkingBobbingSpeed;
        timer += bobbingSpeed * Time.deltaTime;
        if (timer > Mathf.PI * 2) timer -= Mathf.PI * 2;

        float translateChange = waveslice * bobbingAmount;
        float totalAxes = Mathf.Clamp01(Mathf.Abs(Input.GetAxis("Horizontal")) + Mathf.Abs(Input.GetAxis("Vertical")));
        translateChange *= totalAxes;

        Vector3 localPos = transform.localPosition;
        localPos.y = midpoint + translateChange;
        transform.localPosition = localPos;
    }
}