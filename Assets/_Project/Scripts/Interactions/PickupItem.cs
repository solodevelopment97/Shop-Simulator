// Scripts/Interactions/PickupItem.cs
using UnityEngine;
using Placement;

[RequireComponent(typeof(Collider))]
public class PickupItem : MonoBehaviour, IInteractable
{
    public ItemData itemData;

    public void Interact()
    {
        if (itemData.itemType == ItemType.Furniture)
        {
            FurniturePlacer.Instance.BeginPlacement(itemData, gameObject);
            gameObject.SetActive(false);
        }
        else
        {
            Debug.Log($"[ShopItem] Diambil: {itemData.itemName}");
            Destroy(gameObject); // sementara, nanti masuk inventory
        }
    }

    public string GetInteractText()
    {
        return $"Ambil {itemData.itemName} (E)";
    }
}
