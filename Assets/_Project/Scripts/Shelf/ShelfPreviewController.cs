using UnityEngine;

public class ShelfPreviewController : MonoBehaviour
{
    public Material wireframeMaterial;
    public LayerMask shelfMask;   // assign di Inspector ke layer “Shelf”
    public LayerMask pickupMask;  // assign di Inspector ke layer “Pickup”

    private Transform camT;
    private IPreviewable currentTarget;
    private GameObject previewInstance;
    private GameObject lastPrefab;

    private void Awake()
    {
        camT = Camera.main.transform;
    }

    private void Update()
    {
        // RaycastAll mencakup kedua layer
        int bothMasks = shelfMask | pickupMask;
        Ray ray = new Ray(camT.position, camT.forward);
        var hits = Physics.RaycastAll(ray, 5f, bothMasks);
        if (hits.Length == 0)
        {
            HidePreview();
            currentTarget = null;
            lastPrefab = null;
            return;
        }

        // urutkan berdasarkan jarak
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        bool didShelf = false;
        foreach (var hit in hits)
        {
            // 1) kalau ini ItemShop/Box → langsung hide & keluar
            if ((pickupMask & (1 << hit.collider.gameObject.layer)) != 0)
            {
                HidePreview();
                currentTarget = null;
                lastPrefab = null;
                return;
            }

            // 2) kalau ini rak (IPreviewable) → tampilkan preview
            if ((shelfMask & (1 << hit.collider.gameObject.layer)) != 0)
            {
                var prev = hit.collider.GetComponentInParent<IPreviewable>();
                if (prev != null)
                {
                    var prefab = prev.GetPreviewPrefab();
                    var tfOpt = prev.GetPreviewTransform();
                    if (prefab != null && tfOpt.HasValue)
                    {
                        ShowOrUpdate(prefab, tfOpt.Value.position, tfOpt.Value.rotation, prev);
                        didShelf = true;
                        break;
                    }
                }
            }
        }

        if (!didShelf)
        {
            HidePreview();
            currentTarget = null;
            lastPrefab = null;
        }
    }

    private void ShowOrUpdate(GameObject prefab, Vector3 pos, Quaternion rot, IPreviewable prev)
    {
        // rebuild jika target/prefab berganti
        if (prev != currentTarget || prefab != lastPrefab)
        {
            HidePreview();
            if (previewInstance != null) Destroy(previewInstance);
            previewInstance = null;
            currentTarget = prev;
            lastPrefab = prefab;
        }

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
        if (!previewInstance.activeSelf)
            previewInstance.SetActive(true);
    }

    private void HidePreview()
    {
        if (previewInstance != null && previewInstance.activeSelf)
            previewInstance.SetActive(false);
    }
}
