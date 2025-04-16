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
        // Daftar slot inventory yang fixed panjangnya
        public List<InventorySlot> slots = new List<InventorySlot>();

        private void Awake()
        {
            // Inisialisasi list dengan maxSlots slot kosong
            for (int i = 0; i < maxSlots; i++)
            {
                // Slot kosong direpresentasikan dengan item = null dan quantity = 0
                slots.Add(new InventorySlot(null, 0));
            }
        }

        /// <summary>
        /// Menambah item ke inventory.
        /// Jika item sudah ada, akan dilakukan penumpukan (stack) jika memungkinkan.
        /// Jika belum ada dan terdapat slot kosong, item akan disimpan di slot kosong.
        /// </summary>
        public bool AddItem(ItemData newItem, int quantity = 1)
        {
            // Cek apakah item sudah ada dalam inventory (bisa diubah jika terdapat aturan stackable)
            InventorySlot slot = slots.Find(x => x.item == newItem);
            if (slot != null)
            {
                // Jika sudah ada, tambahkan quantity
                slot.quantity += quantity;
                return true;
            }
            else
            {
                // Jika belum ada, cari slot kosong (di mana item == null)
                InventorySlot emptySlot = slots.Find(x => x.item == null);
                if (emptySlot != null)
                {
                    emptySlot.item = newItem;
                    emptySlot.quantity = quantity;
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
        /// Jika quantity item setelah pengurangan <= 0, maka slot dikosongkan tanpa menggeser index slot.
        /// </summary>
        public bool RemoveItem(ItemData itemToRemove, int quantity = 1)
        {
            InventorySlot slot = slots.Find(x => x.item == itemToRemove);
            if (slot != null)
            {
                slot.quantity -= quantity;
                if (slot.quantity <= 0)
                {
                    // Jangan menghapus slot, cukup set sebagai kosong
                    slot.item = null;
                    slot.quantity = 0;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Memeriksa apakah inventory memiliki item tertentu dengan jumlah minimal tertentu.
        /// </summary>
        public bool HasItem(ItemData checkItem, int quantity = 1)
        {
            InventorySlot slot = slots.Find(x => x.item == checkItem);
            return slot != null && slot.quantity >= quantity;
        }
    }
}
