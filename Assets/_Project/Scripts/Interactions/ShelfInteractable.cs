using UnityEngine;
using ShopSimulator;
using Animations;
using Placement;
using NUnit.Framework.Interfaces;

[RequireComponent(typeof(Shelf))]
public class ShelfInteractable : MonoBehaviour, IInteractable, IPreviewable
{
    private Shelf shelf;
    private PlayerCarry carry;
    private Inventory inventory;

    [Header("Anim Settings")]
    [SerializeField] private float flyDuration = 0.5f;

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

        var pu = carry.HeldItem.GetComponent<PickupItem>();
        if (pu == null)
            return "Tidak ada item yang bisa diletakkan";

        return pu.itemData.itemType switch
        {
            ItemType.ShopItem =>
                $"[R] Letakkan 1x {pu.itemData.itemName}",

            ItemType.Box =>
                $"[R] Letakkan 1x {pu.itemData.itemName}",

            _ => "Item ini tidak bisa diletakkan di rak"
        };
    }

    /// <summary>
    /// Dipanggil sekali tiap tap R atau tiap tick BulkMode.
    /// </summary>
    public void Interact()
    {
        TryStoreOne();
    }

    private void TryStoreOne()
    {
        var go = carry.HeldItem;
        if (go == null) return;
        if (!go.TryGetComponent<PickupItem>(out var pu)) return;

        if (pu.itemData.itemType == ItemType.ShopItem)
        {
            StoreShopItem(pu);
        }
        else if (pu.itemData.itemType == ItemType.Box)
        {
            UnpackBox(pu);
        }
    }

    private void StoreShopItem(PickupItem pu)
    {
        var data = pu.itemData;

        Vector3 origin = carry.HoldPoint.position;
        Quaternion rot = carry.HoldPoint.rotation;
        GameObject ghost = Instantiate(data.prefab, origin, rot);

        if (ghost.TryGetComponent<Rigidbody>(out var rb)) rb.isKinematic = true;
        foreach (var c in ghost.GetComponentsInChildren<Collider>()) c.enabled = false;

        int slotIndex = shelf.GetNextFreeSlotIndex();
        if (slotIndex < 0)
        {
            Destroy(ghost);
            Debug.Log("Rak penuh!");
            return;
        }
        Vector3 targetPos = shelf.spawnPoints[slotIndex].position;

        ItemMover.AnimateFly(ghost, targetPos, flyDuration, () =>
        {
            // callback: spawn real instance dan place
            var real = ItemPoolManager.Instance.Spawn(data);
            real.GetComponent<PickupItem>().isReleased = false;

            if (!shelf.PlacePhysicalItem(real))
                ItemPoolManager.Instance.Despawn(data, real);
            else
            {
                shelf.AddStock(data, 1);
                inventory.RemoveItem(data, 1);
                FindFirstObjectByType<InventoryUI>()?.UpdateUI();
            }
        });

        // 4) lepas item di tangan & return original ke pool
        carry.ClearCarriedItem();
        if (!pu.isReleased)
        {
            ItemPoolManager.Instance.Despawn(data, pu.gameObject);
            pu.isReleased = true;
        }

        Debug.Log($"Meletakkan 1x {data.itemName} ke rak.");
    }

    private void UnpackBox(PickupItem box)
    {
        var data = box.itemData;
        if (box.interiorCount <= 0) return;

        // Spawn sub-item
        Vector3 origin = box.transform.position;
        Quaternion rot = box.transform.rotation;
        GameObject ghost = Instantiate(data.boxItems[0].prefab, origin, rot);

        if (ghost.TryGetComponent<Rigidbody>(out var rb)) rb.isKinematic = true;
        foreach (var c in ghost.GetComponentsInChildren<Collider>()) c.enabled = false;

        // 2) target pos
        int slotIndex = shelf.GetNextFreeSlotIndex();
        if (slotIndex < 0) { Destroy(ghost); return; }
        Vector3 targetPos = shelf.spawnPoints[slotIndex].position;

        // 3) animasi
        ItemMover.AnimateFly(ghost, targetPos, flyDuration, () =>
        {
            // spawn real sub-item via pool
            var real = ItemPoolManager.Instance.Spawn(data.boxItems[0]);
            real.GetComponent<PickupItem>().isReleased = false;

            if (!shelf.PlacePhysicalItem(real))
                ItemPoolManager.Instance.Despawn(data.boxItems[0], real);
            else
                shelf.AddStock(data.boxItems[0], 1);
        });

        // Kurangi interior, bukan jumlah kardus
        box.interiorCount--;

        // Update UI qty interior di inventory (mungkin tampil: "Box (isi 3/4)")
        inventory.UpdateBoxInterior(data, box.interiorCount);
        FindFirstObjectByType<InventoryUI>()?.UpdateUI();

        Debug.Log($"Unpack 1 unit, sisa isi kardus: {box.interiorCount}");

        if (box.interiorCount <= 0)
        {
            // Kardus masih utuh: clear carried tapi jangan remove slot
            // carry.ClearCarriedItem();
            Debug.Log("Kardus kosong, silakan drop atau isi ulang.");
        }
    }

    // Preview: pakai prefab ShopItem (untuk Box, subItem pertama)
    public GameObject GetPreviewPrefab()
    {
        var pu = carry.HeldItem?.GetComponent<PickupItem>();
        if (pu == null) return null;

        if (pu.interiorCount <= 0 && pu.itemData.itemType == ItemType.Box) return null;

        if (pu.itemData.itemType == ItemType.Box
            && pu.itemData.boxItems.Count > 0)
            return pu.itemData.boxItems[0].prefab;

        if (pu.itemData.itemType == ItemType.ShopItem)
            return pu.itemData.prefab;

        return null;
    }

    public (Vector3 position, Quaternion rotation)? GetPreviewTransform()
    {
        int slot = shelf.GetNextFreeSlotIndex();
        if (slot < 0) return null;
        var pt = shelf.spawnPoints[slot];
        return (pt.position, pt.rotation);
    }
    public bool CanStoreItem()
    {
        // misal shelf ini punya array slot
        return shelf.HasEmptySlot(); // true kalau ada slot kosong
    }

}
