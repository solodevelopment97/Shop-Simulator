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
            Debug.Log("Sedang dalam mode placement. Tidak bisa mengambil item.");
            return;
        }

        if (!TryGetPlayerComponents(out var carry, out var placer)) return;

        if (carry.IsCarrying)
        {
            Debug.Log("Kamu sedang membawa barang.");
            return;
        }

        switch (itemData.itemType)
        {
            case ItemType.ShopItem:
                carry.PickUp(gameObject);
                break;

            case ItemType.Furniture:
                placer.BeginPlacement(itemData, gameObject);
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

    private bool TryGetPlayerComponents(out PlayerCarry carry, out FurniturePlacer placer)
    {
        carry = GetCarry();
        placer = GetPlacer();

        if (carry == null)
        {
            Debug.LogError("PlayerCarry tidak ditemukan di Player.");
            return false;
        }

        if (placer == null)
        {
            Debug.LogError("FurniturePlacer tidak ditemukan di Player.");
            return false;
        }

        return true;
    }

    private static GameObject GetPlayer()
    {
        if (cachedPlayer == null)
            cachedPlayer = GameObject.FindWithTag("Player");

        return cachedPlayer;
    }

    private static PlayerCarry GetCarry()
    {
        if (cachedCarry == null)
        {
            var player = GetPlayer();
            if (player != null)
                cachedCarry = player.GetComponent<PlayerCarry>();
        }

        return cachedCarry;
    }

    private static FurniturePlacer GetPlacer()
    {
        if (cachedPlacer == null)
        {
            var player = GetPlayer();
            if (player != null)
                cachedPlacer = player.GetComponent<FurniturePlacer>();
        }

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
