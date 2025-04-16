// Scripts/Items/ItemData.cs
using UnityEngine;

[CreateAssetMenu(menuName = "ShopSim/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public GameObject prefab;
    public ItemType itemType;
}
