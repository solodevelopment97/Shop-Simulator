using UnityEngine;

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
        carryPosition.localPosition = new Vector3(0, 1.5f, 1); // Sesuaikan posisi sesuai kebutuhan
    }

    public void PickUp(GameObject item)
    {
        if (IsCarrying) Drop();

        carriedItem = item;
        carriedItem.transform.SetParent(holdPoint);
        carriedItem.transform.localPosition = Vector3.zero;
        carriedItem.transform.localRotation = Quaternion.identity;

        var rb = carriedItem.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
    }

    public void Drop()
    {
        if (!IsCarrying) return;

        // Lepas dari tangan
        carriedItem.transform.SetParent(null);

        // Posisi tetap di HoldPoint
        carriedItem.transform.position = holdPoint.position;

        // Aktifkan rigidbody agar jatuh
        var rb = carriedItem.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;

        carriedItem = null;
    }
}
