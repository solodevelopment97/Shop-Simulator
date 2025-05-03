using UnityEngine;
using ShopSimulator;
using Animations;
using Placement;

[RequireComponent(typeof(Shelf))]
public class ShelfInteractable : MonoBehaviour, IInteractable, IPreviewable
{
    private Shelf shelf;
    private PlayerCarry carry;
    private Inventory inventory;

    [Header("Animation Settings")]
    [SerializeField] private float flyDuration = 0.5f;

    private bool isAnimating = false;

    public bool IsAnimating => isAnimating;

    private void Awake()
    {
        shelf = GetComponent<Shelf>();
        carry = FindFirstObjectByType<PlayerCarry>();
        inventory = FindFirstObjectByType<Inventory>();

        if (shelf == null || carry == null || inventory == null)
        {
            Debug.LogError("ShelfInteractable dependencies are not properly initialized.");
        }
    }

    public string GetInteractText()
    {
        if (!carry.IsCarrying)
            return "Tidak ada item yang bisa diletakkan";

        var pu = carry.HeldItem?.GetComponent<PickupItem>();
        if (pu == null)
            return "Tidak ada item yang bisa diletakkan";

        return pu.itemData.itemType switch
        {
            ItemType.ShopItem => $"[R] Letakkan 1x {pu.itemData.itemName}",
            ItemType.Box => $"[R] Letakkan 1x {pu.itemData.itemName}",
            _ => "Item ini tidak bisa diletakkan di rak"
        };
    }

    public void Interact()
    {
        TryStoreOne();
    }

    private void TryStoreOne()
    {
        var heldItem = carry.HeldItem;
        if (heldItem == null) return;

        if (heldItem.TryGetComponent<PickupItem>(out var pu))
        {
            switch (pu.itemData.itemType)
            {
                case ItemType.ShopItem:
                    HandleItemPlacement(pu, shelf.GetNextFreeSlotIndex(), () => StoreShopItem(pu));
                    break;

                case ItemType.Box:
                    UnpackBox(pu);
                    break;

                default:
                    Debug.LogWarning("Item type not supported for placement.");
                    break;
            }
        }
    }

    private void HandleItemPlacement(PickupItem pu, int slotIndex, System.Action onPlacementComplete)
    {
        if (isAnimating || slotIndex < 0)
        {
            Debug.LogWarning(slotIndex < 0 ? "No free slots available." : "Animation in progress.");
            return;
        }

        isAnimating = true;

        Vector3 origin = carry.HoldPoint.position;
        Quaternion rotation = carry.HoldPoint.rotation;
        GameObject ghost = Instantiate(pu.itemData.prefab, origin, rotation);

        PrepareGhostItem(ghost);

        Vector3 targetPos = shelf.SpawnPoints[slotIndex].position;

        ItemMover.AnimateFly(ghost, targetPos, flyDuration, () =>
        {
            onPlacementComplete?.Invoke();
            isAnimating = false;
        });

        carry.ClearCarriedItem();
        if (!pu.isReleased)
        {
            ItemPoolManager.Instance.Despawn(pu.itemData, pu.gameObject);
            pu.isReleased = true;
        }
    }

    private void StoreShopItem(PickupItem pu)
    {
        var data = pu.itemData;

        var realItem = ItemPoolManager.Instance.Spawn(data);
        realItem.GetComponent<PickupItem>().isReleased = false;

        if (!shelf.PlacePhysicalItem(realItem))
        {
            ItemPoolManager.Instance.Despawn(data, realItem);
            Debug.LogWarning("Failed to place item on the shelf.");
        }
        else
        {
            shelf.AddStock(data, 1);
            inventory.RemoveItem(data, 1);
            FindFirstObjectByType<InventoryUI>()?.UpdateUI();
            Debug.Log($"Placed 1x {data.itemName} on the shelf.");
        }
    }

    private void UnpackBox(PickupItem box)
    {
        if (isAnimating) return; // Blok jika animasi sedang berlangsung
        if (box.interiorCount <= 0)
        {
            Debug.LogWarning("Box is empty.");
            return;
        }

        isAnimating = true;

        // Ambil data sub-item pertama dari box
        var subItemData = box.itemData.boxItems[0];
        if (subItemData == null)
        {
            Debug.LogError("Box does not contain valid sub-items.");
            isAnimating = false;
            return;
        }

        // Buat ghost item untuk animasi
        Vector3 origin = carry.HoldPoint.position;
        Quaternion rotation = carry.HoldPoint.rotation;
        GameObject ghost = Instantiate(subItemData.prefab, origin, rotation);

        PrepareGhostItem(ghost);

        // Cari slot kosong di rak
        int slotIndex = shelf.GetNextFreeSlotIndex();
        if (slotIndex < 0)
        {
            Destroy(ghost);
            Debug.LogWarning("No free slots available on the shelf.");
            isAnimating = false;
            return;
        }

        Vector3 targetPos = shelf.SpawnPoints[slotIndex].position;

        // Animasi ghost item ke rak
        ItemMover.AnimateFly(ghost, targetPos, flyDuration, () =>
        {
            // Spawn item nyata dari pool
            var realItem = ItemPoolManager.Instance.Spawn(subItemData);
            realItem.GetComponent<PickupItem>().isReleased = false;

            // Tempatkan item di rak
            if (!shelf.PlacePhysicalItem(realItem))
            {
                ItemPoolManager.Instance.Despawn(subItemData, realItem);
                Debug.LogWarning("Failed to place unpacked item on the shelf.");
            }
            else
            {
                shelf.AddStock(subItemData, 1);
                box.interiorCount--;
                inventory.UpdateBoxInterior(box.itemData, box.interiorCount);
                FindFirstObjectByType<InventoryUI>()?.UpdateUI();
                Debug.Log($"Unpacked 1 item. Remaining in box: {box.interiorCount}");
            }

            isAnimating = false;
        });

        // Jika box kosong, beri peringatan
        if (box.interiorCount <= 0)
        {
            Debug.Log("Box is empty. Please drop or refill it.");
        }
    }

    private void PrepareGhostItem(GameObject ghost)
    {
        if (ghost.TryGetComponent<Rigidbody>(out var rb)) rb.isKinematic = true;
        foreach (var collider in ghost.GetComponentsInChildren<Collider>()) collider.enabled = false;
    }

    public GameObject GetPreviewPrefab()
    {
        var pu = carry.HeldItem?.GetComponent<PickupItem>();
        if (pu == null || (pu.itemData.itemType == ItemType.Box && pu.interiorCount <= 0))
            return null;

        return pu.itemData.itemType == ItemType.Box && pu.itemData.boxItems.Count > 0
            ? pu.itemData.boxItems[0].prefab
            : pu.itemData.prefab;
    }

    public (Vector3 position, Quaternion rotation)? GetPreviewTransform()
    {
        int slot = shelf.GetNextFreeSlotIndex();
        if (slot < 0) return null;

        var point = shelf.SpawnPoints[slot];
        return (point.position, point.rotation);
    }

    public bool CanStoreItem()
    {
        return shelf.HasEmptySlot() && !isAnimating;
    }
}
