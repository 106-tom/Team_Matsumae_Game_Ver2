using UnityEngine;

public class PreviewUI : MonoBehaviour
{
    [SerializeField] GameObject previewPanel;

    public void Open() { if (previewPanel) previewPanel.SetActive(true); }
    public void Close() { if (previewPanel) previewPanel.SetActive(false); }
    public void Toggle() { if (previewPanel) previewPanel.SetActive(!previewPanel.activeSelf); }
}
