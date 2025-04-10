using UnityEngine;
using Placement;

[RequireComponent(typeof(Collider))]
public class PickupItem : MonoBehaviour, IInteractable
{
    public ItemData itemData;

    public void Interact()
    {
        var player = GameObject.FindWithTag("Player");
        var carry = player.GetComponent<PlayerCarry>();

        if (carry.IsCarrying)
        {
            carry.Drop();
        }
        else
        {
            if (itemData.itemType == ItemType.Furniture)
            {
                // Spawn wireframe dan mulai placement
                var placer = player.GetComponent<FurniturePlacer>();
                if (placer != null)
                {
                    placer.BeginPlacement(itemData);
                }
            }
            else
            {
                carry.PickUp(gameObject, itemData);
            }
        }
    }

    public string GetInteractText()
    {
        return $"Ambil {itemData.itemName} (E)";
    }
}
