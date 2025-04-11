using UnityEngine;

[RequireComponent(typeof(PlayerCarry))]
public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactableMask;
    [SerializeField] private Transform cameraTransform;

    private IInteractable currentInteractable;
    private PlayerCarry playerCarry;

    private void Start()
    {
        playerCarry = GetComponent<PlayerCarry>();
    }

    private void Update()
    {
        CheckForInteractable();
        HandleInteractionInput();
        HandlePlacementInput();
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

            return;
        }

        currentInteractable?.Interact();
    }

    private void HandlePlacementInput()
    {
        // Klik kanan untuk masuk ke mode placement jika sedang membawa item
        if (Input.GetMouseButtonDown(1) && playerCarry.IsCarrying)
        {
            playerCarry.BeginPlacementFromHand();
        }
    }

    private void CheckForInteractable()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableMask))
        {
            currentInteractable = hit.collider.GetComponent<IInteractable>();
        }
        else
        {
            currentInteractable = null;
        }
    }
}
