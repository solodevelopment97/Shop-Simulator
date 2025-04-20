using UnityEngine;

public interface IPreviewable
{
    /// <summary>
    /// Prefab apa yang dipakai untuk preview (bisa berupa produk, box, dsb).
    /// </summary>
    GameObject GetPreviewPrefab();

    /// <summary>
    /// Posisi dan rotasi preview — misalnya slot pada rak.
    /// Kembalikan null kalau tidak ingin preview sekarang.
    /// </summary>
    (Vector3 position, Quaternion rotation)? GetPreviewTransform();
}
