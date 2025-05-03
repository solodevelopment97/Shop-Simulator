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

    // For Box:
    public int cardboardCount = 0; // Number of boxes carried
    public int interiorCount = 0;  // Number of items in the last box

    [Header("Pickup Animation")]
    [Tooltip("Duration of the animation flying to the hand")]
    public float pickupTweenDuration = 0.4f;

    private static GameObject cachedPlayer;
    private static PlayerCarry cachedCarry;
    private static FurniturePlacer cachedPlacer;
    private static Inventory cachedInventory;

    private void Awake()
    {
        if (itemData?.itemType == ItemType.Box)
        {
            // Initialize box & contents based on inventory slot
            interiorCount = itemData.boxQuantities.Sum(); // Total contents per box
        }
    }

    public void Interact()
    {
        if (IsPlacementModeActive())
        {
            Debug.Log("Currently in placement mode. Cannot pick up item.");
            return;
        }

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

    private bool IsPlacementModeActive()
    {
        return FurniturePlacer.Instance != null && FurniturePlacer.Instance.IsPlacing;
    }

    private void HandleShopItemPickup(PlayerCarry playerCarry, Inventory inventory)
    {
        if (TryRemoveFromShelf())
        {
            SpawnGhostAndAnimate(playerCarry, inventory, () =>
            {
                if (inventory.AddItem(itemData, quantity))
                    UpdateInventoryUI();
            });
        }
    }

    private void HandleBoxPickup(PlayerCarry playerCarry, Inventory inventory)
    {
        SpawnGhostAndAnimate(playerCarry, inventory, () =>
        {
            if (inventory.AddBox(itemData, quantity, 1))
                UpdateInventoryUI();
        });
    }

    private bool TryRemoveFromShelf()
    {
        var shelf = GetComponentInParent<Shelf>();
        if (shelf != null)
        {
            shelf.RemovePhysicalItem(gameObject);
            int removed = shelf.RemoveStock(itemData, quantity);
            if (removed <= 0)
            {
                Debug.LogWarning("Stock on the shelf is empty!");
                return false;
            }
        }
        return true;
    }

    private void SpawnGhostAndAnimate(PlayerCarry playerCarry, Inventory inventory, Action onComplete)
    {
        if (!isReleased)
        {
            DespawnItem();
        }

        var ghost = Instantiate(itemData.prefab, transform.position, transform.rotation);
        MakeGhost(ghost);

        PickupMover.AnimateToTarget(ghost, playerCarry.HoldPoint, pickupTweenDuration, () =>
        {
            onComplete?.Invoke();
            if (!isReleased)
            {
                DespawnItem();
            }
        });
    }

    private void DespawnItem()
    {
        ItemPoolManager.Instance?.Despawn(itemData, gameObject);
        isReleased = true;
    }

    private void MakeGhost(GameObject ghost)
    {
        if (ghost.TryGetComponent<Rigidbody>(out var rb)) rb.isKinematic = true;
        foreach (var collider in ghost.GetComponentsInChildren<Collider>()) collider.enabled = false;
        if (ghost.TryGetComponent<Outline>(out var outline)) outline.enabled = false;
    }

    public string GetInteractText()
    {
        return $"Pick up {itemData.itemName} (E)";
    }

    private bool TryGetPlayerComponents(out PlayerCarry carry, out FurniturePlacer placer, out Inventory inventory)
    {
        carry = GetCarry();
        placer = GetPlacer();
        inventory = GetInventory();

        if (carry == null) { Debug.LogError("PlayerCarry not found."); return false; }
        if (placer == null) { Debug.LogError("FurniturePlacer not found."); return false; }
        if (inventory == null) { Debug.LogError("Inventory not found."); return false; }

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

    private void UpdateInventoryUI()
    {
        FindFirstObjectByType<InventoryUI>()?.UpdateUI();
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
