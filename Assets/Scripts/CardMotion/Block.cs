using Effekseer;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// ブロック演出処理
/// </summary>
public class Block : MonoBehaviour
{
    [Header("エフェクト類")]
    [Tooltip("白い円エフェクト")]
    [SerializeField] private EffekseerEffectAsset whiteLing;
    [Tooltip("黒い円エフェクト")]
    [SerializeField] private EffekseerEffectAsset blackLing;

    [Header("カード回転")]
    [SerializeField] private AnimationCurve rotateCurve;

    [Header("テキストのフェードイン,アウト用")]
    [Tooltip("フェードイン : テキスト拡大にかける時間（秒）")]
    [SerializeField] private float scalingTime = 0.3f;
    [Tooltip("フェードアウトにかける時間（秒）")]
    [SerializeField] private float fadeOutTime = 0.4f; 
    [Space(10)]

    [Header("カード回転時間")]
    [SerializeField] private float rotationTime = 0.3f;

    private RectTransform rectTransform;
    private CardMotionHelper cardMotionHelper;
    private UIHelper uiHelper;
    private Camera camera;

    private TextMeshProUGUI blockText;

    // Start is called before the first frame update
    void Start()
    {
        cardMotionHelper = SystemManager.Instance.cardMotionHelper;
        uiHelper         = SystemManager.Instance.uiHelper;
        camera           = CameraManager.Instance.mainCamera;
        blockText        = EffectCanvas.Instance.blockText;
    }

    /// <summary>
    /// ガード処理
    /// </summary>
    /// <returns></returns>
    public IEnumerator StartBlock(Transform attackedCard)
    {
        Debug.Log("ブロックスタート");
        // 自分の位置
        Vector3 myPosition = transform.localPosition;

        // 回転の始点を設定
        Quaternion startRotation = transform.rotation;
        // 回転の終点を設定
        //float targetYAngle = cardMotionHelper.GetYAngleToTarget(myPosition, attackedCard.transform.position);
        //Quaternion endRotation = Quaternion.Euler(0f, targetYAngle, 0f) * startRotation;
       
        // 敵カードへ向く
        //yield return StartCoroutine(cardMotionHelper.RotationTarget(transform, rotationTime, startRotation, endRotation, rotateCurve));

        // 白いリングを表示
        Vector3 whiteLingPosition = new Vector3(myPosition.x, myPosition.y + 0.1f, myPosition.z);
        EffekseerHandle handle = EffekseerSystem.PlayEffect(whiteLing, whiteLingPosition);

        // 黒いリングを表示
        Vector3 blackLingPosition = new Vector3(myPosition.x, myPosition.y + 0.1f, myPosition.z);
        EffekseerHandle handle2 = EffekseerSystem.PlayEffect(blackLing, whiteLingPosition);

        // GUARD!!テキストを表示
        yield return StartCoroutine(StartGuardTextFade(myPosition));

        // 元の角度に回転
        //StartCoroutine(cardMotionHelper.RotationTarget(transform, 0.7f, endRotation, startRotation, rotateCurve));
    }

    /// <summary>
    /// Guard!!テキストの表示処理
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartGuardTextFade(Vector3 pos)
    {
        // 表示位置をカードの位置に設定
        rectTransform = blockText.GetComponent<RectTransform>();
        Vector3 cardPosition = pos;
        //cardPosition.x += 70.0f;
        //cardPosition.y += 100.0f;
        rectTransform.localPosition = cardPosition;

        // テキスト表示(透明値を調整)
        Color startColor = blockText.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 1f);
        StartCoroutine(uiHelper.TextColoring(blockText, fadeOutTime, endColor));

        // テキスト拡大
        Vector3 startScale = rectTransform.localScale;
        Vector3 endScale = new Vector3(startScale.x, 1f, startScale.z);
        yield return StartCoroutine(cardMotionHelper.ScalingTarget(rectTransform.transform, scalingTime, startScale, endScale));

        // ここで0.4秒待機する
        yield return new WaitForSeconds(0.4f);

        // 縦に縮小し非表示にする
        startScale = rectTransform.localScale;
        endScale = new Vector3(1f, 0f, 1f);
        StartCoroutine(cardMotionHelper.ScalingTarget(rectTransform.transform, scalingTime, startScale, endScale));
        
        // 透明にする
        endColor.a = 0f;
        yield return StartCoroutine(uiHelper.TextColoring(blockText, fadeOutTime, endColor));
    }
}