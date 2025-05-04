using UnityEngine;
using ShopSimulator;
using Placement;
using TMPro;

[RequireComponent(typeof(PlayerCarry))]
public class PlayerInteraction : MonoBehaviour
{
    public static PlayerInteraction Instance { get; private set; }

    [Header("Interaction Settings")]
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private Transform cameraTransform;

    [Header("UI Settings")]
    [SerializeField] private UIHintManager uiHintManager;

    [Header("Key Settings")]
    [SerializeField] private KeyCode storeKey = KeyCode.R;
    [SerializeField] private KeyCode interactKey = KeyCode.E; // Tetap untuk item pickup
    [SerializeField] private KeyCode dropKey = KeyCode.G;
    [SerializeField] private KeyCode pickupFurnitureKey = KeyCode.F; // KeyCode untuk furniture pickup

    [Header("Bulk Store Settings")]
    [SerializeField] private float storeHoldThreshold = 0.5f;
    [SerializeField] private float storeInterval = 0.2f;

    [Header("LayerMasks")]
    [SerializeField] private LayerMask shelfMask;
    [SerializeField] private LayerMask pickupMask;

    private PlayerCarry playerCarry;
    private RaycastHit lastHit;

    // Bulk store state
    private bool isStoreKeyHeld = false;
    private bool isBulkStoreMode = false;
    private float storeKeyHoldStartTime = 0f;
    private float lastStoreTime = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        playerCarry = GetComponent<PlayerCarry>();
        cameraTransform ??= Camera.main?.transform;
    }
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    private void Update()
    {
        HandleInput();
        UpdateInteractHintUI();
    }

    private void HandleInput()
    {
        HandleDropInput();
        HandleStoreInput();
        HandlePickupItemInput(); // Tetap untuk item pickup
        HandlePickupFurnitureInput(); // Tambahkan handler untuk furniture pickup
    }

    private void HandleDropInput()
    {
        if (Input.GetKeyDown(dropKey) && playerCarry.IsCarrying)
        {
            playerCarry.Drop();
        }
    }

    private void HandleStoreInput()
    {
        if (Input.GetKeyDown(storeKey))
        {
            isStoreKeyHeld = true;
            isBulkStoreMode = false;
            storeKeyHoldStartTime = Time.time;
            lastStoreTime = Time.time - storeInterval;
        }

        if (isStoreKeyHeld && !isBulkStoreMode && Time.time - storeKeyHoldStartTime >= storeHoldThreshold)
        {
            isBulkStoreMode = true;
            isStoreKeyHeld = false;
        }

        if (Input.GetKeyUp(storeKey))
        {
            isStoreKeyHeld = false;
            isBulkStoreMode = false;
        }

        if (Input.GetKeyDown(storeKey))
        {
            StoreItem();
        }

        if (isBulkStoreMode && Input.GetKey(storeKey) && Time.time - lastStoreTime >= storeInterval)
        {
            StoreItem();
            lastStoreTime = Time.time;
        }
    }

    private void StoreItem()
    {
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out var hit, interactRange, shelfMask))
        {
            var shelf = hit.collider.GetComponentInParent<ShelfInteractable>();
            if (shelf != null && playerCarry.IsCarrying && shelf.CanStoreItem())
            {
                shelf.Interact();
                return;
            }
        }
        Debug.Log("Point at a shelf to store the item.");
    }

    private void HandlePickupItemInput()
    {
        if (!Input.GetKeyDown(interactKey)) return;

        if (TryPickupItem())
        {
            Debug.Log("Item picked up.");
        }
        else
        {
            Debug.Log("No item to pick up.");
        }
    }

    private void HandlePickupFurnitureInput()
    {
        if (!Input.GetKeyDown(pickupFurnitureKey)) return;

        if (TryPickupFurniture())
        {
            Debug.Log("Furniture picked up.");
        }
        else
        {
            Debug.Log("No furniture to pick up.");
        }
    }

    private bool TryPickupItem()
    {
        if (FurniturePlacer.Instance != null && FurniturePlacer.Instance.IsPlacing)
        {
            Debug.Log("Cannot pick up items while in furniture placement mode.");
            return false;
        }
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out var hit, interactRange, pickupMask))
        {
            if (hit.collider.TryGetComponent<PickupItem>(out var pickupItem))
            {
                pickupItem.Interact();
                return true;
            }
        }
        return false;
    }

    private bool TryPickupFurniture()
    {
        if (FurniturePlacer.Instance != null && FurniturePlacer.Instance.IsPlacing)
        {
            Debug.Log("Cannot pick up furniture while in furniture placement mode.");
            return false;
        }
        if (playerCarry.IsCarrying) return false;

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out var hit, interactRange, shelfMask | pickupMask))
        {
            var furniture = hit.collider.GetComponentInParent<InteractableFurniture>();
            if (furniture != null)
            {
                furniture.Interact();
                return true;
            }
        }
        return false;
    }

    private void UpdateInteractHintUI()
    {
        if (uiHintManager == null) return;

        // Bersihkan semua hint sebelum memperbarui
        uiHintManager.ClearAllHints();

        // Hint untuk item yang sedang dibawa
        if (playerCarry.IsCarrying)
        {
            var heldItem = playerCarry.HeldItem?.GetComponent<PickupItem>();
            if (heldItem != null)
            {
                uiHintManager.SetHint("Drop", $"[G] Drop {heldItem.itemData.itemName}");
            }
        }

        // Hint untuk menyimpan item
        if (playerCarry.IsCarrying && Physics.Raycast(cameraTransform.position, cameraTransform.forward, out var hit, interactRange, shelfMask))
        {
            var shelf = hit.collider.GetComponentInParent<ShelfInteractable>();
            if (shelf != null && shelf.CanStoreItem())
            {
                var heldItem = playerCarry.HeldItem?.GetComponent<PickupItem>();
                if (heldItem != null)
                {
                    uiHintManager.SetHint("Store", GetStoreHintText(heldItem));
                }
            }
        }

        if (FurniturePlacer.Instance != null && FurniturePlacer.Instance.IsPlacing)
        {
            return;
        }

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out var hitPickup, interactRange, pickupMask))
        {
            if (hitPickup.collider.TryGetComponent<PickupItem>(out var pickupItem))
            {
                uiHintManager.SetHint("Pickup", GetPickupHintText(pickupItem));
            }
        }

        // Hint untuk mengambil furniture
        if (!playerCarry.IsCarrying && Physics.Raycast(cameraTransform.position, cameraTransform.forward, out var hitFurniture, interactRange, shelfMask | pickupMask))
        {
            var furniture = hitFurniture.collider.GetComponentInParent<InteractableFurniture>();
            if (furniture != null)
            {
                uiHintManager.SetHint("Furniture", $"[F] {furniture.GetInteractText()}");
            }
        }
    }

    private string GetStoreHintText(PickupItem heldItem)
    {
        if (heldItem.itemData.itemType == ItemType.Box)
        {
            return isBulkStoreMode
                ? $"Hold R: Bulk unpack {heldItem.itemData.itemName}"
                : $"[R] Unpack 1x {heldItem.itemData.itemName}";
        }
        else
        {
            return isBulkStoreMode
                ? $"Hold R: Bulk store {heldItem.itemData.itemName}"
                : $"[R] Store 1x {heldItem.itemData.itemName}";
        }
    }

    private string GetPickupHintText(PickupItem pickupItem)
    {
        return pickupItem.itemData.itemType == ItemType.Box
            ? "[E] Pick up Box"
            : $"[E] Pick up {pickupItem.itemData.itemName}";
    }

    public void UpdatePlacementHint(bool isBlocked)
    {
        if (uiHintManager != null)
        {          
            if (isBlocked)
            {
                uiHintManager.SetHint("Placement", "Cannot place here!");
            }
            else
            {
                uiHintManager.SetHint("Placement", "Press Left Mouse Button to place.");
                uiHintManager.SetHint("CancelPlacement", "Press [Escape] to cancel placement.");
                uiHintManager.SetHint("RotatePlacement", "Use [Scroll Wheel] to rotate the furniture.");
            }
        }
    }

    public void ClearPlacementHint()
    {
        if (uiHintManager != null)
        {
            uiHintManager.ClearHint("Placement");
            uiHintManager.ClearHint("CancelPlacement");
            uiHintManager.ClearHint("RotatePlacement");
        }
    }
}
