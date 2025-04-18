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
        // Validasi slot index
        if (slotIndex < 0 || slotIndex >= inventory.slots.Count)
        {
            Debug.Log("Slot inventory kosong.");
            return;
        }

        var slot = inventory.slots[slotIndex];

        // ----------------------------
        // 1) Kalau slot benar-benar habis, buang instance lama dan abaikan keybind
        // ----------------------------
        if (slot.item == null || slot.quantity <= 0)
        {
            // Kalau kita masih memegang instance untuk slot ini, destroy & clear
            if (currentSlotIndex == slotIndex && currentHeldInstance != null)
            {
                Destroy(currentHeldInstance);
                slotInstances.Remove(slotIndex);
                playerCarry.ClearCarriedItem();
                currentHeldInstance = null;
                currentSlotIndex = -1;
            }

            Debug.Log($"Slot {slotIndex + 1} kosong.");
            return;
        }

        // ----------------------------
        // 2) Kalau belum ada item di-hold atau instance sudah di-clear, ambil baru
        // ----------------------------
        if (!playerCarry.IsCarrying || currentHeldInstance == null)
        {
            // Instantiate atau reuse
            GameObject instance = GetOrInstantiateInstance(slotIndex, slot.item.prefab);
            currentHeldInstance = instance;
            currentSlotIndex = slotIndex;
            instance.SetActive(true);
            playerCarry.PickUp(instance);
            return;
        }

        // ----------------------------
        // 3) Kalau keybind untuk slot yang sama (toggle)
        // ----------------------------
        if (currentSlotIndex == slotIndex)
        {
            bool isActive = currentHeldInstance.activeSelf;
            currentHeldInstance.SetActive(!isActive);

            if (currentHeldInstance.activeSelf)
                playerCarry.SetCarriedItem(currentHeldInstance);
            else
                playerCarry.ClearCarriedItem();

            return;
        }

        // ----------------------------
        // 4) Kalau switch slot lain, hide instance lama dan pickup baru
        // ----------------------------
        // hide lama tanpa memanggil Drop()
        if (currentHeldInstance.activeSelf)
            currentHeldInstance.SetActive(false);

        playerCarry.ClearCarriedItem();

        // instantiate/reuse slot baru
        GameObject newInstance = GetOrInstantiateInstance(slotIndex, slot.item.prefab);
        currentHeldInstance = newInstance;
        currentSlotIndex = slotIndex;
        newInstance.SetActive(true);
        playerCarry.PickUp(newInstance);
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
