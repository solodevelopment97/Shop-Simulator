using System.Collections.Generic;
using UnityEngine;

namespace ShopSimulator
{
    [System.Serializable]
    public class InventorySlot
    {
        public ItemData item;  // Referensi ke data item
        public int quantity;   // Jumlah item
        public int interiorCount; // Jumlah isi dalam box (jika itemType == Box)

        public InventorySlot(ItemData newItem, int qty)
        {
            item = newItem;
            quantity = qty;

            if (item != null && item.itemType == ItemType.Box)
            {
                // Hitung interiorCount berdasarkan isi box
                interiorCount = CalculateInteriorCount(item, qty);
            }
            else
            {
                // Untuk item biasa atau jika item null
                interiorCount = qty;
            }
        }

        public bool CanStack(ItemData newItem, int qtyToAdd)
        {
            // Periksa apakah item sama, stackable, dan tidak melebihi batas stack
            return item != null &&
                   item == newItem &&
                   item.isStackable &&
                   quantity + qtyToAdd <= item.maxStackSize;
        }

        private int CalculateInteriorCount(ItemData boxItem, int qty)
        {
            if (boxItem.boxQuantities == null || boxItem.boxQuantities.Count == 0)
                return 0;

            int totalInterior = 0;
            foreach (var count in boxItem.boxQuantities)
            {
                totalInterior += count;
            }

            return totalInterior * qty;
        }
    }
}
