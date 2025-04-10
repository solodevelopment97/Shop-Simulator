// Scripts/Placement/FurniturePlacer.cs
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
        private float rotation;

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
                originalObj.SetActive(false); // Sembunyikan original saat preview

            previewObj = Instantiate(item.prefab);
            previewObj.name = item.itemName + "_Preview";

            ApplyWireframe(previewObj);
            rotation = 0;
        }

        private void Update()
        {
            if (previewObj == null) return;

            UpdatePreviewPosition();

            rotation += Input.GetAxis("Mouse ScrollWheel") * 100f;

            if (Input.GetMouseButtonDown(0))
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
                float offsetY = previewObj.GetComponent<Collider>().bounds.extents.y;
                previewObj.transform.position = hit.point + Vector3.up * offsetY;
                previewObj.transform.rotation = Quaternion.Euler(0, rotation, 0);
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
        }

        private void ApplyWireframe(GameObject obj)
        {
            foreach (var r in obj.GetComponentsInChildren<Renderer>())
                r.material = wireframeMaterial;
        }

        private void ApplySolid(GameObject obj)
        {
            foreach (var r in obj.GetComponentsInChildren<Renderer>())
                r.material = solidMaterial;
        }
    }
}
