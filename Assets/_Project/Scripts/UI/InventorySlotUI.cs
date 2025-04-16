using UnityEngine;
using UnityEngine.UI;
using ShopSimulator; // Sesuaikan namespace dengan project Anda
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI quantityText;

    /// <summary>
    /// Setup UI slot berdasarkan data InventorySlot.
    /// Jika slot null atau slot tidak memiliki item, tampilkan slot kosong.
    /// </summary>
    public void Setup(InventorySlot slot)
    {
        if (slot != null && slot.item != null)
        {
            icon.sprite = slot.item.icon;
            icon.enabled = true;
            quantityText.text = slot.quantity > 1 ? slot.quantity.ToString() : "";
        }
        else
        {
            // Tampilkan slot kosong
            icon.enabled = false;
            quantityText.text = "";
        }
    }
}
