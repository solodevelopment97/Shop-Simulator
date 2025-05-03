using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIHintManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI hintText;

    private readonly Dictionary<string, string> activeHints = new();

    /// <summary>
    /// Menambahkan atau memperbarui hint berdasarkan kunci.
    /// </summary>
    public void SetHint(string key, string text)
    {
        if (string.IsNullOrEmpty(key)) return;

        if (string.IsNullOrEmpty(text))
        {
            activeHints.Remove(key);
        }
        else
        {
            activeHints[key] = text;
        }

        UpdateHintText();
    }

    /// <summary>
    /// Menghapus hint berdasarkan kunci.
    /// </summary>
    public void ClearHint(string key)
    {
        if (activeHints.ContainsKey(key))
        {
            activeHints.Remove(key);
            UpdateHintText();
        }
    }

    /// <summary>
    /// Menghapus semua hint.
    /// </summary>
    public void ClearAllHints()
    {
        activeHints.Clear();
        UpdateHintText();
    }

    /// <summary>
    /// Memperbarui teks hint yang ditampilkan.
    /// </summary>
    private void UpdateHintText()
    {
        if (hintText == null) return;

        hintText.text = string.Join("\n", activeHints.Values);
        hintText.enabled = activeHints.Count > 0;
    }
}
