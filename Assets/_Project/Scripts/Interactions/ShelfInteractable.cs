using UnityEngine;
using ShopSimulator;
using Placement;

[RequireComponent(typeof(Shelf))]
public class ShelfInteractable : MonoBehaviour, IInteractable, IPreviewable
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

    public string GetInteractText()
    {
        if (!carry.IsCarrying)
            return "Tidak ada item yang bisa diletakkan";

        if (!carry.HeldItem.TryGetComponent<PickupItem>(out var box))
            return "Tidak ada item yang bisa diletakkan";

        switch (box.itemData.itemType)
        {
            case ItemType.ShopItem:
                return PlayerInteraction.BulkMode
                    ? $"Hold E: Bulk letakkan {box.itemData.itemName}"
                    : $"E: Letakkan 1x {box.itemData.itemName}";

            // if you add ItemType.Box later, handle here...

            default:
                return "Item ini tidak bisa diletakkan di rak";
        }
    }

    /// <summary>
    /// Meletakkan 1 unit produk dari kardus ke rak.
    /// Mengembalikan true kalau benar-benar berhasil.
    /// </summary>
    public bool PlaceOneUnit()
    {
        if (!carry.IsCarrying) return false;
        var go = carry.HeldItem;
        if (go == null) return false;
        if (!go.TryGetComponent<PickupItem>(out var pickup)) return false;
        if (pickup.itemData.itemType != ItemType.ShopItem) return false;

        var data = pickup.itemData;

        // 1) Coba spawn fisik dulu
        var prod = Instantiate(data.prefab);
        if (!shelf.PlacePhysicalItem(prod))
        {
            Destroy(prod);
            Debug.Log("Tidak ada slot visual tersisa di rak.");
            return false;
        }

        // 2) Stok data sekarang bertambah
        shelf.AddStock(data, 1);

        // 3) Update inventory & UI
        inventory.RemoveItem(data, 1);
        FindFirstObjectByType<InventoryUI>()?.UpdateUI();

        // 4) Hancurkan item di tangan (ShopItem satuan)
        carry.ClearCarriedItem();
        Destroy(go);

        return true;
    }

    /// <summary>
    /// Dipanggil saat E ditekan (tap) atau (bulk) hold dilepas.
    /// Tidak double‑dip: hanya memanggil PlaceOneUnit di loop kecil.
    /// </summary>
    public void Interact()
    {
        if (!carry.IsCarrying || !carry.HeldItem.TryGetComponent<PickupItem>(out var pickup))
        {
            Debug.Log("Tidak ada item yang bisa diletakkan.");
            return;
        }

        int tries = PlayerInteraction.BulkMode ? 1 : 1; // untuk ShopItem kita hanya 1 per tap
        int placed = 0;
        for (int i = 0; i < tries; i++)
        {
            if (PlaceOneUnit()) placed++;
            else break;
        }

        if (placed > 0)
            Debug.Log($"Meletakkan {placed} unit {pickup.itemData.itemName} ke rak.");
        else
            Debug.Log("Gagal meletakkan (rak penuh atau slot tidak cukup).");
    }

    public GameObject GetPreviewPrefab()
    {
        // Hanya preview kalau sedang pegang ShopItem
        var pi = FindFirstObjectByType<PlayerCarry>()?.HeldItem?
                     .GetComponent<PickupItem>();
        if (pi != null && pi.itemData.itemType == ItemType.ShopItem)
            return pi.itemData.prefab;
        return null;
    }

    public (Vector3 position, Quaternion rotation)? GetPreviewTransform()
    {
        // hanya reaktif kalau prefab valid
        var prefab = GetPreviewPrefab();
        if (prefab == null) return null;

        // cari next slot bebas
        int slot = GetComponent<Shelf>().GetNextFreeSlotIndex();
        if (slot < 0) return null;

        var pt = GetComponent<Shelf>().spawnPoints[slot];
        return (pt.position, pt.rotation);
    }
}
