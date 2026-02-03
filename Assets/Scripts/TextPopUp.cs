using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// テキストのポップアップ演出クラス
/// </summary>
public class TextPopUp : MonoBehaviour
{
    [Header("フェードイン,アウトにかける時間")]
    [SerializeField] private float fadeInTime = 0.3f;
    [SerializeField] private float fadeOutTime = 0.4f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            StartCoroutine(StartTextPopUp());
        }
    }

    /// <summary>
    /// テキストポップアップ演出開始
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartTextPopUp()
    {
        TextMeshProUGUI targetText = GetComponent<TextMeshProUGUI>();
        RectTransform rectTransform = GetComponent<RectTransform>();
        SpriteHelper spriteHelper = GetComponent<SpriteHelper>();

        rectTransform.localScale = new Vector3(0, rectTransform.localScale.y, rectTransform.localScale.z);

        // 少し待機
        yield return new WaitForSeconds(0.2f);

        // 拡大してフェードイン
        Vector3 endScale = new Vector3(1, rectTransform.localScale.y, rectTransform.localScale.z);
        yield return StartCoroutine(spriteHelper.TextScaling(rectTransform, fadeInTime, endScale));

        // 少し待機
        yield return new WaitForSeconds(0.4f);

        // 横に引き延ばす
        endScale = new Vector3(3.0f, 0f, 0f);
        StartCoroutine(spriteHelper.TextScaling(rectTransform, fadeOutTime,endScale ));

        // 徐々に透明にする
        Color endColor = new Color(1f, 1f, 1f, 0f);
        yield return StartCoroutine(spriteHelper.TextColoring(targetText, fadeOutTime, endColor));
    }
}
