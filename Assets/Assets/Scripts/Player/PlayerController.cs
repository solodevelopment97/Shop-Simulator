using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private Camera mainCamera;

    private void Awake()
    {
        if (movement == null) movement = GetComponent<PlayerMovement>();
        if (playerCamera == null) playerCamera = GetComponentInChildren<PlayerCamera>();
        if (mainCamera == null) mainCamera = GetComponentInChildren<Camera>();
    }

    public void EnableControls(bool enable)
    {
        movement.enabled = enable;
        playerCamera.enabled = enable;
    }
}