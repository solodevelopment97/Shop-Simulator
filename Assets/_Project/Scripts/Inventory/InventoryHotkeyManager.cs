using System.Collections.Generic;
using UnityEngine;
using ShopSimulator;
using Placement;

public class InventoryHotkeyManager : MonoBehaviour
{
    private Inventory inventory;
    private PlayerCarry playerCarry;

    private Dictionary<int, GameObject> slotInstances = new Dictionary<int, GameObject>();
    private int currentSlotIndex = -1;
    private GameObject currentHeldInstance;

    private KeyCode[] hotkeyCodes;

    private void Start()
    {
        inventory = GetComponent<Inventory>();
        playerCarry = GetComponent<PlayerCarry>();

        if (inventory == null)
        {
            Debug.LogError("Inventory component not found on GameObject.");
            return;
        }

        if (playerCarry == null)
        {
            Debug.LogError("PlayerCarry component not found on GameObject.");
            return;
        }

        // Cache hotkey KeyCodes for slots
        hotkeyCodes = new KeyCode[inventory.MaxSlots];
        for (int i = 0; i < inventory.MaxSlots; i++)
        {
            hotkeyCodes[i] = KeyCode.Alpha1 + i;
        }
    }

    private void Update()
    {
        if (FurniturePlacer.Instance != null && FurniturePlacer.Instance.IsPlacing)
            return;

        HandleHotkeyInput();
    }

    private void HandleHotkeyInput()
    {
        for (int i = 0; i < hotkeyCodes.Length; i++)
        {
            if (Input.GetKeyDown(hotkeyCodes[i]))
            {
                ProcessHotkey(i);
            }
        }
    }

    private void ProcessHotkey(int slotIndex)
    {
        ResetStateIfDropped();

        if (!IsValidSlot(slotIndex))
        {
            Debug.Log($"Slot {slotIndex + 1} is empty or invalid.");
            return;
        }

        if (!playerCarry.IsCarrying)
        {
            PickUpItemFromSlot(slotIndex);
        }
        else if (currentSlotIndex == slotIndex)
        {
            DropCurrentItem();
        }
        else
        {
            ReplaceCurrentItem(slotIndex);
        }
    }

    private void ResetStateIfDropped()
    {
        if (!playerCarry.IsCarrying && currentHeldInstance != null)
        {
            currentHeldInstance = null;
            currentSlotIndex = -1;
        }
    }

    private bool IsValidSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventory.Slots.Count)
            return false;

        var slot = inventory.Slots[slotIndex];
        return slot.item != null && slot.quantity > 0;
    }

    private void PickUpItemFromSlot(int slotIndex)
    {
        var slot = inventory.Slots[slotIndex];
        var inst = ItemPoolManager.Instance?.Spawn(slot.item);

        if (inst == null)
        {
            Debug.LogError("Failed to spawn item from pool.");
            return;
        }

        inst.transform.SetParent(playerCarry.HoldPoint, false);
        inst.transform.localPosition = Vector3.zero;
        inst.transform.localRotation = Quaternion.identity;

        if (inst.TryGetComponent<Rigidbody>(out var rb))
            rb.isKinematic = true;

        playerCarry.SetCarriedItem(inst);
        currentHeldInstance = inst;
        currentSlotIndex = slotIndex;
    }

    private void DropCurrentItem()
    {
        if (currentHeldInstance == null)
            return;

        var pickupItem = currentHeldInstance.GetComponent<PickupItem>();
        playerCarry.Drop(removeFromInventory: false);

        ItemPoolManager.Instance?.Despawn(pickupItem.itemData, currentHeldInstance);

        currentHeldInstance = null;
        currentSlotIndex = -1;
    }

    private void ReplaceCurrentItem(int slotIndex)
    {
        DropCurrentItem();
        PickUpItemFromSlot(slotIndex);
    }
}
