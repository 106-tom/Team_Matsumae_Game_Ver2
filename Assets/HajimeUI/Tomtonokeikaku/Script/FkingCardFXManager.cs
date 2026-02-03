using UnityEngine;
using System.Collections; // コルーチン（フェード処理）のために必要

/// <summary>
/// カードの追加エフェクト（光沢、破壊、フェード）を管理するスクリプト。
/// FkingCardDisplay や CardVisualController と共存させて使います。
/// </summary>
public class FkingCardFXManager : MonoBehaviour
{
    // ▼▼▼ 修正点: 1. アニメーターの項目を削除 ▼▼▼

    [Header("1. 破壊エフェクト")]
    [Tooltip("CardBreak用の背景に置く黒いカードのメッシュ")]
    public GameObject cardBreakBackground;

    [Tooltip("カード全体のUIをフェードさせるためのCanvasGroup")]
    public CanvasGroup cardCanvasGroup;

    [Header("2. 光沢表現")]
    [Tooltip("光沢表現に使うガラス板のメッシュ（外部から参照されます）")]
    public GameObject glossPlate;

    [Header("3. 動的スケーリング (連携用)")]
    [Tooltip("拡大縮小の基準となる現在のフレームTransform (FkingCardDisplayから自動設定)")]
    public Transform battleStateTarget; // FkingCardDisplayから自動で設定されます


    // --- 内部ロジック ---

    void Update()
    {
        // 3. 動的スケーリング機能
        // battleStateTarget の大きさに、光沢と破壊背景の大きさを毎フレーム合わせる
        ApplyDynamicScaling();
    }

    /// <summary>
    /// battleStateTarget のスケールを glossPlate と cardBreakBackground に適用します。
    /// </summary>
    public void ApplyDynamicScaling()
    {
        if (battleStateTarget == null)
        {
            // ターゲットが設定されていなければ何もしない
            return;
        }

        // ターゲットのローカルスケールを取得
        Vector3 targetScale = battleStateTarget.localScale;

        // メッシュ（Quadなど）のスケールを合わせる
        if (glossPlate != null)
        {
            //glossPlate.transform.localScale = targetScale;
        }

        if (cardBreakBackground != null)
        {
           // cardBreakBackground.transform.localScale = targetScale;
        }
    }


    // --- 外部から呼び出す関数 ---

    /// <summary>
    /// 破壊用UI（黒いカード）の表示/非表示を切り替えます。
    /// </summary>
    public void SetCardBreakBackgroundActive(bool isActive)
    {
        if (cardBreakBackground != null)
        {
            cardBreakBackground.SetActive(isActive);
        }
    }

    public GameObject GetCardBreakObject() { return cardBreakBackground; }
    public GameObject GetGlossPlate() { return glossPlate; }

    /// <summary>
    /// カードUI全体をフェードアウトさせます（破壊用）。
    /// </summary>
    /// <param name="duration">フェードにかかる時間</param>
    public void FadeCardOut(float duration = 0.5f)
    {
        if (cardCanvasGroup != null)
        {
            StartCoroutine(DoFade(0f, duration));
        }
    }

    /// <summary>
    /// カードUI全体をフェードインさせます。
    /// </summary>
    public void FadeCardIn(float duration = 0.5f)
    {
        if (cardCanvasGroup != null)
        {
            StartCoroutine(DoFade(1f, duration));
        }
    }

    // フェード処理の実体
    private IEnumerator DoFade(float targetAlpha, float duration)
    {
        float startAlpha = cardCanvasGroup.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            cardCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }
        cardCanvasGroup.alpha = targetAlpha;
    }
}