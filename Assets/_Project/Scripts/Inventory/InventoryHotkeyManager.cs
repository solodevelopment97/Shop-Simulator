using System.Collections.Generic;
using UnityEngine;
using ShopSimulator;
using Placement;

public class InventoryHotkeyManager : MonoBehaviour
{
    private Inventory inventory;
    private PlayerCarry playerCarry;

    // Dictionary untuk menyimpan instance item per slot (key = slot index)
    private Dictionary<int, GameObject> slotInstances = new Dictionary<int, GameObject>();

    // Menyimpan index slot yang sedang aktif dan instance yang sedang di-hold
    private int currentSlotIndex = -1;
    private GameObject currentHeldInstance;

    private void Start()
    {
        inventory = GetComponent<Inventory>();
        playerCarry = GetComponent<PlayerCarry>();

        if (inventory == null)
            Debug.LogError("Inventory tidak ditemukan pada GameObject Player.");
        if (playerCarry == null)
            Debug.LogError("PlayerCarry tidak ditemukan pada GameObject Player.");
    }

    private void Update()
    {
        // Jika dalam mode placement, jangan proses hotkey inventory
        if (FurniturePlacer.Instance != null && FurniturePlacer.Instance.IsPlacing)
            return;

        // Periksa input hotkey untuk slot 1 hingga inventory.maxSlots
        for (int i = 0; i < inventory.maxSlots; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                HandleHotkey(i);
            }
        }
    }

    private void HandleHotkey(int slotIndex)
    {
        // 0) Kalau kita sudah drop, reset state
        if (!playerCarry.IsCarrying && currentHeldInstance != null)
        {
            currentHeldInstance = null;
            currentSlotIndex = -1;
        }

        // 1) Validasi slot
        if (slotIndex < 0 || slotIndex >= inventory.slots.Count) return;
        var slot = inventory.slots[slotIndex];
        if (slot.item == null || slot.quantity <= 0)
        {
            Debug.Log($"Slot {slotIndex + 1} kosong.");
            return;
        }

        // 2) Kalau tangan kosong → spawn & pickup
        if (!playerCarry.IsCarrying)
        {
            // Spawn fresh dari pool
            var inst = ItemPoolManager.Instance.Spawn(slot.item);
            inst.transform.SetParent(playerCarry.HoldPoint, false);
            inst.transform.localPosition = Vector3.zero;
            inst.transform.localRotation = Quaternion.identity;
            if (inst.TryGetComponent<Rigidbody>(out var rb)) rb.isKinematic = true;

            playerCarry.SetCarriedItem(inst);
            currentHeldInstance = inst;
            currentSlotIndex = slotIndex;
            return;
        }

        // 3) Kalau sudah bawa & slot yang sama ditekan → drop
        if (currentSlotIndex == slotIndex && currentHeldInstance != null)
        {
            // clear dari tangan
            var pi = currentHeldInstance.GetComponent<PickupItem>();
            playerCarry.Drop(removeFromInventory: false);// me‑unparent & enable physics
            ItemPoolManager.Instance.Despawn(pi.itemData, currentHeldInstance);
            currentHeldInstance = null;
            currentSlotIndex = -1;
            return;
        }

        // 4) Kalau bawa tapi tekan slot beda → drop dulu, lalu spawn slot baru
        if (currentHeldInstance != null)
        {
            var oldPi = currentHeldInstance.GetComponent<PickupItem>();
            // Hanya clear dari tangan, jangan potong inventory
            playerCarry.Drop(removeFromInventory: false);
            ItemPoolManager.Instance.Despawn(oldPi.itemData, currentHeldInstance);
            currentHeldInstance = null;
            currentSlotIndex = -1;
        }
        HandleHotkey(slotIndex); // rekursi kecil untuk spawn baru
    }
}
