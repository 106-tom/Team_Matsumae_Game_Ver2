using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 疲労回復演出クラス
/// </summary>
public class Heal : MonoBehaviour
{
    [Header("表示したいカードの見た目を映すカメラ")]
    [SerializeField] private GameObject UICamera;
    [Space(10)]

    [Header("表示したいカードの見た目を保存する先")]
    [SerializeField] private RenderTexture renderTexture;
    [Space(10)]

    //[Header("実際に表示するカード")]
    //[SerializeField] private GameObject mainCardIDDisplay;
    //[Space(10)]

    [Header("カード浮上")]
    [Tooltip("浮上時間")]
    [SerializeField] private float upTime;
    [Tooltip("浮上速度変化グラフ")]
    [SerializeField] private AnimationCurve upCurve;
    [SerializeField] private Vector3 upOffset;
    [Space(10)]

    [Header("カード下降")]
    [Tooltip("下降時間")]
    [SerializeField] private float downTime;
    [Tooltip("下降速度変化グラフ")]
    [SerializeField] private AnimationCurve downCurve;
    [Space(10)]

    [Header("カード回転")]
    [Tooltip("回転時間")]
    [SerializeField] private float rotateTime;
    [Tooltip("回転速度変化グラフ")]
    [SerializeField] private AnimationCurve rotateCurve;
    [Space(10)]

    private GameObject glassPlate;
    private CardMotionHelper cardMotionHelper;
    private CardEffectHelper cardEffectHelper;

    private void Start()
    {
        cardMotionHelper = SystemManager.Instance.cardMotionHelper;
        cardEffectHelper = SystemManager.Instance.cardEffectHelper;
    }

    /// <summary>
    /// 疲労回復演出
    /// </summary>
    /// <returns></returns>
    public IEnumerator StartHeal(
        Transform transform, 
        GameObject mainCardIDDisplay, 
        FkingCardFXManager cardFXManager)
    {
        // 上昇処理の始点と終点
        Vector3 upStartPosition = transform.localPosition;
        Vector3 upEndPosition = transform.localPosition + upOffset;

        // 降下の終点
        Vector3 downEndPosition = transform.localPosition;

        // 回転処理の始点と終点
        Quaternion startRotation = transform.rotation;
        Vector3 eulerAngles = new Vector3(0f, 0f, 0f);
        Quaternion endRotation = Quaternion.Euler(eulerAngles);

        // 浮かせる
        yield return StartCoroutine(cardMotionHelper.MoveTarget(transform, upTime, upStartPosition, upEndPosition, upCurve));
        Vector3 downStartPosition = transform.localPosition;

        // 回転させて光らせる
        //yield return StartCoroutine(cardMotionHelper.RotationTarget(transform, rotateTime, startRotation, endRotation, rotateCurve));
        
        // 光沢演出開始
        Color color = Color.white;
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
