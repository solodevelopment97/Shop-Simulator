using UnityEngine;
using ShopSimulator;
using Placement;

[RequireComponent(typeof(PlayerCarry))]
public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
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
        // Tekan G untuk melakukan Drop
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (playerCarry.IsCarrying)
            {
                playerCarry.Drop();
            }
            return;
        }

        // Tekan E untuk Interaksi
        if (!Input.GetKeyDown(KeyCode.E)) return;

        if (playerCarry.IsCarrying)
        {
            // Jika sedang membawa barang, dan ada objek interaksi,
            // periksa apakah objek interaksi merupakan item shop.
            if (currentInteractable != null)
            {
                PickupItem pickup = lastHit.collider.GetComponent<PickupItem>();
                if (pickup != null)
                {
                    // Jika objek interaksi merupakan ShopItem, interaksi diizinkan (misalnya pickup atau swap)
                    if (pickup.itemData.itemType == ItemType.ShopItem)
                    {
                        currentInteractable.Interact();
                    }
                    else
                    {
                        Debug.Log("Tidak bisa berinteraksi, sedang membawa barang (Furniture tidak bisa diambil saat membawa barang).");
                    }
                }
                else
                {
                    Debug.Log("Tidak bisa berinteraksi, sedang membawa barang.");
                }
            }
            else
            {
                // Jika tidak ada objek interaksi, tidak terjadi apa-apa (drop dilakukan dengan G)
                Debug.Log("Tidak ada objek untuk diinteraksi.");
            }
        }
        else
        {
            // Jika tidak membawa barang, langsung lakukan interaksi pada objek yang terdeteksi
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
