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

    /// <summary>
    /// Menangani logika hotkey inventory:
    /// - Jika currentHeldInstance sudah tidak valid (misalnya sudah didrop), reset referensinya.
    /// - Jika tidak membawa item, ambil instance dari slot tersebut.
    /// - Jika hotkey ditekan untuk slot yang sama, toggle show/hide instance.
    /// - Jika hotkey ditekan untuk slot yang berbeda, sembunyikan instance aktif dan gunakan instance dari slot baru.
    /// Fungsi ini tidak memanggil Drop(), karena fungsi drop sudah ditangani di tempat lain.
    /// </summary>
    /// <param name="slotIndex">Indeks slot yang diakses oleh hotkey.</param>
    private void HandleHotkey(int slotIndex)
    {
        // Pertama: cek validitas currentHeldInstance
        if (currentHeldInstance != null && currentHeldInstance.transform.parent != playerCarry.HoldPoint)
        {
            // Instance ini sudah tidak berada di holdPoint (misalnya karena di-drop eksternal),
            // sehingga kita reset referensinya.
            currentHeldInstance = null;
            currentSlotIndex = -1;
        }

        // Validasi: pastikan slotIndex berada dalam jangkauan inventory yang terisi
        if (slotIndex < 0 || slotIndex >= inventory.slots.Count)
        {
            Debug.Log("Slot inventory kosong.");
            return;
        }

        InventorySlot slot = inventory.slots[slotIndex];
        if (slot.item == null || slot.item.prefab == null)
        {
            Debug.Log($"Slot {slotIndex + 1} kosong atau itemPrefab belum diassign.");
            return;
        }

        // Jika tidak membawa item (atau currentHeldInstance sudah di-reset), ambil instance untuk slot tersebut.
        if (!playerCarry.IsCarrying || currentHeldInstance == null)
        {
            GameObject instance = GetOrInstantiateInstance(slotIndex, slot.item.prefab);
            currentHeldInstance = instance;
            currentSlotIndex = slotIndex;
            instance.SetActive(true);
            playerCarry.PickUp(instance);
            return;
        }

        // Jika hotkey ditekan untuk slot yang sama (toggle)
        if (currentSlotIndex == slotIndex)
        {
            bool isActive = currentHeldInstance.activeSelf;
            currentHeldInstance.SetActive(!isActive);
            if (currentHeldInstance.activeSelf)
            {
                playerCarry.SetCarriedItem(currentHeldInstance);
            }
            else
            {
                playerCarry.ClearCarriedItem();
            }
        }
        else // Jika hotkey ditekan untuk slot yang berbeda (switch item)
        {
            // Sembunyikan instance yang sedang aktif tanpa memanggil Drop()
            if (currentHeldInstance != null && currentHeldInstance.activeSelf)
            {
                currentHeldInstance.SetActive(false);
            }
            playerCarry.ClearCarriedItem();

            // Ambil atau instantiate instance untuk slot baru, tampilkan, dan pick up
            GameObject newInstance = GetOrInstantiateInstance(slotIndex, slot.item.prefab);
            currentHeldInstance = newInstance;
            currentSlotIndex = slotIndex;
            newInstance.SetActive(true);
            playerCarry.PickUp(newInstance);
        }
    }

    /// <summary>
    /// Mengembalikan instance item untuk slot tertentu dari dictionary.
    /// Jika instance sudah ada namun parent-nya tidak valid (misalnya sudah di-drop), hapus entry dan instantiate baru.
    /// </summary>
    /// <param name="slotIndex">Indeks slot inventory.</param>
    /// <param name="prefab">Prefab item untuk slot tersebut.</param>
    /// <returns>GameObject instance yang valid.</returns>
    private GameObject GetOrInstantiateInstance(int slotIndex, GameObject prefab)
    {
        if (slotInstances.ContainsKey(slotIndex))
        {
            GameObject existing = slotInstances[slotIndex];
            if (existing == null || existing.transform.parent != playerCarry.HoldPoint)
            {
                // Jika instance sudah dihapus atau tidak valid, hapus entry dan buat baru
                slotInstances.Remove(slotIndex);
            }
            else
            {
                return existing;
            }
        }

        // Instantiate baru dan simpan di dictionary
        GameObject instance = Instantiate(prefab, playerCarry.transform.position, playerCarry.transform.rotation);
        slotInstances[slotIndex] = instance;
        return instance;
    }
}
