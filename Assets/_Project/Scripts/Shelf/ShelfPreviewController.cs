using UnityEngine;

public class ShelfPreviewController : MonoBehaviour
{
    public Material wireframeMaterial;
    [SerializeField] private Camera cam;

    private IPreviewable currentTarget;
    private GameObject previewInstance;
    private GameObject lastPrefab;  // prefab yang sedang di–preview

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    private void Update()
    {
        // 1) Raycast
        var ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        bool hit = Physics.Raycast(ray, out var info, 3f, LayerMask.GetMask("Interactable"));

        if (!hit)
        {
            // kalau ga kena apa‑apa → hide preview saja
            HidePreview();
            currentTarget = null;
            lastPrefab = null;
            return;
        }

        // 2) Coba dapatkan IPreviewable & prefab + transform
        var previewable = info.collider.GetComponentInParent<IPreviewable>();
        var prefab = previewable?.GetPreviewPrefab();
        var tfOpt = previewable?.GetPreviewTransform();

        if (previewable == null || prefab == null || !tfOpt.HasValue)
        {
            HidePreview();
            currentTarget = previewable;
            lastPrefab = null;
            return;
        }

        // 3) Jika target baru atau prefab berubah, rebuild previewInstance
        if (previewable != currentTarget || prefab != lastPrefab)
        {
            HidePreview();
            Destroy(previewInstance);
            previewInstance = null;

            currentTarget = previewable;
            lastPrefab = prefab;
        }

        // 4) Show / update previewInstance
        ShowPreview(prefab, tfOpt.Value.position, tfOpt.Value.rotation);
    }

    private void ShowPreview(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        // instantiate sekali saja per prefab
        if (previewInstance == null)
        {
            previewInstance = Instantiate(prefab);
            // disable collider
            foreach (var c in previewInstance.GetComponentsInChildren<Collider>())
                c.enabled = false;
            // ganti semua materials ke wireframe
            foreach (var r in previewInstance.GetComponentsInChildren<Renderer>())
            {
                var mats = new Material[r.sharedMaterials.Length];
                for (int i = 0; i < mats.Length; i++)
                    mats[i] = wireframeMaterial;
                r.materials = mats;
            }
        }

        // selalu update posisi & aktifkan
        previewInstance.transform.SetPositionAndRotation(pos, rot);
        if (!previewInstance.activeSelf)
            previewInstance.SetActive(true);
    }

    private void HidePreview()
    {
        if (previewInstance != null && previewInstance.activeSelf)
            previewInstance.SetActive(false);
    }
}
