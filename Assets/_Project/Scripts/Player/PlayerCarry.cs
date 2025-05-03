using UnityEngine;
using Placement;
using ShopSimulator;

[RequireComponent(typeof(FurniturePlacer))]
public class PlayerCarry : MonoBehaviour
{
    [Header("Carry Settings")]
    [SerializeField] private Transform holdPoint;
    [SerializeField] private Camera cam;

    [Header("Bobbing Effect Settings")]
    [Tooltip("Base speed of bobbing when the player is moving.")]
    [SerializeField] private float baseBobbingSpeed = 6f;
    [Tooltip("Amplitude of the bobbing effect.")]
    [SerializeField] private float baseBobbingAmount = 0.05f;

    private float bobbingTimer = 0f;
    private Vector3 defaultHoldLocalPosition;
    private GameObject carriedItem;
    private FurniturePlacer furniturePlacer;
    private PlayerMovement playerMovement;
    private Vector3 bobbingOffset = Vector3.zero;

    public bool IsCarrying => carriedItem != null;
    public Transform HoldPoint => holdPoint;
    public GameObject HeldItem => carriedItem;

    private void Awake()
    {
        InitializeHoldPoint();
        InitializeCamera();
        furniturePlacer = GetComponent<FurniturePlacer>();
        playerMovement = GetComponent<PlayerMovement>();

        Debug.Assert(furniturePlacer != null, "FurniturePlacer component is missing.");
        Debug.Assert(playerMovement != null, "PlayerMovement component is missing.");
    }

    private void Update()
    {
        if (IsCarrying)
        {
            ApplyBobbingEffect();
        }
    }

    /// <summary>
    /// Picks up the specified item and attaches it to the hold point.
    /// </summary>
    /// <param name="item">The item to pick up.</param>
    public void PickUp(GameObject item)
    {
        if (item == null)
        {
            Debug.LogWarning("Attempted to pick up a null item.");
            return;
        }

        if (IsCarrying)
        {
            Drop();
        }

        carriedItem = item;
        AttachToHoldPoint(carriedItem);

        if (carriedItem.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true;
        }
    }

    /// <summary>
    /// Drops the currently held item.
    /// </summary>
    /// <param name="removeFromInventory">Whether to remove the item from the inventory.</param>
    public void Drop(bool removeFromInventory = true)
    {
        if (!IsCarrying) return;

        if (removeFromInventory && carriedItem.TryGetComponent<PickupItem>(out var pickup))
        {
            var inventory = GetComponent<Inventory>();
            inventory?.RemoveItem(pickup.itemData, 1);
            FindFirstObjectByType<InventoryUI>()?.UpdateUI();
        }

        DetachFromHoldPoint(carriedItem);

        if (carriedItem.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = false;
        }

        carriedItem = null;
    }

    /// <summary>
    /// Begins the placement process for the currently held item.
    /// </summary>
    public void BeginPlacementFromHand()
    {
        if (!IsCarrying) return;

        if (!carriedItem.TryGetComponent<PickupItem>(out var pickupItem) || pickupItem.itemData == null)
        {
            Debug.LogWarning("Held item is missing PickupItem or itemData.");
            return;
        }

        if (pickupItem.itemData.itemType != ItemType.Furniture)
        {
            Debug.Log("Held item is not furniture and cannot be placed.");
            return;
        }

        DetachFromHoldPoint(carriedItem);
        furniturePlacer.BeginPlacement(pickupItem.itemData, carriedItem);
        carriedItem = null;
    }

    /// <summary>
    /// Clears the reference to the currently held item without modifying its state.
    /// </summary>
    public void ClearCarriedItem()
    {
        carriedItem = null;
    }

    /// <summary>
    /// Manually sets the carried item without invoking Drop().
    /// </summary>
    /// <param name="item">The item to set as carried.</param>
    public void SetCarriedItem(GameObject item)
    {
        carriedItem = item;
    }

    private void InitializeHoldPoint()
    {
        if (holdPoint == null)
        {
            Debug.LogWarning("HoldPoint is not assigned. Creating a default hold point.");
            holdPoint = new GameObject("DefaultHoldPoint").transform;
            holdPoint.SetParent(transform);
            holdPoint.localPosition = new Vector3(0, 1.5f, 1);
        }
        defaultHoldLocalPosition = holdPoint.localPosition;
    }

    private void InitializeCamera()
    {
        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("MainCamera is not found in the scene.");
            }
        }
    }

    private void ApplyBobbingEffect()
    {
        if (playerMovement != null && playerMovement.IsMoving)
        {
            float speedFactor = playerMovement.CurrentSpeed / playerMovement.runSpeed;
            bobbingTimer = Mathf.Repeat(bobbingTimer + baseBobbingSpeed * speedFactor * Time.deltaTime, Mathf.PI * 2f);
            float offsetY = Mathf.Sin(bobbingTimer) * baseBobbingAmount;
            bobbingOffset = new Vector3(0, offsetY, 0);
        }
        else
        {
            bobbingOffset = Vector3.Lerp(bobbingOffset, Vector3.zero, Time.deltaTime * 8f);
        }

        if (carriedItem != null)
        {
            carriedItem.transform.localPosition = bobbingOffset;
        }
    }

    private void AttachToHoldPoint(GameObject item)
    {
        item.transform.SetParent(holdPoint);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
    }

    private void DetachFromHoldPoint(GameObject item)
    {
        item.transform.SetParent(null);
        item.transform.position = holdPoint.position;
    }
}
