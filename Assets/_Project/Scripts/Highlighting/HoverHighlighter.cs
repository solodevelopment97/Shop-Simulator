using UnityEngine;
using QuickOutline;
using Placement;

public class HoverHighlighter : MonoBehaviour
{
    [Header("Warna Highlight")]
    public Color shopItemColor = Color.blue;
    public Color boxItemColor;
    public Color furnitureColor = Color.yellow;
    public LayerMask highlightMask;

    private Outline lastOutline;

    void Update()
    {
        // Nonaktifkan outline sebelumnya
        if (lastOutline != null)
        {
            lastOutline.enabled = false;
            lastOutline = null;
        }

        // Raycast
        if (Physics.Raycast(transform.position, transform.forward, out var hit, 5f, highlightMask))
        {
            var outline = hit.collider.GetComponentInParent<Outline>();
            if (outline != null)
            {
                // Pilih warna berdasarkan tipe
                Color c = Color.white;

                if (hit.collider.TryGetComponent<PickupItem>(out var pi1)
                    && pi1.itemData.itemType == ItemType.ShopItem)
                {
                    c = shopItemColor;
                }
                else if (hit.collider.TryGetComponent<PickupItem>(out var pi2)
                    && pi2.itemData.itemType == ItemType.Box)
                {
                    c = boxItemColor;
                }
                else if (hit.collider.GetComponentInParent<InteractableFurniture>() != null)
                {
                    c = furnitureColor;
                }

                outline.OutlineColor = c;
                outline.OutlineWidth = 5f;
                outline.enabled = true;

                outline.UpdateMaterialProperties();
                lastOutline = outline;
            }
        }
    }
}
