using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class FitToGridCell : MonoBehaviour
{
    void Start()
    {
        var rect = GetComponent<RectTransform>();
        var parent = rect.parent as RectTransform;
        if (parent == null) return;

        // 親の Grid Layout Group を取得
        var grid = parent.GetComponentInParent<GridLayoutGroup>();
        if (grid == null) return;

        // グリッドのセルサイズ
        Vector2 cellSize = grid.cellSize;

        // カードの元画像サイズ（Prefab の sizeDelta）
        Vector2 originalSize = rect.sizeDelta;

        // セルに収まるスケール（アスペクト比維持）
        float scale = Mathf.Min(
            cellSize.x / originalSize.x,
            cellSize.y / originalSize.y
        );

        rect.localScale = Vector3.one * scale;

        // セル中央に配置
        rect.anchoredPosition = Vector2.zero;
    }
}
