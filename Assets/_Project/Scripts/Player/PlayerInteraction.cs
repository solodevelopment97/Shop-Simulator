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
    private IInteractable currentInteractable;  

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
        if (interactHintText != null)
            interactHintText.enabled = false;
    }

    private void Update()
    {
        DetectInteractable();
        DetectHold();
        UpdateInteractHintUI();
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
            currentInteractable = null;
            return;
        }

        var ray = new Ray(cameraTransform.position, cameraTransform.forward);
        hasHit = Physics.Raycast(ray, out lastHit, interactRange, interactableMask);

        if (hasHit)
        {
            shelfTarget = lastHit.collider.GetComponentInParent<ShelfInteractable>();
            furnTarget = lastHit.collider.GetComponentInParent<InteractableFurniture>();
            // **baru**: tangkap IInteractable umum untuk UI hint
            currentInteractable = lastHit.collider.GetComponentInParent<IInteractable>();
        }
        else
        {
            shelfTarget = null;
            furnTarget = null;
            currentInteractable = null;
        }
    }

    private void HandleInteraction()
    {
        // --- DROP (G) ---
        if (Input.GetKeyDown(dropKey))
        {
            if (playerCarry.IsCarrying)
                playerCarry.Drop();
            return;
        }

        // --- STORE ke rak (R) ---
        if (Input.GetKeyDown(storeKey))
        {
            // hanya jalan kalau pegang ShopItem & target rak
            var held = playerCarry.HeldItem?.GetComponent<PickupItem>();
            if (held != null
             && held.itemData.itemType == ItemType.ShopItem
             && shelfTarget != null)
            {
                shelfTarget.Interact();
            }
            else
            {
                Debug.Log("Tidak ada barang untuk diletakkan ke rak.");
            }
            return;
        }

        // --- PICKUP / SWAP (E) ---
        if (Input.GetKeyDown(interactKey))
        {
            // 1) kalau ada PickupItem ShopItem di depan, ambil/swap
            if (lastHit.collider != null
             && lastHit.collider.TryGetComponent<PickupItem>(out var pick)
             && pick.itemData.itemType == ItemType.ShopItem)
            {
                pick.Interact();
                return;
            }

            // 2) kalau tangan kosong & target furniture, mulai placement
            if (!playerCarry.IsCarrying && furnTarget != null)
            {
                furnTarget.Interact();
                return;
            }

            // 3) kalau tidak ada objek lain
            Debug.Log("Tidak ada objek untuk di-interaksi.");
        }
    }

    private void UpdateInteractHintUI()
    {
        if (interactHintText == null) return;

        // 1) Kalau pegang ShopItem & menghadap rak → tombol STORE
        var held = playerCarry.HeldItem?.GetComponent<PickupItem>();
        if (held != null && held.itemData.itemType == ItemType.ShopItem
            && shelfTarget != null)
        {
            interactHintText.text = $"[R] Letakkan 1x {held.itemData.itemName}";
            interactHintText.enabled = true;
            return;
        }

        // 2) Kalau pegang apa saja & di depan ada PickupItem ShopItem → tombol PICKUP/SWAP
        if (lastHit.collider != null
         && lastHit.collider.TryGetComponent<PickupItem>(out var pickable)
         && pickable.itemData.itemType == ItemType.ShopItem)
        {
            interactHintText.text = $"[E] Ambil {pickable.itemData.itemName}";
            interactHintText.enabled = true;
            return;
        }

        // 3) Kalau tangan kosong & target furniture → tombol PLACE Furniture
        if (!playerCarry.IsCarrying && furnTarget != null)
        {
            interactHintText.text = $"[E] Pasang Furniture: {furnTarget.GetInteractText()}";
            interactHintText.enabled = true;
            return;
        }

        // 4) Kalau tidak ada yang bisa di‑interaksi
        interactHintText.enabled = false;
    }
}
