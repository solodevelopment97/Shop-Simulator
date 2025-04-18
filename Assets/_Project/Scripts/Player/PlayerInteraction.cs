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

    [Header("Key Settings")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private KeyCode dropKey = KeyCode.G;

    [Header("Hold Settings")]
    [Tooltip("Detik untuk anggap E hold sebagai bulk")]
    [SerializeField] private float holdThreshold = 0.5f;
    [Tooltip("Jeda (detik) antar unit saat bulk")]
    [SerializeField] private float placeInterval = 0.2f;

    private PlayerCarry playerCarry;
    private RaycastHit lastHit;
    private bool hasHit;
    private ShelfInteractable shelfTarget;
    private InteractableFurniture furnTarget;

    private bool eHeld = false;
    private float eHoldStart = 0f;
    private float lastPlaceTime = 0f;
    public static bool BulkMode { get; private set; } = false;

    private void Awake()
    {
        playerCarry = GetComponent<PlayerCarry>();
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
        if (cameraTransform == null)
            Debug.LogError("PlayerInteraction: CameraTransform belum di-set.");
    }

    private void Update()
    {
        DetectInteractable();
        DetectHold();
        HandleInteraction();
    }

    private void DetectHold()
    {
        if (Input.GetKeyDown(interactKey))
        {
            eHeld = true;
            eHoldStart = Time.time;
            BulkMode = false;
            lastPlaceTime = Time.time - placeInterval; // agar langsung placeOneUnit jika hold
        }

        if (eHeld && !BulkMode && Time.time - eHoldStart >= holdThreshold)
        {
            BulkMode = true;
            eHeld = false;
        }

        if (Input.GetKeyUp(interactKey))
        {
            eHeld = false;
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

        if (hasHit)
        {
            shelfTarget = lastHit.collider.GetComponentInParent<ShelfInteractable>();
            furnTarget = lastHit.collider.GetComponentInParent<InteractableFurniture>();
        }
        else
        {
            shelfTarget = null;
            furnTarget = null;
        }
    }

    private void HandleInteraction()
    {
        // 1) Drop (G)
        if (Input.GetKeyDown(dropKey))
        {
            if (playerCarry.IsCarrying)
                playerCarry.Drop();
            return;
        }

        // 2) Bulk hold: saat BulkMode & masih tekan E
        var holdingE = Input.GetKey(interactKey);
        var heldPickup = playerCarry.HeldItem?.GetComponent<PickupItem>();
        bool carryingShop = heldPickup != null && heldPickup.itemData.itemType == ItemType.ShopItem;

        if (BulkMode && holdingE && carryingShop)
        {
            if (shelfTarget != null && Time.time - lastPlaceTime >= placeInterval)
            {
                if (!shelfTarget.PlaceOneUnit())
                {
                    Debug.Log("Rak penuh atau kardus habis.");
                    // jika habis atau penuh, disable bulk agar tidak terus mencoba
                    BulkMode = false;
                }
                lastPlaceTime = Time.time;
            }
            return;
        }

        // 3) Tap E (KeyUp): satu unit
        if (Input.GetKeyUp(interactKey))
        {
            // A) Carry Shop + target rak → one unit
            if (carryingShop)
            {
                if (shelfTarget != null)
                    shelfTarget.Interact();
                else
                    Debug.Log("Arahkan ke rak untuk meletakkan ShopItem.");
                return;
            }

            // B) Carry Furniture → placement from hand
            if (heldPickup != null && heldPickup.itemData.itemType == ItemType.Furniture)
            {
                playerCarry.BeginPlacementFromHand();
                return;
            }

            // C) Hand empty → pickup item atau new furniture
            if (!playerCarry.IsCarrying)
            {
                if (!hasHit)
                {
                    Debug.Log("Tidak ada objek untuk diinteraksi.");
                }
                else if (lastHit.collider.TryGetComponent<PickupItem>(out var pick))
                {
                    pick.Interact();
                }
                else if (furnTarget != null)
                {
                    furnTarget.Interact();
                }
                else
                {
                    Debug.Log($"Objek {lastHit.collider.name} tidak bisa diinteraksi.");
                }
                return;
            }

            Debug.Log("Interaksi tidak tersedia dalam kondisi ini.");
        }
    }
}
