using Placement;
using ShopSimulator;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PickupItem : MonoBehaviour, IInteractable
{
    public ItemData itemData;
    public int quantity = 1;

    private static GameObject cachedPlayer;
    private static PlayerCarry cachedCarry;
    private static FurniturePlacer cachedPlacer;
    private static Inventory cachedInventory;

    public void Interact()
    {
        // Jika sedang mode placement, batalkan
        if (FurniturePlacer.Instance != null && FurniturePlacer.Instance.IsPlacing)
        {
            Debug.Log("Sedang dalam mode placement. Tidak bisa mengambil item.");
            return;
        }

        // Ambil komponen PlayerCarry, FurniturePlacer, Inventory
        if (!TryGetPlayerComponents(out var carry, out var placer, out var inventory))
            return;

        switch (itemData.itemType)
        {
            case ItemType.ShopItem:
                // Coba cari Shelf di parent
                var shelf = GetComponentInParent<Shelf>();
                if (shelf != null)
                {
                    shelf.RemovePhysicalItem(gameObject);
                    int removed = shelf.RemoveStock(itemData, quantity);
                    if (removed <= 0)
                    {
                        Debug.LogWarning("Stok di rak sudah habis!");
                        return;
                    }
                }
                // Tambah ke inventory
                if (inventory.AddItem(itemData, quantity))
                {
                    Debug.Log($"Item {itemData.itemName} ditambahkan ke inventory.");
                    Destroy(gameObject); // hanya menghancurkan produk rak
                    FindFirstObjectByType<InventoryUI>()?.UpdateUI();
                }
                else if (shelf != null)
                {
                    // rollback stok kalau inventory penuh
                    shelf.AddStock(itemData, quantity);
                    Debug.Log("Inventory penuh, gagal mengambil dari rak.");
                }
                break;
        }
    }

    public string GetInteractText()
    {
        return $"Ambil {itemData.itemName} (E)";
    }

    private bool TryGetPlayerComponents(out PlayerCarry carry, out FurniturePlacer placer, out Inventory inventory)
    {
        carry = GetCarry();
        placer = GetPlacer();
        inventory = GetInventory();

        if (carry == null) { Debug.LogError("PlayerCarry tidak ditemukan."); return false; }
        if (placer == null) { Debug.LogError("FurniturePlacer tidak ditemukan."); return false; }
        if (inventory == null) { Debug.LogError("Inventory tidak ditemukan."); return false; }

        return true;
    }

    private static GameObject GetPlayer()
    {
        if (cachedPlayer == null)
            cachedPlayer = GameObject.FindWithTag("Player");
        return cachedPlayer;
    }

    private static PlayerCarry GetCarry()
    {
        if (cachedCarry == null)
            cachedCarry = GetPlayer()?.GetComponent<PlayerCarry>();
        return cachedCarry;
    }

    private static FurniturePlacer GetPlacer()
    {
        if (cachedPlacer == null)
            cachedPlacer = GetPlayer()?.GetComponent<FurniturePlacer>();
        return cachedPlacer;
    }

    private static Inventory GetInventory()
    {
        if (cachedInventory == null)
            cachedInventory = GetPlayer()?.GetComponent<Inventory>();
        return cachedInventory;
    }

    [RuntimeInitializeOnLoadMethod]
    private static void ResetCacheOnSceneLoad()
    {
        cachedPlayer = null;
        cachedCarry = null;
        cachedPlacer = null;
        cachedInventory = null;
    }
}
