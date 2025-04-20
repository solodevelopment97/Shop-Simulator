using UnityEngine;

public class ShelfPreviewController : MonoBehaviour
{
    public Material wireframeMaterial;
    [SerializeField] private Camera cam;

    private IPreviewable currentTarget;
    private GameObject previewInstance;
    private GameObject lastPrefab;  // <— simpan prefab terakhir

    void Update()
    {
        if (cam == null) return;

        // Raycast ke layer Interactable
        bool hit = Physics.Raycast(
            cam.ViewportPointToRay(new Vector3(0.5f, 0.5f)),
            out var info,
            3f,
            LayerMask.GetMask("Interactable")
        );
        if (!hit)
        {
            ClearPreview();
            currentTarget = null;
            lastPrefab = null;
            return;
        }

        // dapatkan IPreviewable dan prefab
        var previewable = info.collider.GetComponentInParent<IPreviewable>();
        var prefabOpt = previewable?.GetPreviewPrefab();
        var tfOpt = previewable?.GetPreviewTransform();
        if (previewable == null || prefabOpt == null || !tfOpt.HasValue)
        {
            ClearPreview();
            currentTarget = previewable;
            lastPrefab = null;
            return;
        }

        // Kalau target baru **atau** prefab baru → reset preview
        if (previewable != currentTarget || prefabOpt != lastPrefab)
        {
            ClearPreview();
            currentTarget = previewable;
            lastPrefab = prefabOpt;
        }

        // Panggil ShowPreview dengan prefab & transform
        ShowPreview(prefabOpt, tfOpt.Value.position, tfOpt.Value.rotation);
    }

    private void ShowPreview(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (previewInstance == null)
        {
            previewInstance = Instantiate(prefab);
            foreach (var c in previewInstance.GetComponentsInChildren<Collider>())
                c.enabled = false;
            foreach (var r in previewInstance.GetComponentsInChildren<Renderer>())
            {
                var mats = new Material[r.sharedMaterials.Length];
                for (int i = 0; i < mats.Length; i++)
                    mats[i] = wireframeMaterial;
                r.materials = mats;
            }
        }
        previewInstance.transform.SetPositionAndRotation(pos, rot);
        previewInstance.SetActive(true);
    }

    private void ClearPreview()
    {
        if (previewInstance != null)
            Destroy(previewInstance);
        previewInstance = null;
    }

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
    }
}