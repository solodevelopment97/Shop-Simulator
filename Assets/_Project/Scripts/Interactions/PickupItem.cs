using Placement;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PickupItem : MonoBehaviour, IInteractable
{
    public ItemData itemData;

    private static GameObject cachedPlayer;
    private static PlayerCarry cachedCarry;
    private static FurniturePlacer cachedPlacer;

    public void Interact()
    {
        if (FurniturePlacer.Instance != null && FurniturePlacer.Instance.IsPlacing)
        {
            Debug.Log("Sedang menempatkan furniture, tidak bisa ambil item lain.");
            return;
        }

        var player = GetPlayer();
        if (player == null) return;

        var carry = GetCarry();
        if (carry == null || carry.IsCarrying)
        {
            Debug.Log("Tidak bisa ambil, sedang membawa barang.");
            return;
        }

        switch (itemData.itemType)
        {
            case ItemType.ShopItem:
                carry.PickUp(gameObject); // Tidak bisa di-place, hanya bisa dipegang & drop
                break;

            case ItemType.Furniture:
                var placer = GetPlacer();
                if (placer != null)
                {
                    placer.BeginPlacement(itemData, gameObject);
                }
                break;

            default:
                Debug.LogWarning($"ItemType {itemData.itemType} tidak dikenali.");
                break;
        }
    }

    public string GetInteractText()
    {
        return $"Ambil {itemData.itemName} (E)";
    }

    private GameObject GetPlayer()
    {
        if (cachedPlayer == null)
            cachedPlayer = GameObject.FindWithTag("Player");

        return cachedPlayer;
    }

    private PlayerCarry GetCarry()
    {
        if (cachedCarry == null && GetPlayer() != null)
            cachedCarry = cachedPlayer.GetComponent<PlayerCarry>();

        return cachedCarry;
    }

    private FurniturePlacer GetPlacer()
    {
        if (cachedPlacer == null && GetPlayer() != null)
            cachedPlacer = cachedPlayer.GetComponent<FurniturePlacer>();

        return cachedPlacer;
    }

    [RuntimeInitializeOnLoadMethod]
    private static void ResetCacheOnSceneLoad()
    {
        cachedPlayer = null;
        cachedCarry = null;
        cachedPlacer = null;
    }
}
