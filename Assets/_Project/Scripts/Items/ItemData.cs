// Scripts/Items/ItemData.cs
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ShopSim/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public GameObject prefab;
    public ItemType itemType;

    [Header("Isi Box (jika itemType == Box)")]
    public List<ItemData> boxItems = new List<ItemData>();
    public List<int> boxQuantities = new List<int>();

    private void OnValidate()
    {
        // pastikan jumlah pair sama
        if (boxItems.Count != boxQuantities.Count)
            Debug.LogWarning($"{name}: boxItems.Count harus sama dengan boxQuantities.Count");
    }
}
