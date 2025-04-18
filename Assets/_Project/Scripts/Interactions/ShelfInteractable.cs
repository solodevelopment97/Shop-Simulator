using UnityEngine;
using ShopSimulator;
using Placement;

[RequireComponent(typeof(Shelf))]
public class ShelfInteractable : MonoBehaviour, IInteractable
{
    private Shelf shelf;
    private PlayerCarry carry;
    private Inventory inventory;

    private void Awake()
    {
        shelf = GetComponent<Shelf>();
        carry = FindFirstObjectByType<PlayerCarry>();
        inventory = FindFirstObjectByType<Inventory>();
    }

    /// <summary>
    /// Meletakkan satu unit dari kardus ke rak.
    /// </summary>
    public bool PlaceOneUnit()
    {
        if (!carry.IsCarrying) return false;
        if (!carry.HeldItem.TryGetComponent<PickupItem>(out var box)
            || box.itemData.itemType != ItemType.ShopItem)
            return false;

        var data = box.itemData;
        // coba stok data dulu
        int added = shelf.AddStock(data, 1);
        if (added <= 0) return false;

        // kurangi inventory data & UI
        inventory.RemoveItem(data, 1);
        FindFirstObjectByType<InventoryUI>()?.UpdateUI();

        // spawn produk fisik
        var prod = Instantiate(data.prefab);
        if (!shelf.PlacePhysicalItem(prod))
        {
            Destroy(prod);
            return false;
        }

        // kurangi isi kardus, drop kardus jika habis
        box.quantity -= 1;
        if (box.quantity <= 0)
            carry.Drop();
        else
            carry.SetCarriedItem(carry.HeldItem);

        return true;
    }

    public string GetInteractText()
    {
        if (carry.IsCarrying && carry.HeldItem.TryGetComponent<PickupItem>(out var pi) &&
       pi.itemData.itemType == ItemType.ShopItem)
        {
            return PlayerInteraction.BulkMode
                ? $"Hold E: Bulk letakkan {pi.itemData.itemName}"
                : $"E: Letakkan 1x {pi.itemData.itemName}";
        }
        else
        {
            return "Tidak ada item yang bisa diletakkan";
        }
    }

    public void Interact()
    {
        if (!carry.IsCarrying) return;
        if (!carry.HeldItem.TryGetComponent<PickupItem>(out var box)
            || box.itemData.itemType != ItemType.ShopItem)
        {
            Debug.Log("Hanya kardus (ShopItem) yang bisa diletakkan.");
            return;
        }
        if (!PlaceOneUnit())
            Debug.Log("Rak penuh atau kardus habis.");

        var itemData = box.itemData;
        int inBox = box.quantity;
        bool bulk = PlayerInteraction.BulkMode;
        int toPlace = bulk ? inBox : 1;

        int added = shelf.AddStock(itemData, toPlace);
        if (added <= 0)
        {
            Debug.Log("Rak penuh untuk item ini!");
            return;
        }

        // Kurangi inventory data & UI
        inventory.RemoveItem(itemData, added);
        FindFirstObjectByType<InventoryUI>()?.UpdateUI();

        // --- Physical placement produk, bukan kardus ---
        for (int i = 0; i < added; i++)
        {
            // instantiate produk (bukan box)
            var prod = Instantiate(itemData.prefab);
            if (!shelf.PlacePhysicalItem(prod))
            {
                Destroy(prod);
                Debug.LogWarning("Slot rak fisik penuh saat menambah produk.");
                break;
            }
        }

        // Kurangi isi kardus
        box.quantity -= added;

        // Kalau kardus sekarang kosong, buang kardus dari tangan & inventory
        if (box.quantity <= 0)
        {
            carry.Drop();  // kardus jatuh/terbuang
        }
        else
        {
            // kalau masih sisa, pastikan boxGO tetap di tangan
            carry.SetCarriedItem(carry.HeldItem);
        }

        Debug.Log($"Meletakkan {added} unit {itemData.itemName} ke rak.");
    }
}
