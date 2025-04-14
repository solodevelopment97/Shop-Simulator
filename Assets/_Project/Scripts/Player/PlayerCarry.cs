using UnityEngine;
using Placement;
using ShopSimulator;

[RequireComponent(typeof(FurniturePlacer))]
public class PlayerCarry : MonoBehaviour
{
    [Header("Carry Settings")]
    [SerializeField] private Transform holdPoint;
    [SerializeField] private Camera cam;


    private GameObject carriedItem;
    private FurniturePlacer furniturePlacer;

    public bool IsCarrying => carriedItem != null;
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

        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null)
                Debug.LogError("Kamera tidak ditemukan. Pastikan memiliki MainCamera di scene.");
        }

        furniturePlacer = GetComponent<FurniturePlacer>();
        if (furniturePlacer == null)
            Debug.LogError("FurniturePlacer tidak ditemukan.");
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

    public void Drop()
    {
        if (!IsCarrying) return;

        // Ambil komponen Inventory dari player
        Inventory inv = GetComponent<Inventory>();
        if (inv != null && carriedItem != null && carriedItem.TryGetComponent<PickupItem>(out var pickup))
        {
            // Mengurangi inventory sebanyak 1 unit dari item yang di-drop
            inv.RemoveItem(pickup.itemData, 1);
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
