using System.Collections.Generic;
using UnityEngine;
using ShopSimulator;

[RequireComponent(typeof(Collider))]
public class Shelf : MonoBehaviour
{
    [Header("Spawn Points (child Transforms)")]
    [Tooltip("Drag & drop child Transforms di Inspector")]
    public List<Transform> spawnPoints = new List<Transform>();

    [Header("Capacity per Item")]
    public int capacityPerItem = 20;

    private Dictionary<ItemData, int> stock = new Dictionary<ItemData, int>();
    private int nextSpawnIndex = 0;

    public int GetStock(ItemData item) =>
        stock.TryGetValue(item, out var q) ? q : 0;

    public int AddStock(ItemData item, int quantity)
    {
        int current = GetStock(item);
        int canAdd = Mathf.Min(quantity, capacityPerItem - current);
        if (canAdd <= 0) return 0;
        stock[item] = current + canAdd;
        return canAdd;
    }

    /// <summary>
    /// Pindahkan GameObject itemGO ke spawnPoint berikutnya.
    /// </summary>
    public bool PlacePhysicalItem(GameObject itemGO)
    {
        if (nextSpawnIndex >= spawnPoints.Count)
            return false;

        var slot = spawnPoints[nextSpawnIndex++];
        itemGO.transform.SetParent(slot, worldPositionStays: false);
        itemGO.transform.localPosition = Vector3.zero;
        itemGO.transform.localRotation = Quaternion.identity;

        if (itemGO.TryGetComponent<Rigidbody>(out var rb))
            rb.isKinematic = true;

        return true;
    }
}
