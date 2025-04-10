using UnityEngine;

public class PlayerCarry : MonoBehaviour
{
    [SerializeField] private Transform holdPoint;
    private GameObject carriedObject;
    private ItemData carriedItem;

    public bool IsCarrying => carriedObject != null;

    public void PickUp(GameObject obj, ItemData data)
    {
        if (IsCarrying) return;

        carriedObject = obj;
        carriedItem = data;

        obj.transform.SetParent(holdPoint);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;

        var rb = obj.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;
    }

    public void Drop()
    {
        if (!IsCarrying) return;

        var rb = carriedObject.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = false;

        carriedObject.transform.SetParent(null);
        carriedObject = null;
        carriedItem = null;
    }

    public ItemData GetCarriedItem()
    {
        return carriedItem;
    }
}
