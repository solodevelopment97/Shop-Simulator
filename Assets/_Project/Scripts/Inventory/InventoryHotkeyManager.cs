using UnityEngine;
using ShopSimulator; // Pastikan namespace sesuai dengan project Anda

public class InventoryHotkeyManager : MonoBehaviour
{
    private Inventory inventory;
    private PlayerCarry playerCarry;

    // Menyimpan referensi instance item yang sedang di-hold oleh keybind
    private GameObject currentHeldInstance;
    // Menyimpan index slot yang terakhir digunakan
    private int currentSlotIndex = -1;

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
        // Periksa keybind untuk slot 1 sampai dengan inventory.maxSlots
        for (int i = 0; i < inventory.maxSlots; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                HandleHotkey(i);
            }
        }
    }

    /// <summary>
    /// Menangani logika keybind:
    /// - Jika belum ada item di-hold, instantiate dan tampilkan item dari slot.
    /// - Jika keybind yang ditekan sama dengan item yang sedang di-hold, toggle aktif/nonaktif.
    /// - Jika keybind untuk item lain ditekan, hide item lama dan instantiate item baru.
    /// Inventory tidak dikurangi (item tetap ada) sampai fungsi drop (yang menghapus item) dipanggil terpisah.
    /// </summary>
    /// <param name="slotIndex">Indeks slot yang diakses oleh keybind.</param>
    private void HandleHotkey(int slotIndex)
    {
        // Validasi slot: jika slotIndex melebihi jumlah slot yang terisi, keluarkan pesan
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

        // Jika belum ada item yang di-hold, langsung instantiate dan tampilkan item tersebut.
        if (!playerCarry.IsCarrying || currentHeldInstance == null)
        {
            GameObject instance = Instantiate(
                slot.item.prefab,
                playerCarry.transform.position,
                playerCarry.transform.rotation
            );
            currentHeldInstance = instance;
            currentSlotIndex = slotIndex;
            // Gunakan method PickUp() supaya item diparent ke holdPoint
            playerCarry.PickUp(instance);
            return;
        }

        // Jika sudah ada item yang di-hold, periksa apakah item tersebut berasal dari slot yang sama.
        // Ambil komponen PickupItem dari currentHeldInstance untuk membandingkan itemData.
        PickupItem currentPickup = currentHeldInstance.GetComponent<PickupItem>();
        if (currentPickup == null)
        {
            Debug.LogWarning("Item yang dipegang tidak memiliki komponen PickupItem.");
            return;
        }

        // Jika keybind yang ditekan sama dengan yang sedang di-hold
        if (currentSlotIndex == slotIndex)
        {
            // Toggle aktif/nonaktif: jika aktif, hide; jika tersembunyi, show kembali.
            bool isActive = currentHeldInstance.activeSelf;
            currentHeldInstance.SetActive(!isActive);

            if (!isActive)
            {
                // Jika item di-show kembali, pastikan referensi di PlayerCarry di-update.
                playerCarry.SetCarriedItem(currentHeldInstance);
            }
            else
            {
                // Saat di-hide, clear referensinya dari PlayerCarry agar PickUp berikutnya tidak otomatis drop.
                playerCarry.ClearCarriedItem();
            }
        }
        else
        {
            // Jika keybind untuk item yang berbeda ditekan:
            // Hide item yang sedang di-hold
            if (currentHeldInstance.activeSelf)
            {
                currentHeldInstance.SetActive(false);
            }
            // Perbarui currentSlotIndex dan instantiate item baru
            GameObject newInstance = Instantiate(
                slot.item.prefab,
                playerCarry.transform.position,
                playerCarry.transform.rotation
            );
            currentHeldInstance = newInstance;
            currentSlotIndex = slotIndex;
            // Pastikan item baru diparent ke holdPoint melalui PickUp()
            // Karena playerCarry.ClearCarriedItem() telah dipanggil saat hide, 
            // PickUp() tidak akan memanggil Drop() (karena IsCarrying jadi false).
            playerCarry.PickUp(newInstance);
        }
    }
}
