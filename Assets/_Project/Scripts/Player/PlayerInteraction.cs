// Scripts/Player/PlayerInteraction.cs
using UnityEngine;

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
    void Update()
    {
        CheckForInteractable();

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (playerCarry != null && playerCarry.IsCarrying)
            {
                if (currentInteractable == null)
                {
                    // Drop jika tidak ada target interaksi
                    playerCarry.Drop();
                }
                else
                {
                    // TOLAK interaksi jika sedang bawa barang
                    Debug.Log("Tidak bisa berinteraksi, sedang membawa barang.");
                }

                return;
            }

            if (currentInteractable != null)
            {
                currentInteractable.Interact();
            }
        }
    }

    void CheckForInteractable()
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
