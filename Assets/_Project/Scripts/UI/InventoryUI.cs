using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ShopSimulator
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("Prefab for Inventory Slot UI")]
        [SerializeField] private GameObject slotPrefab;
        [Tooltip("Parent transform for placing slot UI (e.g., bottom bar panel)")]
        [SerializeField] private Transform slotParent;

        private Inventory inventory;
        private readonly List<GameObject> slotPool = new();

        private void Start()
        {
            // Get reference to Inventory; e.g., if Inventory is on the player
            inventory = FindFirstObjectByType<Inventory>();
            if (inventory == null)
            {
                Debug.LogError("Inventory not found in the scene!");
                return;
            }

            InitializeSlotPool();
            UpdateUI();
        }

        /// <summary>
        /// Initializes the object pool for inventory slots.
        /// </summary>
        private void InitializeSlotPool()
        {
            if (slotPrefab == null || slotParent == null)
            {
                Debug.LogError("SlotPrefab or SlotParent is not assigned!");
                return;
            }

            for (int i = 0; i < inventory.MaxSlots; i++)
            {
                GameObject slotInstance = Instantiate(slotPrefab, slotParent);
                slotInstance.SetActive(false);
                slotPool.Add(slotInstance);
            }
        }

        /// <summary>
        /// Updates the inventory UI to reflect the current state of the inventory.
        /// </summary>
        public void UpdateUI()
        {
            if (inventory == null || slotPool.Count == 0)
                return;

            int slotCount = inventory.Slots.Count;

            for (int i = 0; i < slotPool.Count; i++)
            {
                GameObject slotInstance = slotPool[i];
                InventorySlotUI slotUI = slotInstance.GetComponent<InventorySlotUI>();

                if (i < slotCount)
                {
                    slotUI.Setup(inventory.Slots[i]);
                    slotInstance.SetActive(true);
                }
                else
                {
                    slotUI.Setup(null);
                    slotInstance.SetActive(false);
                }
            }
        }
    }
}
