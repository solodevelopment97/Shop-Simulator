using System.Collections.Generic;
using UnityEngine;

namespace Placement
{
    public class FurniturePlacer : MonoBehaviour
    {
        public static FurniturePlacer Instance { get; private set; }

        [Header("Placement Settings")]
        [SerializeField] private Camera cam;
        [SerializeField] private LayerMask surfaceMask;
        [SerializeField] private Material wireframeMaterial;
        [SerializeField] private Material solidMaterial;

        private Dictionary<Renderer, Material[]> originalMaterials = new();

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

        public void BeginPlacement(ItemData itemData, GameObject sourceObject)
        {
            if (IsPlacing) return;

            currentItemData = itemData;
            originalObject = sourceObject;

            if (originalObject != null)
                originalObject.SetActive(false);

            previewObject = Instantiate(itemData.prefab);
            originalMaterials.Clear(); // bersihkan dulu
            foreach (var renderer in previewObject.GetComponentsInChildren<Renderer>())
            {
                originalMaterials[renderer] = renderer.sharedMaterials; // pakai sharedMaterials agar tidak instansi baru
            }
            previewObject.name = itemData.itemName + "_Preview";

            previewBounds = CalculateBounds(previewObject);
            DisableColliders(previewObject);
            ApplyWireframeMaterial(previewObject);

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
                Debug.LogError("Camera belum diassign di FurniturePlacer.");
                return;
            }

            if (previewObject == null)
            {
                //Debug.LogWarning("Preview object null saat UpdatePreviewTransform.");
                return;
            }

            if (previewBounds.size == Vector3.zero)
            {
                Debug.LogWarning("Preview bounds kosong. Kemungkinan collider belum terdeteksi.");
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
            previewObject = null;

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

            Collider[] hits = Physics.OverlapBox(
                worldBounds.center,
                worldBounds.extents,
                rotation
            );

            foreach (var hit in hits)
            {
                if (hit.GetComponent<ShopItemMarker>() != null)
                {
                    return true;
                }
            }

            return false;
        }

        private void ApplyWireframeMaterial(GameObject obj)
        {
            foreach (Renderer renderer in obj.GetComponentsInChildren<Renderer>())
            {
                // Ganti semua material jadi wireframeMaterial
                int count = renderer.sharedMaterials.Length;
                Material[] wireframes = new Material[count];
                for (int i = 0; i < count; i++)
                    wireframes[i] = new Material(wireframeMaterial); // instansi agar warnanya bisa diubah

                renderer.materials = wireframes;
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

            originalMaterials.Clear();
        }

        private void UpdateWireframeColor(bool isBlocked)
        {
            if (previewObject == null)
            {
                //Debug.LogWarning("Preview object null saat UpdateWireframeColor.");
                return;
            }

            Color color = isBlocked ? Color.red : Color.green;

            foreach (Renderer renderer in previewObject.GetComponentsInChildren<Renderer>())
            {
                if (renderer == null || renderer.material == null)
                {
                    Debug.LogWarning("Renderer atau material null dalam UpdateWireframeColor.");
                    continue;
                }

                if (renderer.material.HasProperty("_Color"))
                {
                    renderer.material.color = color;
                }
            }
        }

    }
}
