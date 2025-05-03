using UnityEngine;

public class ShelfPreviewController : MonoBehaviour
{
    public Material wireframeMaterial;
    public LayerMask shelfMask;
    public LayerMask pickupMask;

    private Transform cameraTransform;
    private IPreviewable currentTarget;
    private GameObject previewInstance;
    private GameObject lastPrefab;
    private int combinedLayerMask;

    private void Awake()
    {
        cameraTransform = Camera.main?.transform;
        if (cameraTransform == null)
        {
            Debug.LogError("Main Camera not found!");
        }
        combinedLayerMask = shelfMask | pickupMask;
    }

    private void Update()
    {
        if (cameraTransform == null) return;

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        var hits = Physics.RaycastAll(ray, 5f, combinedLayerMask);

        if (hits.Length == 0)
        {
            ResetPreview();
            return;
        }

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            if (IsPickupLayer(hit.collider.gameObject.layer))
            {
                ResetPreview();
                return;
            }

            if (IsShelfLayer(hit.collider.gameObject.layer))
            {
                HandleShelfHit(hit);
                return;
            }
        }

        ResetPreview();
    }

    private bool IsPickupLayer(int layer) => (pickupMask & (1 << layer)) != 0;

    private bool IsShelfLayer(int layer) => (shelfMask & (1 << layer)) != 0;

    private void HandleShelfHit(RaycastHit hit)
    {
        var previewable = hit.collider.GetComponentInParent<IPreviewable>();
        if (previewable == null) return;

        var prefab = previewable.GetPreviewPrefab();
        var transformData = previewable.GetPreviewTransform();

        if (prefab != null && transformData.HasValue)
        {
            ShowOrUpdatePreview(prefab, transformData.Value.position, transformData.Value.rotation, previewable);
        }
    }

    private void ShowOrUpdatePreview(GameObject prefab, Vector3 position, Quaternion rotation, IPreviewable previewable)
    {
        if (previewable != currentTarget || prefab != lastPrefab)
        {
            ResetPreview();
            currentTarget = previewable;
            lastPrefab = prefab;
        }

        if (previewInstance == null)
        {
            previewInstance = Instantiate(prefab);
            DisableColliders(previewInstance);
            ApplyWireframeMaterial(previewInstance);
        }

        previewInstance.transform.SetPositionAndRotation(position, rotation);
        if (!previewInstance.activeSelf)
        {
            previewInstance.SetActive(true);
        }
    }

    private void ResetPreview()
    {
        if (previewInstance != null && previewInstance.activeSelf)
        {
            previewInstance.SetActive(false);
        }
        currentTarget = null;
        lastPrefab = null;
    }

    private void DisableColliders(GameObject obj)
    {
        foreach (var collider in obj.GetComponentsInChildren<Collider>())
        {
            collider.enabled = false;
        }
    }

    private void ApplyWireframeMaterial(GameObject obj)
    {
        foreach (var renderer in obj.GetComponentsInChildren<Renderer>())
        {
            var materials = new Material[renderer.sharedMaterials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = wireframeMaterial;
            }
            renderer.materials = materials;
        }
    }
}
