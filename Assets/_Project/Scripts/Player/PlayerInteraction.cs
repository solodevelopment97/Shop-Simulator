using UnityEngine;
using ShopSimulator;
using Placement;
using TMPro;

[RequireComponent(typeof(PlayerCarry))]
public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private Transform cameraTransform;

    [Header("UI Settings")]
    [SerializeField] private TextMeshProUGUI interactHintText;

    [Header("Key Settings")]
    [SerializeField] private KeyCode storeKey = KeyCode.R;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private KeyCode dropKey = KeyCode.G;

    [Header("Bulk Store Settings")]
    [SerializeField] private float storeHoldThreshold = 0.5f;
    [SerializeField] private float storeInterval = 0.2f;

    [Header("LayerMasks")]
    public LayerMask shelfMask;
    public LayerMask pickupMask;

    private PlayerCarry playerCarry;
    private RaycastHit lastHit;

    // state for bulk store
    private bool rHeld = false;
    private bool bulkStoreMode = false;
    private float rHoldStart = 0f;
    private float lastStoreTime = 0f;

    private void Awake()
    {
        playerCarry = GetComponent<PlayerCarry>();
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (interactHintText != null)
            interactHintText.enabled = false;
    }

    private void Update()
    {
        HandleDrop();
        HandleStoreInput();
        HandlePickupOrPlace();
        UpdateInteractHintUI();
    }

    private void HandleDrop()
    {
        if (Input.GetKeyDown(dropKey) && playerCarry.IsCarrying)
            playerCarry.Drop();
    }

    private void HandleStoreInput()
    {
        // detect hold vs tap
        if (Input.GetKeyDown(storeKey))
        {
            rHeld = true;
            bulkStoreMode = false;
            rHoldStart = Time.time;
            lastStoreTime = Time.time - storeInterval;
        }

        if (rHeld && !bulkStoreMode && Time.time - rHoldStart >= storeHoldThreshold)
        {
            bulkStoreMode = true;
            rHeld = false;
        }

        if (Input.GetKeyUp(storeKey))
        {
            rHeld = false;
            bulkStoreMode = false;
        }

        // on tap
        if (Input.GetKeyDown(storeKey))
            StoreOnce();

        // on hold
        if (bulkStoreMode && Input.GetKey(storeKey))
        {
            if (Time.time - lastStoreTime >= storeInterval)
            {
                StoreOnce();
                lastStoreTime = Time.time;
            }
        }
    }

    private void StoreOnce()
    {
        // raycast hanya ke layer rak
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out var hit, interactRange, shelfMask))
        {
            var shelf = hit.collider.GetComponentInParent<ShelfInteractable>();
            if (shelf != null && playerCarry.IsCarrying && shelf.CanStoreItem())
            {
                shelf.Interact();  // meletakkan 1 unit atau unpack box
                return;
            }
        }
        Debug.Log("Arahkan ke rak untuk menyimpan barang.");
    }

    private void HandlePickupOrPlace()
    {
        if (!Input.GetKeyDown(interactKey))
            return;

        // 1) pickup item/box
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out var hitPi, interactRange, pickupMask))
        {
            if (hitPi.collider.TryGetComponent<PickupItem>(out var pick))
            {
                pick.Interact();
                return;
            }
        }

        // 2) place furniture
        if (!playerCarry.IsCarrying)
        {
            // raycast any interactable furniture
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out var hitF, interactRange, shelfMask | pickupMask))
            {
                var furn = hitF.collider.GetComponentInParent<InteractableFurniture>();
                if (furn != null)
                {
                    furn.Interact();
                    return;
                }
            }
        }

        Debug.Log("Tidak ada objek untuk di-interaksi.");
    }

    private void UpdateInteractHintUI()
    {
        if (interactHintText == null) return;

        bool show = false;
        string text = "";

        // hint STORE
        if (playerCarry.IsCarrying)
        {
            var pu = playerCarry.HeldItem.GetComponent<PickupItem>();
            if (pu != null && Physics.Raycast(cameraTransform.position, cameraTransform.forward, out var hs, interactRange, shelfMask))
            {
                if (pu.itemData.itemType == ItemType.Box)
                    text = bulkStoreMode ? $"Hold R: Bulk unpack {pu.itemData.itemName}"
                                         : $"[R] Unpack 1x dari {pu.itemData.itemName}";
                else
                    text = bulkStoreMode ? $"Hold R: Bulk letakkan {pu.itemData.itemName}"
                                         : $"[R] Letakkan 1x {pu.itemData.itemName}";
                show = true;
            }
        }

        // hint PICKUP
        if (!show && Physics.Raycast(cameraTransform.position, cameraTransform.forward, out var hp, interactRange, pickupMask))
        {
            if (hp.collider.TryGetComponent<PickupItem>(out var pickable))
            {
                text = pickable.itemData.itemType == ItemType.Box
                    ? "[E] Ambil Box"
                    : $"[E] Ambil {pickable.itemData.itemName}";
                show = true;
            }
        }

        // hint PLACE furniture
        if (!show && !playerCarry.IsCarrying)
        {
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out var hf, interactRange, shelfMask | pickupMask))
            {
                var furn = hf.collider.GetComponentInParent<InteractableFurniture>();
                if (furn != null)
                {
                    text = $"[E] {furn.GetInteractText()}";
                    show = true;
                }
            }
        }

        interactHintText.text = text;
        interactHintText.enabled = show;
    }
}
