using UnityEngine;
using Placement;

[RequireComponent(typeof(FurniturePlacer))]
public class PlayerCarry : MonoBehaviour
{
    [Header("Carry Settings")]
    [SerializeField] private Transform holdPoint;
    [SerializeField] private Camera cam;

    private GameObject carriedItem;
    private FurniturePlacer furniturePlacer;

    public bool IsCarrying => carriedItem != null;

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
}
