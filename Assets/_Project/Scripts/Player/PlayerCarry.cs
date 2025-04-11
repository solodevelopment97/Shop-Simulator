using UnityEngine;
using Placement;

[RequireComponent(typeof(FurniturePlacer))]
public class PlayerCarry : MonoBehaviour
{
    [SerializeField] private Transform holdPoint;
    [SerializeField] private Camera cam;

    private GameObject carriedItem;
    private Transform carryPosition;

    public bool IsCarrying => carriedItem != null;

    private void Start()
    {
        carryPosition = new GameObject("CarryPosition").transform;
        carryPosition.SetParent(transform);
        carryPosition.localPosition = new Vector3(0, 1.5f, 1); // Posisi relatif ke player
    }

    public void PickUp(GameObject item)
    {
        if (IsCarrying)
            Drop();

        carriedItem = item;
        carriedItem.transform.SetParent(holdPoint);
        carriedItem.transform.localPosition = Vector3.zero;
        carriedItem.transform.localRotation = Quaternion.identity;

        var rb = carriedItem.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;
    }

    public void Drop()
    {
        if (!IsCarrying) return;

        carriedItem.transform.SetParent(null);
        carriedItem.transform.position = holdPoint.position;

        var rb = carriedItem.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = false;

        carriedItem = null;
    }

    public void BeginPlacementFromHand()
    {
        if (!IsCarrying) return;

        var pickupItem = carriedItem.GetComponent<PickupItem>();
        if (pickupItem == null || pickupItem.itemData == null) return;

        if (pickupItem.itemData.itemType != ItemType.Furniture)
        {
            Debug.Log("Hanya item bertipe Furniture yang bisa dipasang.");
            return;
        }

        var placer = GetComponent<FurniturePlacer>();
        if (placer == null) return;

        // Lepas parent agar tidak nempel di kamera
        carriedItem.transform.SetParent(null);

        placer.BeginPlacement(pickupItem.itemData, carriedItem);
        carriedItem = null;
    }
}
