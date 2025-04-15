using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ShopSimulator; // Sesuaikan namespace dengan project Anda
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Prefab untuk Inventory Slot UI")]
    [SerializeField] private GameObject slotPrefab;
    [Tooltip("Parent transform untuk meletakkan slot UI (misalnya panel bottom bar)")]
    [SerializeField] private Transform slotParent;

    private Inventory inventory;

    private void Start()
    {
        // Ambil referensi ke Inventory; misalnya, jika Inventory ada pada player
        inventory = FindFirstObjectByType<Inventory>();
        if (inventory == null)
        {
            Debug.LogError("Inventory tidak ditemukan di scene!");
            return;
        }
        UpdateUI();
    }

    /// <summary>
    /// Update tampilan UI inventory sesuai dengan data pada Inventory.
    /// Selalu membuat UI slot sebanyak inventory.maxSlots, dan mengatur slot kosong jika belum ada item.
    /// </summary>
    public void UpdateUI()
    {
        // Hapus semua slot UI lama
        foreach (Transform child in slotParent)
        {
            Destroy(child.gameObject);
        }

        // Loop untuk membuat slot UI sebanyak maxSlots
        for (int i = 0; i < inventory.maxSlots; i++)
        {
            GameObject slotInstance = Instantiate(slotPrefab, slotParent);
            InventorySlotUI slotUI = slotInstance.GetComponent<InventorySlotUI>();

            // Jika indeks kurang dari jumlah slot terisi di inventory, tampilkan data slotnya.
            // Jika tidak, tampilkan slot kosong.
            if (i < inventory.slots.Count)
            {
                slotUI.Setup(inventory.slots[i]);
            }
            else
            {
                slotUI.Setup(null);
            }
        }
    }
}
