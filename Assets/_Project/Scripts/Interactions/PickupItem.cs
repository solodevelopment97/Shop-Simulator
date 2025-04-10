using Placement;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PickupItem : MonoBehaviour, IInteractable
{
    public ItemData itemData;

    public void Interact()
    {
        if (FurniturePlacer.Instance != null && FurniturePlacer.Instance.IsPlacing)
        {
            Debug.Log("Sedang menempatkan furniture, tidak bisa ambil item lain.");
            return;
        }

        var player = GameObject.FindWithTag("Player");
        if (player == null) return;

        var carry = player.GetComponent<PlayerCarry>();
        if (carry == null) return;

        if (carry.IsCarrying)
        {
            Debug.Log("Tidak bisa ambil, sedang membawa barang.");
            return;
        }

        switch (itemData.itemType)
        {
            case ItemType.ShopItem:
                carry.PickUp(gameObject);
                break;

            case ItemType.Furniture:
                var placer = player.GetComponent<FurniturePlacer>();
                if (placer != null)
                {
                    placer.BeginPlacement(itemData, gameObject);
                }
                break;

            default:
                Debug.LogWarning("ItemType tidak dikenali.");
                break;
        }
    }

    public string GetInteractText()
    {
        return $"Ambil {itemData.itemName} (E)";
    }
}
