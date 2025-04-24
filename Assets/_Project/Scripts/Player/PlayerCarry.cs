using UnityEngine;
using Placement;
using ShopSimulator;

[RequireComponent(typeof(FurniturePlacer))]
public class PlayerCarry : MonoBehaviour
{
    [Header("Carry Settings")]
    [SerializeField] private Transform holdPoint;
    [SerializeField] private Camera cam;

    [Header("Bobbing Effect Settings")]
    [Tooltip("Kecepatan dasar bobbing (bila player bergerak)")]
    [SerializeField] private float baseBobbingSpeed = 6f;
    [Tooltip("Besaran offset bobbing")]
    [SerializeField] private float baseBobbingAmount = 0.05f;

    // Timer untuk menghitung gelombang bobbing
    private float bobbingTimer = 0f;
    // Simpan posisi default holdPoint atau posisi awal item di holdPoint
    private Vector3 defaultHoldLocalPosition;

    private GameObject carriedItem;
    private FurniturePlacer furniturePlacer;
    private PlayerMovement playerMovement;
    private Vector3 bobbingOffset = Vector3.zero;

    public bool IsCarrying => carriedItem != null;
    public Transform HoldPoint => holdPoint;

    public GameObject HeldItem => carriedItem;

    private void Awake()
    {
        if (holdPoint == null)
        {
            Debug.LogWarning("HoldPoint belum di-assign di Inspector. Membuat default.");
            holdPoint = new GameObject("DefaultHoldPoint").transform;
            holdPoint.SetParent(transform);
            holdPoint.localPosition = new Vector3(0, 1.5f, 1);
        }
        defaultHoldLocalPosition = holdPoint.localPosition;

        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null)
                Debug.LogError("Kamera tidak ditemukan. Pastikan memiliki MainCamera di scene.");
        }

        furniturePlacer = GetComponent<FurniturePlacer>();
        playerMovement = GetComponent<PlayerMovement>();

        if (playerMovement == null)
            Debug.LogError("PlayerMovement tidak ditemukan.");
        if (furniturePlacer == null)
            Debug.LogError("FurniturePlacer tidak ditemukan.");
    }

    private void Update()
    {
        if (IsCarrying && carriedItem != null)
        {
            if (playerMovement != null && playerMovement.IsMoving)
            {
                // Hitung offset bobbing
                float speedFactor = playerMovement.currentSpeed / playerMovement.runSpeed;
                bobbingTimer += baseBobbingSpeed * speedFactor * Time.deltaTime;
                if (bobbingTimer > Mathf.PI * 2f)
                    bobbingTimer -= Mathf.PI * 2f;

                float offsetY = Mathf.Sin(bobbingTimer) * baseBobbingAmount;

                // Perbarui offset
                bobbingOffset = new Vector3(0, offsetY, 0);
            }
            else
            {
                // Smoothly kembalikan ke posisi nol saat berhenti
                bobbingOffset = Vector3.Lerp(bobbingOffset, Vector3.zero, Time.deltaTime * 8f);
            }

            // Terapkan offset ke posisi local
            carriedItem.transform.localPosition = bobbingOffset;
        }
    }

    public void PickUp(GameObject item)
    {
        if (item == null)
        {
            Debug.LogWarning("Item null saat mencoba pickup.");
            return;
        }

        if (IsCarrying)
            Drop();

        carriedItem = item;

        // Parenting dan posisi
        carriedItem.transform.SetParent(holdPoint);
        carriedItem.transform.localPosition = Vector3.zero;
        carriedItem.transform.localRotation = Quaternion.identity;

        // Matikan physics
        if (carriedItem.TryGetComponent<Rigidbody>(out var rb))
            rb.isKinematic = true;
    }

    public void Drop(bool removeFromInventory = true)  
    {
        if (!IsCarrying) return;

        // Ambil komponen Inventory dari player
        if (removeFromInventory && carriedItem.TryGetComponent<PickupItem>(out var pickup))
        {
            var inv = GetComponent<Inventory>();
            if (inv != null)
            {
                inv.RemoveItem(pickup.itemData, 1);
                FindFirstObjectByType<InventoryUI>()?.UpdateUI();
            }
        }

        carriedItem.transform.SetParent(null);
        carriedItem.transform.position = holdPoint.position;

        if (carriedItem.TryGetComponent<Rigidbody>(out var rb))
            rb.isKinematic = false;

        carriedItem = null;
    }

    public void BeginPlacementFromHand()
    {
        if (!IsCarrying) return;

        if (!carriedItem.TryGetComponent<PickupItem>(out var pickupItem) || pickupItem.itemData == null)
        {
            Debug.LogWarning("Item tidak memiliki PickupItem atau itemData.");
            return;
        }

        if (pickupItem.itemData.itemType != ItemType.Furniture)
        {
            Debug.Log("Item bukan Furniture, tidak bisa dipasang.");
            return;
        }

        carriedItem.transform.SetParent(null); // lepas dari holdPoint
        furniturePlacer.BeginPlacement(pickupItem.itemData, carriedItem);
        carriedItem = null;
    }
    /// <summary>
    /// Mengosongkan referensi carriedItem tanpa mengubah status GameObject (misalnya bila hanya di-hide).
    /// </summary>
    public void ClearCarriedItem()
    {
        carriedItem = null;
    }

    /// <summary>
    /// Mengatur carriedItem secara manual tanpa memanggil Drop() (misalnya saat item di-show kembali).
    /// </summary>
    public void SetCarriedItem(GameObject item)
    {
        carriedItem = item;
    }
}
