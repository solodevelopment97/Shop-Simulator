using UnityEngine;
using ShopSimulator;
using Placement;
using TMPro;

[RequireComponent(typeof(PlayerCarry))]
public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactableMask;
    [SerializeField] private Transform cameraTransform;

    [Header("UI Settings")]
    [SerializeField] private TextMeshProUGUI interactHintText;

    [Header("Key Settings")]
    [SerializeField] private KeyCode storeKey = KeyCode.R;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private KeyCode dropKey = KeyCode.G;

    [Header("Bulk Settings")]
    [Tooltip("Durasi hold R untuk bulk unpack/store")]
    [SerializeField] private float storeHoldThreshold = 0.5f;
    [Tooltip("Jeda antar unit saat bulk store/unpack")]
    [SerializeField] private float storeInterval = 0.2f;

    private PlayerCarry playerCarry;
    private RaycastHit lastHit;
    private ShelfInteractable shelfTarget;
    private InteractableFurniture furnTarget;
    private bool hasHit;

    // Bulk store state
    private bool rHeld = false;
    private bool bulkStoreMode = false;
    private float rHoldStart = 0f;
    private float lastStoreTime = 0f;

    private void Awake()
    {
        playerCarry = GetComponent<PlayerCarry>();
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
        if (cameraTransform == null)
            Debug.LogError("PlayerInteraction: CameraTransform belum di-set.");
        if (interactHintText != null)
            interactHintText.enabled = false;
    }

    private void Update()
    {
        DetectInteractable();
        DetectStoreHold();
        UpdateInteractHintUI();
        HandleInteraction();
        HandleBulkStore();
    }

    private void DetectStoreHold()
    {
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
    }

    private void DetectInteractable()
    {
        if (FurniturePlacer.Instance != null && FurniturePlacer.Instance.IsPlacing)
        {
            hasHit = false;
            shelfTarget = null;
            furnTarget = null;
            return;
        }

        var ray = new Ray(cameraTransform.position, cameraTransform.forward);
        hasHit = Physics.Raycast(ray, out lastHit, interactRange, interactableMask);
        shelfTarget = hasHit ? lastHit.collider.GetComponentInParent<ShelfInteractable>() : null;
        furnTarget = hasHit ? lastHit.collider.GetComponentInParent<InteractableFurniture>() : null;
    }

    private void HandleInteraction()
    {
        // DROP (G)
        if (Input.GetKeyDown(dropKey))
        {
            if (playerCarry.IsCarrying)
                playerCarry.Drop();
            return;
        }

        // STORE single (R tap)
        if (Input.GetKeyDown(storeKey))
        {
            TryStore();
            return;
        }

        // PICKUP / PLACE (E)
        if (Input.GetKeyDown(interactKey))
        {
            // Pickup ShopItem or Box
            if (hasHit && lastHit.collider.TryGetComponent<PickupItem>(out var pick)
                && (pick.itemData.itemType == ItemType.ShopItem || pick.itemData.itemType == ItemType.Box))
            {
                pick.Interact();
                return;
            }
            // Place furniture from hand
            if (!playerCarry.IsCarrying && furnTarget != null)
            {
                furnTarget.Interact();
                return;
            }
            Debug.Log("Tidak ada objek untuk di-interaksi.");
        }
    }

    private void HandleBulkStore()
    {
        if (!bulkStoreMode || !Input.GetKey(storeKey) || shelfTarget == null)
            return;

        if (Time.time - lastStoreTime < storeInterval)
            return;

        TryStore();
        lastStoreTime = Time.time;
    }

    private void TryStore()
    {
        var pu = playerCarry.HeldItem?.GetComponent<PickupItem>();
        if (pu == null || shelfTarget == null)
        {
            Debug.Log("Tidak ada barang untuk disimpan.");
            return;
        }
        // Panggil Interact() pada shelfTarget, yang akan unpack atau letakkan satu item
        shelfTarget.Interact();
    }

    private void UpdateInteractHintUI()
    {
        if (interactHintText == null) return;

        var pu = playerCarry.HeldItem?.GetComponent<PickupItem>();

        // STORE hint
        if (pu != null && (pu.itemData.itemType == ItemType.ShopItem || pu.itemData.itemType == ItemType.Box)
            && shelfTarget != null)
        {
            interactHintText.text = pu.itemData.itemType == ItemType.Box
                ? (bulkStoreMode ? $"Hold R: Bulk unpack {pu.itemData.itemName}" : $"[R] Unpack 1x dari {pu.itemData.itemName}")
                : (bulkStoreMode ? $"Hold R: Bulk letakkan {pu.itemData.itemName}" : $"[R] Letakkan 1x {pu.itemData.itemName}");
            interactHintText.enabled = true;
            return;
        }

        // PICKUP hint
        if (hasHit && lastHit.collider.TryGetComponent<PickupItem>(out var pickable)
            && (pickable.itemData.itemType == ItemType.ShopItem || pickable.itemData.itemType == ItemType.Box))
        {
            interactHintText.text = pickable.itemData.itemType == ItemType.Box
                ? "[E] Ambil Box"
                : $"[E] Ambil {pickable.itemData.itemName}";
            interactHintText.enabled = true;
            return;
        }

        // PLACE furniture hint
        if (!playerCarry.IsCarrying && furnTarget != null)
        {
            interactHintText.text = $"[E] {furnTarget.GetInteractText()}";
            interactHintText.enabled = true;
            return;
        }

        interactHintText.enabled = false;
    }
}

