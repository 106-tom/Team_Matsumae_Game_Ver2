using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// バフ演出クラス
/// </summary>
public class Buff : MonoBehaviour
{
    [Header("表示したいカードの見た目を映すカメラ")]
    [SerializeField] private GameObject UICamera;
    [Space(10)]

    [Header("表示したいカードの見た目を保存する先")]
    [SerializeField] private RenderTexture renderTexture;
    [Space(10)]

    [Header("カード浮上")]
    [Tooltip("浮上時間")]
    [SerializeField] private float upTime;
    [Tooltip("浮上速度変化グラフ")]
    [SerializeField] private AnimationCurve upCurve;
    [Space(10)]

    [Header("カード下降")]
    [Tooltip("下降時間")]
    [SerializeField] private float downTime;
    [Tooltip("下降速度変化グラフ")]
    [SerializeField] private AnimationCurve downCurve;

    private GameObject glassPlate;

    // 補助クラス
    CardMotionHelper cardMotionHelper;
    CardEffectHelper cardEffectHelper;

    private void Start()
    {
        cardMotionHelper = SystemManager.Instance.cardMotionHelper;
        cardEffectHelper = SystemManager.Instance.cardEffectHelper;
    }

    /// <summary>
    /// バフ演出開始
    /// </summary>
    /// <returns></returns>
    public IEnumerator StartBuff(
        Transform transform,
        GameObject mainCardIDDisplay,
        FkingCardFXManager cardFXManager)
    {
        // 上昇処理の始点と終点
        Vector3 upStartPosition = transform.localPosition;
        Vector3 upEndPosition = transform.localPosition + new Vector3(0f, 0.5f, 0f);

        // 降下の終点
        Vector3 downEndPosition = transform.localPosition;

        // 浮かせる
        yield return StartCoroutine(cardMotionHelper.MoveTarget(transform, upTime, upStartPosition, upEndPosition, upCurve));
        Vector3 downStartPosition = transform.localPosition;

        // 光沢演出開始
        Color color = Color.green;
        StartCoroutine(cardEffectHelper.PlayShineEffect(
            mainCardIDDisplay,
            UICamera,
            glassPlate,
            renderTexture,
            cardFXManager,
            downTime,
            color));

        // 下げる
        yield return StartCoroutine(cardMotionHelper.MoveTarget(transform, downTime, downStartPosition, downEndPosition, downCurve));
    }
}
