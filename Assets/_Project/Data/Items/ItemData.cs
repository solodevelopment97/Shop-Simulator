using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Shop/Item")]
public class ItemData : ScriptableObject
{
    [Header("Item Info")]
    public string itemName;
    public Sprite itemIcon;
    public GameObject prefab;
    public float weight;

    [Header("Item Type")]
    public ItemType itemType;
}
