using System.Collections.Generic;
using UnityEngine;
using ShopSimulator;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

[RequireComponent(typeof(Collider))]
public class Shelf : MonoBehaviour
{
    [Header("Child Transforms sebagai slot visual")]
    public List<Transform> spawnPoints = new();

    [Header("Kapasitas data per item")]
    public int capacityPerItem = 20;

    // stok data
    private Dictionary<ItemData, int> stock = new();

    // track slot visual yang terpakai
    private bool[] occupied;

    private void Awake()
    {
        occupied = new bool[spawnPoints.Count];
    }

    public int GetStock(ItemData item) =>
        stock.TryGetValue(item, out var q) ? q : 0;

    public int AddStock(ItemData item, int qty)
    {
        int cur = GetStock(item);
        int can = Mathf.Min(qty, capacityPerItem - cur);
        if (can <= 0) return 0;
        stock[item] = cur + can;
        return can;
    }

    public int RemoveStock(ItemData item, int qty)
    {
        if (!stock.TryGetValue(item, out var cur)) return 0;
        int rem = Mathf.Min(qty, cur);
        cur -= rem;
        if (cur <= 0) stock.Remove(item);
        else stock[item] = cur;
        return rem;
    }

    /// <summary>
    /// Tempatkan produk di slot visual pertama yang free.
    /// </summary>
    public bool PlacePhysicalItem(GameObject go)
    {
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            if (!occupied[i])
            {
                occupied[i] = true;
                go.transform.SetParent(spawnPoints[i], false);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                if (go.TryGetComponent<Rigidbody>(out var rb))
                    rb.isKinematic = true;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Lepaskan produk dari slot visual, free slot‑nya kembali.
    /// </summary>
    public bool RemovePhysicalItem(GameObject go)
    {
        var parent = go.transform.parent;
        int idx = spawnPoints.IndexOf(parent);
        if (idx >= 0 && occupied[idx])
        {
            occupied[idx] = false;
            go.transform.SetParent(null);
            if (go.TryGetComponent<Rigidbody>(out var rb))
                rb.isKinematic = false;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Kembalikan indeks spawnPoints pertama yang FREE, atau -1 kalau penuh.
    /// </summary>
    public int GetNextFreeSlotIndex()
    {
        for (int i = 0; i < occupied.Length; i++)
            if (!occupied[i]) return i;
        return -1;
    }
    public bool HasEmptySlot()
    {
        foreach (var slot in occupied)
        {
            if (slot == false)
            {
                return true; // ketemu slot kosong
            }
        }
        return false; // tidak ada slot kosong
    }

}
