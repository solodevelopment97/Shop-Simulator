using UnityEngine;

namespace Placement
{
    public class FurniturePlacer : MonoBehaviour
    {
        public static FurniturePlacer Instance;

        [SerializeField] private Camera cam;
        [SerializeField] private LayerMask surfaceMask;
        [SerializeField] private Material wireframeMaterial;
        [SerializeField] private Material solidMaterial;

        private GameObject previewObj;
        private ItemData currentItem;
        private GameObject originalObj;
        private Bounds previewBounds;

        private float rotation;
        private bool isBlocked;

        public bool IsPlacing => previewObj != null;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void BeginPlacement(ItemData item, GameObject sourceObj)
        {
            if (previewObj != null) return;

            currentItem = item;
            originalObj = sourceObj;

            if (originalObj != null)
                originalObj.SetActive(false);

            previewObj = Instantiate(item.prefab);
            previewObj.name = item.itemName + "_Preview";

            // Hitung bounds SEBELUM collider dimatikan
            previewBounds = new Bounds(previewObj.transform.position, Vector3.zero);
            foreach (var col in previewObj.GetComponentsInChildren<Collider>())
            {
                previewBounds.Encapsulate(col.bounds);
            }

            // Nonaktifkan collider agar tidak bisa diklik/diinteraksi saat preview
            foreach (var col in previewObj.GetComponentsInChildren<Collider>())
            {
                col.enabled = false;
            }

            ApplyWireframe(previewObj);
            rotation = 0;
        }

        private void Update()
        {
            if (previewObj == null) return;

            UpdatePreviewPosition();

            rotation += Input.GetAxis("Mouse ScrollWheel") * 100f;

            if (Input.GetMouseButtonDown(0) && !isBlocked)
            {
                Place();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cancel();
            }
        }

        private void UpdatePreviewPosition()
        {
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            if (Physics.Raycast(ray, out RaycastHit hit, 10f, surfaceMask))
            {
                float bottomOffset = previewBounds.center.y - previewBounds.min.y;
                Vector3 targetPos = hit.point + Vector3.up * bottomOffset;
                Quaternion targetRot = Quaternion.Euler(0, rotation, 0);

                previewObj.transform.position = targetPos;
                previewObj.transform.rotation = targetRot;

                // Buat bounds transformasi di posisi baru
                Bounds worldBounds = new Bounds(targetPos, previewBounds.size);

                // Cek apakah menyentuh ShopItem
                Collider[] colliders = Physics.OverlapBox(
                    worldBounds.center,
                    worldBounds.extents,
                    targetRot
                );

                isBlocked = false;

                foreach (var col in colliders)
                {
                    if (col.GetComponent<ShopItemMarker>() != null)
                    {
                        isBlocked = true;
                        break;
                    }
                }

                SetWireframeBlocked(isBlocked);
            }
        }



        private void Place()
        {
            if (originalObj != null)
            {
                originalObj.transform.position = previewObj.transform.position;
                originalObj.transform.rotation = previewObj.transform.rotation;
                originalObj.SetActive(true);
                ApplySolid(originalObj);
            }

            Destroy(previewObj);
            ResetState();
        }

        private void Cancel()
        {
            Destroy(previewObj);
            if (originalObj != null)
                originalObj.SetActive(true);

            ResetState();
        }

        private void ResetState()
        {
            previewObj = null;
            currentItem = null;
            originalObj = null;
            rotation = 0f;
            isBlocked = false;
        }

        private void ApplyWireframe(GameObject obj)
        {
            foreach (var r in obj.GetComponentsInChildren<Renderer>())
                r.material = new Material(wireframeMaterial); // Instance baru agar warnanya bisa diubah
        }

        private void ApplySolid(GameObject obj)
        {
            foreach (var r in obj.GetComponentsInChildren<Renderer>())
                r.material = solidMaterial;
        }

        private void SetWireframeBlocked(bool blocked)
        {
            Color color = blocked ? Color.red : Color.green;

            foreach (var renderer in previewObj.GetComponentsInChildren<Renderer>())
            {
                if (renderer.material.HasProperty("_Color"))
                    renderer.material.color = color;
            }
        }
    }
}
