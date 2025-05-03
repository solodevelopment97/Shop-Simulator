using UnityEngine;
using QuickOutline;
using Placement;

public class HoverHighlighter : MonoBehaviour
{
    [Header("Highlight Colors")]
    public Color shopItemColor = Color.blue;
    public Color boxItemColor;
    public Color furnitureColor = Color.yellow;

    [Header("LayerMasks")]
    public LayerMask shelfMask;
    public LayerMask pickupMask;
    //public LayerMask npcMask; // Tambahan untuk NPC di masa depan

    private Outline lastOutline;

    void Update()
    {
        // Nonaktifkan outline sebelumnya
        if (lastOutline != null)
        {
            lastOutline.enabled = false;
            lastOutline = null;
        }

        // Raycast untuk Shelf dan Pickup
        if (Physics.Raycast(transform.position, transform.forward, out var hit, 5f, shelfMask | pickupMask))
        {
            var outline = hit.collider?.GetComponentInParent<Outline>();
            if (outline != null)
            {
                // Pilih warna berdasarkan tipe objek
                Color highlightColor = GetHighlightColor(hit.collider);

                // Terapkan properti outline
                outline.OutlineColor = highlightColor;
                outline.OutlineWidth = 5f;
                outline.enabled = true;

                outline.UpdateMaterialProperties();
                lastOutline = outline;
            }
        }
    }

    private Color GetHighlightColor(Collider collider)
    {
        // Cek untuk PickupItem
        if (collider.TryGetComponent<PickupItem>(out var pickupItem))
        {
            return pickupItem.itemData.itemType switch
            {
                ItemType.ShopItem => shopItemColor,
                ItemType.Box => boxItemColor,
                _ => Color.white
            };
        }

        // Cek untuk InteractableFurniture
        if (collider.GetComponentInParent<InteractableFurniture>() != null)
        {
            return furnitureColor;
        }

        // Cek untuk NPC (contoh tambahan)
        //if (npcMask != 0 && collider.gameObject.layer == LayerMask.NameToLayer("NPC"))
        //{
        //    return Color.green; // Warna untuk NPC
        //}

        // Default color
        return Color.white;
    }
}
