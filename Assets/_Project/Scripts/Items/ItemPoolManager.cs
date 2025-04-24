using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ItemPoolManager : MonoBehaviour
{
    public static ItemPoolManager Instance;

    [Tooltip("List semua ItemData yang perlu di‑pool, assign di Inspector")]
    public List<ItemData> poolableItems;

    private Dictionary<ItemData, IObjectPool<GameObject>> pools
      = new Dictionary<ItemData, IObjectPool<GameObject>>();

    private void Awake()
    {
        Instance = this;
        foreach (var data in poolableItems)
        {
            // buat pool khusus untuk tiap prefab
            pools[data] = new ObjectPool<GameObject>(
                () => Instantiate(data.prefab),
                go =>
                {
                    go.SetActive(true);
                    // optional: reset transform/material, dsb
                },
                go =>
                {
                    go.SetActive(false);
                },
                go =>
                {
                    Destroy(go);
                },
                true,     // collectionCheck
                10,       // defaultCapacity
                100       // maxSize
            );
        }
    }

    /// <summary>
    /// Ambil instance dari pool berdasar ItemData.
    /// </summary>
    public GameObject Spawn(ItemData data)
    {
        if (!pools.TryGetValue(data, out var pool))
            throw new KeyNotFoundException(
                $"ItemData '{data.itemName}' belum diregister di pool.");
        var go = pool.Get();
        if (go.TryGetComponent<PickupItem>(out var pi))
            pi.isReleased = false;
        return go;
    }

    /// <summary>
    /// Lepas instance kembali ke pool.
    /// </summary>
    public void Despawn(ItemData data, GameObject go)
    {
        var pool = pools[data];  // throw kalau belum ada
        pool.Release(go);
    }
}
