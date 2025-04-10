using UnityEngine;

namespace Placement
{
    public class FurniturePlacer : MonoBehaviour
    {
        public static FurniturePlacer Instance;

        [SerializeField] private Camera cam;
        [SerializeField] private LayerMask surfaceMask;
        [SerializeField] private Material wireframeMaterial;
        [SerializeField] private Material defaultMaterial;

        private GameObject previewInstance;
        private ItemData currentItem;
        private float currentRotation;
        private InteractableFurniture originObject;

        private void Awake() => Instance = this;

        public void BeginPlacement(ItemData item, MonoBehaviour source)
        {
            if (previewInstance != null) return;

            currentItem = item;
            originObject = source;

            previewInstance = Instantiate(item.prefab);
            SetWireframeMaterial(previewInstance);
        }

        void Update()
        {
            if (previewInstance == null) return;

            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            if (Physics.Raycast(ray, out RaycastHit hit, 10f, surfaceMask))
            {
                var bounds = previewInstance.GetComponent<Collider>().bounds;
                float yOffset = bounds.extents.y;
                previewInstance.transform.position = hit.point + Vector3.up * yOffset;
                previewInstance.transform.rotation = Quaternion.Euler(0f, currentRotation, 0f);
            }

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0f)
                currentRotation += scroll * 100f;

            if (Input.GetMouseButtonDown(0)) PlaceFurniture();
            if (Input.GetKeyDown(KeyCode.Escape)) CancelPlacement();
        }

        void PlaceFurniture()
        {
            SetDefaultMaterial(previewInstance);
            previewInstance = null;
            currentItem = null;
            originObject = null;
            currentRotation = 0f;
        }

        void CancelPlacement()
        {
            Destroy(previewInstance);
            originObject?.Restore();
            previewInstance = null;
            currentItem = null;
            originObject = null;
            currentRotation = 0f;
        }

        void SetWireframeMaterial(GameObject obj)
        {
            foreach (var r in obj.GetComponentsInChildren<Renderer>())
                r.sharedMaterial = wireframeMaterial;
        }

        void SetDefaultMaterial(GameObject obj)
        {
            foreach (var r in obj.GetComponentsInChildren<Renderer>())
                r.sharedMaterial = defaultMaterial;
        }
    }
}
