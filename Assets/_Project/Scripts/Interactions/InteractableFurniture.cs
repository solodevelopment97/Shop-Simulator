// Assets/Scripts/Interaction/InteractableFurniture.cs
using UnityEngine;
using Placement;

public class InteractableFurniture : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData itemData;

    public void Interact()
    {
        // Sembunyikan dari world
        gameObject.SetActive(false);

        // Kirim ke FurniturePlacer
        FurniturePlacer.Instance.BeginPlacement(itemData, this);
    }

    public void Restore()
    {
        gameObject.SetActive(true);
    }

    public string GetInteractText()
    {
        return $"Ambil {itemData.itemName} (E)";
    }
}
