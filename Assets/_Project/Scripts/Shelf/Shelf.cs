using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Shelf : MonoBehaviour
{
    [Header("Child Transforms as visual slots")]
    [SerializeField] private List<Transform> spawnPoints = new();

    [Header("Capacity per item")]
    [SerializeField] private int capacityPerItem = 20;

    // Stock data
    private readonly Dictionary<ItemData, int> stock = new();

    // Tracks occupied visual slots
    private bool[] occupied;

    private void Awake()
    {
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogError("Spawn points are not assigned or empty!");
            return;
        }

        occupied = new bool[spawnPoints.Count];
    }
    /// <summary>
    /// Provides read-only access to spawn points.
    /// </summary>
    public IReadOnlyList<Transform> SpawnPoints => spawnPoints;
    /// <summary>
    /// Gets the current stock quantity of a specific item.
    /// </summary>
    public int GetStock(ItemData item) =>
        stock.TryGetValue(item, out var quantity) ? quantity : 0;

    /// <summary>
    /// Adds stock for a specific item, respecting the capacity limit.
    /// </summary>
    public int AddStock(ItemData item, int quantity)
    {
        if (item == null) return 0;

        int currentStock = GetStock(item);
        int canAdd = Mathf.Min(quantity, capacityPerItem - currentStock);

        if (canAdd > 0)
        {
            stock[item] = currentStock + canAdd;
        }

        return canAdd;
    }

    /// <summary>
    /// Removes stock for a specific item.
    /// </summary>
    public int RemoveStock(ItemData item, int quantity)
    {
        if (item == null || !stock.TryGetValue(item, out var currentStock)) return 0;

        int toRemove = Mathf.Min(quantity, currentStock);
        currentStock -= toRemove;

        if (currentStock <= 0)
        {
            stock.Remove(item);
        }
        else
        {
            stock[item] = currentStock;
        }

        return toRemove;
    }

    /// <summary>
    /// Places a physical item in the first available visual slot.
    /// </summary>
    public bool PlacePhysicalItem(GameObject item)
    {
        if (item == null) return false;

        for (int i = 0; i < spawnPoints.Count; i++)
        {
            if (!occupied[i])
            {
                occupied[i] = true;
                AttachItemToSlot(item, spawnPoints[i]);
                return true;
            }
        }

        Debug.LogWarning("No free slots available to place the item.");
        return false;
    }

    /// <summary>
    /// Removes a physical item from its visual slot and frees the slot.
    /// </summary>
    public bool RemovePhysicalItem(GameObject item)
    {
        if (item == null) return false;

        Transform parent = item.transform.parent;
        int index = spawnPoints.IndexOf(parent);

        if (index >= 0 && occupied[index])
        {
            occupied[index] = false;
            DetachItemFromSlot(item);
            return true;
        }

        Debug.LogWarning("Item is not in a valid slot.");
        return false;
    }

    /// <summary>
    /// Gets the index of the first free slot, or -1 if all slots are occupied.
    /// </summary>
    public int GetNextFreeSlotIndex()
    {
        for (int i = 0; i < occupied.Length; i++)
        {
            if (!occupied[i]) return i;
        }

        return -1; // No free slots
    }

    /// <summary>
    /// Checks if there is at least one empty slot available.
    /// </summary>
    public bool HasEmptySlot() => System.Array.Exists(occupied, slot => !slot);

    /// <summary>
    /// Attaches an item to a specific slot.
    /// </summary>
    private void AttachItemToSlot(GameObject item, Transform slot)
    {
        item.transform.SetParent(slot, false);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;

        if (item.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true;
        }
    }

    /// <summary>
    /// Detaches an item from its slot.
    /// </summary>
    private void DetachItemFromSlot(GameObject item)
    {
        item.transform.SetParent(null);

        if (item.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = false;
        }
    }
}
