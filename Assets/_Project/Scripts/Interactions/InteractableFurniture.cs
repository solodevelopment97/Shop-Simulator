using UnityEngine;
using Placement;

[RequireComponent(typeof(Collider))]
public class InteractableFurniture : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData itemData;

    public void Interact()
    {
        if (itemData == null)
        {
            Debug.LogWarning("ItemData belum di-assign pada InteractableFurniture.");
            return;
        }

        if (FurniturePlacer.Instance == null)
        {
            Debug.LogError("FurniturePlacer.Instance tidak ditemukan.");
            return;
        }
        if (FurniturePlacer.Instance.IsPlacing)
        {
            Debug.Log("Sedang dalam mode placement. Tidak bisa ambil furniture lain.");
            return;
        }
        // Nonaktifkan objek dari dunia
        gameObject.SetActive(false);

        // Kirim data ke sistem FurniturePlacer
        FurniturePlacer.Instance.BeginPlacement(itemData, gameObject);
    }

    public void Restore()
    {
        gameObject.SetActive(true);
    }

    public string GetInteractText()
    {
        return itemData != null
            ? $"Ambil {itemData.itemName}"
            : "Ambil Furniture";
    }
}
