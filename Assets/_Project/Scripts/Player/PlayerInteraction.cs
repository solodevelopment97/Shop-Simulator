using Placement;
using UnityEngine;

[RequireComponent(typeof(PlayerCarry))]
public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactableMask;
    [SerializeField] private Transform cameraTransform;

    private PlayerCarry playerCarry;
    private IInteractable currentInteractable;
    private RaycastHit lastHit;

    private void Awake()
    {
        playerCarry = GetComponent<PlayerCarry>();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (cameraTransform == null)
        {
            Debug.LogError("PlayerInteraction: CameraTransform belum di-set dan tidak menemukan MainCamera.");
        }
    }

    private void Update()
    {
        if (cameraTransform == null) return;

        DetectInteractable();
        HandleInteractionInput();
    }

    private void DetectInteractable()
    {
        // Jika sedang dalam mode penempatan furniture, jangan deteksi interaksi
        if (FurniturePlacer.Instance != null && FurniturePlacer.Instance.IsPlacing)
            return;

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        if (Physics.Raycast(ray, out lastHit, interactRange, interactableMask))
        {
            currentInteractable = lastHit.collider.GetComponent<IInteractable>();
        }
        else
        {
            currentInteractable = null;
        }
    }

    private void HandleInteractionInput()
    {
        if (!Input.GetKeyDown(KeyCode.E)) return;

        if (playerCarry.IsCarrying)
        {
            if (currentInteractable == null)
            {
                playerCarry.Drop();
            }
            else
            {
                Debug.Log("Tidak bisa berinteraksi, sedang membawa barang.");
            }
        }
        else
        {
            if (currentInteractable != null)
            {
                currentInteractable.Interact();
            }
            else
            {
                Debug.Log("Tidak ada objek untuk diinteraksi.");
            }
        }
    }
}
