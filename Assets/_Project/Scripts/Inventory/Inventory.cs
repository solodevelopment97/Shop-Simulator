using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

namespace ShopSimulator
{
    public class Inventory : MonoBehaviour
    {
        [Header("Konfigurasi Inventory")]
        [Tooltip("Jumlah maksimal slot inventory")]
        public int MaxSlots { get; private set; } = 7;

        [Header("Data Inventory")]
        public List<InventorySlot> Slots { get; private set; } = new List<InventorySlot>();

        private Dictionary<string, List<InventorySlot>> itemLookup = new Dictionary<string, List<InventorySlot>>();
        private void Awake()
        {
            InitializeSlots();
        }

        private void InitializeSlots()
        {
            for (int i = 0; i < MaxSlots; i++)
            {
                var slot = new InventorySlot(null, 0);
                Slots.Add(slot);
            }
        }

        public bool AddItem(ItemData newItem, int quantity = 1)
        {
            if (newItem == null || quantity <= 0)
            {
                Debug.LogWarning("Invalid item or quantity.");
                return false;
            }

            // Cek apakah item bisa ditumpuk di slot yang ada
            foreach (var slot in Slots)
            {
                if (CanAddToSlot(slot, newItem, quantity))
                {
                    slot.quantity += quantity;
                    UpdateInteriorCount(slot, quantity);
                    return true;
                }
            }

            // Jika tidak bisa ditumpuk, cari slot kosong
            var emptySlot = Slots.FirstOrDefault(s => s.item == null);
            if (emptySlot != null)
            {
                AssignSlot(emptySlot, newItem, quantity);
                return true;
            }

            Debug.Log("Inventory Full!");
            return false;
        }

        public bool AddBox(ItemData boxData, int quantity, int interiorCount)
        {
            if (boxData == null || boxData.itemType != ItemType.Box)
            {
                Debug.LogError("Invalid box data provided.");
                return false;
            }

            // Cek apakah box bisa ditumpuk di slot yang ada
            foreach (var slot in Slots)
            {
                if (CanAddToSlot(slot, boxData, quantity))
                {
                    slot.quantity += quantity;
                    slot.interiorCount += interiorCount * quantity;
                    return true;
                }
            }

            // Jika tidak bisa ditumpuk, cari slot kosong
            var emptySlot = Slots.FirstOrDefault(s => s.item == null);
            if (emptySlot != null)
            {
                AssignSlot(emptySlot, boxData, quantity);
                emptySlot.interiorCount = interiorCount * quantity;
                return true;
            }

            Debug.Log("Inventory Full!");
            return false;
        }

        public bool RemoveItem(ItemData itemToRemove, int quantity = 1)
        {
            if (itemToRemove == null || quantity <= 0)
            {
                Debug.LogWarning("Invalid item or quantity.");
                return false;
            }

            if (itemLookup.TryGetValue(itemToRemove.itemName, out var slots))
            {
                foreach (var slot in slots.ToList()) // Gunakan ToList untuk menghindari modifikasi koleksi saat iterasi
                {
                    if (quantity <= 0) break;

                    int removeAmount = Mathf.Min(slot.quantity, quantity);
                    slot.quantity -= removeAmount;
                    quantity -= removeAmount;

                    if (slot.quantity <= 0)
                    {
                        ClearSlot(slot);
                    }
                }

                return quantity == 0;
            }

            Debug.LogWarning("Item not found in inventory.");
            return false;
        }

        public bool HasItem(ItemData checkItem, int quantity = 1)
        {
            if (itemLookup.TryGetValue(checkItem.itemName, out var slots))
            {
                int totalQuantity = slots.Sum(slot => slot.quantity);
                return totalQuantity >= quantity;
            }
            return false;
        }

        public void UpdateBoxInterior(ItemData boxData, int newInteriorCount)
        {
            if (boxData == null || boxData.itemType != ItemType.Box)
            {
                Debug.LogWarning("Invalid box data.");
                return;
            }

            if (itemLookup.TryGetValue(boxData.itemName, out var slots))
            {
                foreach (var slot in slots)
                {
                    slot.interiorCount = newInteriorCount;
                }
            }
        }

        private bool CanAddToSlot(InventorySlot slot, ItemData newItem, int quantity)
        {
            // Periksa apakah slot dapat menampung item baru
            return slot.item != null &&
                   slot.item == newItem &&
                   newItem.isStackable &&
                   slot.quantity + quantity <= newItem.maxStackSize;
        }

        private void AssignSlot(InventorySlot slot, ItemData newItem, int quantity)
        {
            slot.item = newItem;
            slot.quantity = quantity;

            if (newItem.itemType == ItemType.Box)
            {
                slot.interiorCount = CalculateInteriorCount(newItem, quantity);
            }
            else
            {
                slot.interiorCount = quantity;
            }

            if (!itemLookup.ContainsKey(newItem.itemName))
            {
                itemLookup[newItem.itemName] = new List<InventorySlot>();
            }
            if (!itemLookup[newItem.itemName].Contains(slot))
            {
                itemLookup[newItem.itemName].Add(slot);
            }
        }

        private void ClearSlot(InventorySlot slot)
        {
            if (itemLookup.TryGetValue(slot.item.itemName, out var slots))
            {
                slots.Remove(slot);
                if (slots.Count == 0)
                {
                    itemLookup.Remove(slot.item.itemName);
                }
            }

            slot.item = null;
            slot.quantity = 0;
            slot.interiorCount = 0;
        }

        private void UpdateInteriorCount(InventorySlot slot, int quantity)
        {
            if (slot.item.itemType == ItemType.Box)
            {
                int perBox = slot.item.boxQuantities.Sum();
                slot.interiorCount += perBox * quantity;
            }
        }

        private int CalculateInteriorCount(ItemData boxItem, int quantity)
        {
            if (boxItem.boxQuantities == null || boxItem.boxQuantities.Count == 0)
                return 0;

            int totalInterior = 0;
            foreach (var count in boxItem.boxQuantities)
            {
                totalInterior += count;
            }

            return totalInterior * quantity;
        }
    }
}
