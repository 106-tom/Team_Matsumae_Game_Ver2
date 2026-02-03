using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// UI補助関数
/// </summary>
public class UIHelper : MonoBehaviour
{
    /// <summary>
    /// スプライト用フェードイン
    /// </summary>
    /// <param name="targetSprite">フェードインさせたいスプライト</param>
    /// <param name="fadeInTime">フェードインにかける時間</param>
    /// <returns></returns>
    public IEnumerator SpriteFadeIn(SpriteRenderer targetSprite, float fadeInTime)
    {
        float elapsedTime = 0f;
        Color startColor = targetSprite.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 1f);

        while (elapsedTime < fadeInTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeInTime;
            targetSprite.color = Color.Lerp(startColor, endColor, t);

            yield return null;
        }
        targetSprite.color = endColor;
    }

    /// <summary>
    /// スプライト用フェードアウト
    /// </summary>
    /// <param name="targetSprite">フェードアウトさせたいスプライト</param>
    /// <param name="fadeOutTime">フェードアウトにかける時間</param>
    /// <returns></returns>
    public IEnumerator SpriteFadeOut(SpriteRenderer targetSprite, float fadeOutTime)
    {
        float elapsedTime = 0f;
        Color startColor = targetSprite.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        while (elapsedTime < fadeOutTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeOutTime;
            targetSprite.color = Color.Lerp(startColor, endColor, t);

            yield return null;
        }
        targetSprite.color = endColor;
    }

    /// <summary>
    /// テキスト拡大・縮小
    /// </summary>
    /// <param name="targetTransform">スケーリングさせたいスプライトのRectTransform</param>
    /// <param name="scalingTime">スケーリングにかける時間</param>
    /// <param name="targetScale">最終的なスケール</param>
    /// <returns></returns>
    public IEnumerator TextScaling(RectTransform targetTransform, float scalingTime, Vector3 targetScale)
    {
        float elapsedTime = 0f;
        Vector3 startScale = targetTransform.localScale;

        while (elapsedTime < scalingTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / scalingTime;
            targetTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        targetTransform.localScale = targetScale; // 最終スケールに設定
    }

    /// <summary>
    /// テキスト色変更
    /// </summary>
    /// <param name="targetText">カラーリングさせたいテキスト</param>
    /// <param name="coloringTime">カラーリングにかける時間</param>
    /// <param name="endColor">最終的な色</param>
    /// <returns></returns>
    public IEnumerator TextColoring(TextMeshProUGUI targetText, float coloringTime, Color endColor)
    {
        float elapsedTime = 0f;
        Color startColor = targetText.color;

        while (elapsedTime < coloringTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / coloringTime;
            targetText.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }
        targetText.color = endColor;
    }
}
