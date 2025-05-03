using System.Collections.Generic;
using UnityEngine;

namespace Placement
{
    /// <summary>
    /// Handles the placement of furniture in the game world.
    /// </summary>
    public class FurniturePlacer : MonoBehaviour
    {
        public static FurniturePlacer Instance { get; private set; }

        [Header("Placement Settings")]
        [SerializeField] private Camera cam;
        [SerializeField] private LayerMask surfaceMask;
        [SerializeField] private Material wireframeMaterial;
        [SerializeField] private Material solidMaterial;

        private readonly Dictionary<Renderer, Material[]> originalMaterials = new();
        private GameObject previewObject;
        private GameObject originalObject;
        private ItemData currentItemData;
        private Bounds previewBounds;

        private float currentRotation;
        private bool isPlacementBlocked;

        public bool IsPlacing => previewObject != null;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Update()
        {
            if (!IsPlacing) return;

            HandlePlacementInput();
            UpdatePreviewTransform();
        }

        /// <summary>
        /// Starts the placement process for a given item.
        /// </summary>
        public void BeginPlacement(ItemData itemData, GameObject sourceObject)
        {
            if (IsPlacing) return;

            currentItemData = itemData;
            originalObject = sourceObject;

            if (originalObject != null)
                originalObject.SetActive(false);

            previewObject = Instantiate(itemData.prefab);
            CacheOriginalMaterials(previewObject);
            previewObject.name = $"{itemData.itemName}_Preview";

            previewBounds = CalculateBounds(previewObject);
            DisableColliders(previewObject);
            ApplyMaterial(previewObject, wireframeMaterial);

            currentRotation = 0f;
        }

        private void HandlePlacementInput()
        {
            currentRotation += Input.GetAxis("Mouse ScrollWheel") * 100f;

            if (Input.GetMouseButtonDown(0) && !isPlacementBlocked)
            {
                ConfirmPlacement();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelPlacement();
            }
        }

        private void UpdatePreviewTransform()
        {
            if (cam == null)
            {
                Debug.LogError("Camera is not assigned in FurniturePlacer.");
                return;
            }

            if (previewObject == null) return;

            if (previewBounds.size == Vector3.zero)
            {
                Debug.LogWarning("Preview bounds are empty. Colliders might not be detected.");
                return;
            }

            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            if (Physics.Raycast(ray, out RaycastHit hit, 10f, surfaceMask))
            {
                Vector3 targetPosition = hit.point + Vector3.up * (previewBounds.center.y - previewBounds.min.y);
                Quaternion targetRotation = Quaternion.Euler(0f, currentRotation, 0f);

                previewObject.transform.SetPositionAndRotation(targetPosition, targetRotation);

                isPlacementBlocked = CheckCollisionAtPreview(targetPosition, targetRotation);
                UpdateWireframeColor(isPlacementBlocked);
            }
        }

        private void ConfirmPlacement()
        {
            if (originalObject != null)
            {
                originalObject.transform.SetPositionAndRotation(previewObject.transform.position, previewObject.transform.rotation);
                originalObject.SetActive(true);
                RestoreOriginalMaterials(originalObject);
            }

            Destroy(previewObject);
            ClearPlacementState();
        }

        private void CancelPlacement()
        {
            if (previewObject != null)
                Destroy(previewObject);

            if (originalObject != null)
                originalObject.SetActive(true);

            ClearPlacementState();
        }

        private void ClearPlacementState()
        {
            previewObject = null;
            currentItemData = null;
            originalObject = null;
            currentRotation = 0f;
            isPlacementBlocked = false;
            originalMaterials.Clear();
        }

        private Bounds CalculateBounds(GameObject obj)
        {
            Bounds bounds = new Bounds(obj.transform.position, Vector3.zero);

            foreach (Collider col in obj.GetComponentsInChildren<Collider>())
            {
                bounds.Encapsulate(col.bounds);
            }

            return bounds;
        }

        private void DisableColliders(GameObject obj)
        {
            foreach (var col in obj.GetComponentsInChildren<Collider>())
            {
                col.enabled = false;
            }
        }

        private bool CheckCollisionAtPreview(Vector3 position, Quaternion rotation)
        {
            Bounds worldBounds = new Bounds(position, previewBounds.size);
            Collider[] hits = new Collider[10]; // Adjust size as needed
            int hitCount = Physics.OverlapBoxNonAlloc(worldBounds.center, worldBounds.extents, hits, rotation);

            for (int i = 0; i < hitCount; i++)
            {
                if (hits[i].GetComponent<ShopItemMarker>() != null)
                {
                    return true;
                }
            }

            return false;
        }

        private void ApplyMaterial(GameObject obj, Material material)
        {
            foreach (Renderer renderer in obj.GetComponentsInChildren<Renderer>())
            {
                int count = renderer.sharedMaterials.Length;
                Material[] materials = new Material[count];
                for (int i = 0; i < count; i++)
                    materials[i] = new Material(material);

                renderer.materials = materials;
            }
        }

        private void RestoreOriginalMaterials(GameObject obj)
        {
            foreach (var pair in originalMaterials)
            {
                if (pair.Key != null)
                {
                    pair.Key.materials = pair.Value;
                }
            }
        }

        private void CacheOriginalMaterials(GameObject obj)
        {
            originalMaterials.Clear();
            foreach (var renderer in obj.GetComponentsInChildren<Renderer>())
            {
                originalMaterials[renderer] = renderer.sharedMaterials;
            }
        }

        private void UpdateWireframeColor(bool isBlocked)
        {
            if (previewObject == null) return;

            Color color = isBlocked ? Color.red : Color.green;

            foreach (Renderer renderer in previewObject.GetComponentsInChildren<Renderer>())
            {
                if (renderer == null || renderer.material == null) continue;

                if (renderer.material.HasProperty("_Color"))
                {
                    renderer.material.color = color;
                }
            }
        }
    }
}
