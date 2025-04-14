using UnityEngine;

namespace ShopSimulator
{
    [System.Serializable]
    public class InventorySlot
    {
        public ItemData item;  // Referensi ke data item
        public int quantity;   // Jumlah item

        public InventorySlot(ItemData newItem, int qty)
        {
            item = newItem;
            quantity = qty;
        }
    }
}
