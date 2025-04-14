using System.Collections.Generic;
using UnityEngine;

namespace ShopSimulator
{
    public class Inventory : MonoBehaviour
    {
        [Header("Konfigurasi Inventory")]
        [Tooltip("Jumlah maksimal slot inventory")]
        public int maxSlots = 20;

        [Header("Data Inventory")]
        // Daftar slot inventory
        public List<InventorySlot> slots = new List<InventorySlot>();

        /// <summary>
        /// Menambah item ke inventory.
        /// Jika item sudah ada, akan dilakukan penumpukan (stack) jika memungkinkan.
        /// </summary>
        public bool AddItem(ItemData newItem, int quantity = 1)
        {
            // Cek apakah item sudah ada dalam inventory (bisa diubah jika terdapat aturan stackable)
            InventorySlot slot = slots.Find(x => x.item == newItem);
            if (slot != null)
            {
                slot.quantity += quantity;
                return true;
            }
            else
            {
                // Jika belum ada dan masih ada ruang slot
                if (slots.Count < maxSlots)
                {
                    slots.Add(new InventorySlot(newItem, quantity));
                    return true;
                }
                else
                {
                    Debug.Log("Inventory Full!");
                    return false;
                }
            }
        }

        /// <summary>
        /// Menghapus item dari inventory.
        /// </summary>
        public bool RemoveItem(ItemData itemToRemove, int quantity = 1)
        {
            InventorySlot slot = slots.Find(x => x.item == itemToRemove);
            if (slot != null)
            {
                slot.quantity -= quantity;
                if (slot.quantity <= 0)
                {
                    slots.Remove(slot);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Memeriksa apakah inventory memiliki item tertentu.
        /// </summary>
        public bool HasItem(ItemData checkItem, int quantity = 1)
        {
            InventorySlot slot = slots.Find(x => x.item == checkItem);
            return slot != null && slot.quantity >= quantity;
        }
    }
}
