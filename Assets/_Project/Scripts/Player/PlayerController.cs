using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private Camera mainCamera;

    private void Awake()
    {
        InitializeComponents();
    }

    /// <summary>
    /// Enables or disables player controls.
    /// </summary>
    /// <param name="enable">True to enable controls, false to disable.</param>
    public void EnableControls(bool enable)
    {
        if (movement != null) movement.enabled = enable;
        if (playerCamera != null) playerCamera.enabled = enable;
    }

    /// <summary>
    /// Dynamically adjusts the camera sensitivity.
    /// </summary>
    /// <param name="sensitivityX">Horizontal sensitivity.</param>
    /// <param name="sensitivityY">Vertical sensitivity.</param>
    public void SetCameraSensitivity(float sensitivityX, float sensitivityY)
    {
        if (playerCamera != null)
        {
            playerCamera.SetMouseSensitivity(sensitivityX, sensitivityY);
        }
        else
        {
            Debug.LogWarning("PlayerCamera is not assigned. Cannot set sensitivity.");
        }
    }

    /// <summary>
    /// Initializes required components and logs errors if any are missing.
    /// </summary>
    private void InitializeComponents()
    {
        if (movement == null)
        {
            movement = GetComponent<PlayerMovement>();
            if (movement == null)
            {
                Debug.LogError("PlayerMovement component is missing on PlayerController.");
            }
        }

        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<PlayerCamera>();
            if (playerCamera == null)
            {
                Debug.LogError("PlayerCamera component is missing in children of PlayerController.");
            }
        }

        if (mainCamera == null)
        {
            mainCamera = GetComponentInChildren<Camera>();
            if (mainCamera == null)
            {
                Debug.LogError("Main Camera is missing in children of PlayerController.");
            }
        }
    }
}
