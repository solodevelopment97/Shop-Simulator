using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ShopSimulator
{
    [System.Serializable]
    public class InventorySlot
    {
        public ItemData item;  // Referensi ke data item
        public int quantity;   // Jumlah item
        public int interiorCount;

        public InventorySlot(ItemData newItem, int qty)
        {
            item = newItem;
            quantity = qty;
            if(item != null && item.itemType == ItemType.Box)
            {
                // Kalau boxQuantities null, inisialisasi sebagai list kosong
                var list = item.boxQuantities ?? new List<int>();
                var sum = 0;
                foreach (var q in list)
                    sum += q;
                this.interiorCount = sum;
            }
            else
            {
                // ShopItem atau itemData null
                this.interiorCount = quantity;
            }
        }
    }
}
