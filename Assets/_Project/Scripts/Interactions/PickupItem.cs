using System;
using System.Linq;
using Animations;
using Placement;
using QuickOutline;
using ShopSimulator;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(ShopItemMarker))]
[RequireComponent(typeof(Outline))]

public class PickupItem : MonoBehaviour, IInteractable
{
    [NonSerialized] public bool isReleased = false;

    public ItemData itemData;
    public int quantity = 1;
    // Untuk Box:
    public int cardboardCount = 0;    // berapa kardus yang dibawa
    public int interiorCount = 0;     // berapa item di dalam kardus terakhir

    [Header("Pickup Animation")]
    [Tooltip("Durasi animasi terbang ke tangan")]
    public float pickupTweenDuration = 0.4f;

    private static GameObject cachedPlayer;
    private static PlayerCarry cachedCarry;
    private static FurniturePlacer cachedPlacer;
    private static Inventory cachedInventory;

    private void Awake()
    { 
        if (itemData.itemType == ItemType.Box)
        {
            // inisialisasi kardus & isi berdasarkan inventory slot
            interiorCount = itemData.boxQuantities.Sum(); // total isi per kardus
        }
    }
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
                HandleShopItemPickup(carry, inventory);
                break;
            case ItemType.Box:
                HandleBoxPickup(carry, inventory);
                break;
        }
    }

    private void HandleShopItemPickup(PlayerCarry playerCarry, Inventory inventory)
    {
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
        // 1) sembunyikan / despawn objek asli segera
        if (!isReleased)
        {
            ItemPoolManager.Instance.Despawn(itemData, gameObject);
            isReleased = true;
        }       
        // 2) spawn ghost visual di posisi asli
        var ghost = Instantiate(itemData.prefab, transform.position, transform.rotation);
        MakeGhost(ghost);

        PickupMover.AnimateToTarget(ghost, playerCarry.HoldPoint, pickupTweenDuration, () =>
        {
            if (inventory.AddItem(itemData, quantity))
                FindFirstObjectByType<InventoryUI>()?.UpdateUI();

            // despawn real pooled object
            if (!isReleased)
            {
                ItemPoolManager.Instance.Despawn(itemData, gameObject);
                isReleased = true;
            }
        });

    }

    private void HandleBoxPickup(PlayerCarry playerCarry, Inventory inventory)
    {
        int remaining = quantity;

        if (!isReleased)
        {
            ItemPoolManager.Instance.Despawn(itemData, gameObject);
            isReleased = true;
        }
        // 2) spawn ghost visual di posisi asli
        var ghost = Instantiate(itemData.prefab, transform.position, transform.rotation);
        MakeGhost(ghost);
        PickupMover.AnimateToTarget(ghost, playerCarry.HoldPoint, pickupTweenDuration, () =>
        {
            if (inventory.AddBox(itemData, remaining, 1))
                    FindFirstObjectByType<InventoryUI>()?.UpdateUI();

            // despawn real pooled object
            if (!isReleased)
            {
                ItemPoolManager.Instance.Despawn(itemData, gameObject);
                isReleased = true;
            }
        });
    }

    private void MakeGhost(GameObject ghost)
    {
        if (ghost.TryGetComponent<Rigidbody>(out var rb)) rb.isKinematic = true;
        foreach (var c in ghost.GetComponentsInChildren<Collider>()) c.enabled = false;
        // disable Outline on ghost
        if (ghost.TryGetComponent<Outline>(out var o)) o.enabled = false;
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
