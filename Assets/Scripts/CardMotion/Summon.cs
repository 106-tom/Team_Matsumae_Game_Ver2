using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Cinemachine;

/// <summary>
/// 召喚演出クラス
/// </summary>
public class Summon : MonoBehaviour
{
    [Header("エフェクト類")]
    [Tooltip("閃光エフェクト")]
    [SerializeField] private ParticleSystem flashEffect;
    [Tooltip("煙エフェクト")]
    [SerializeField] private ParticleSystem smokeEffect;
    [Tooltip("岩エフェクト")]
    [SerializeField] private ParticleSystem rockEffect;
    [Space(10)]

    [Header("画面振動用")]
    [Tooltip("振動の強さ")]
    [SerializeField] private float amplitude;
    [Tooltip("振動の速さ")]
    [SerializeField] private float frequency;
    [Tooltip("振動時間")]
    [SerializeField] private float shakeTime;
    [Space(10)]

    [Header("カード上昇")]
    [Tooltip("どの位置まで上昇するか")]
    [SerializeField] private Vector3 upEndPosition;
    [Tooltip("上昇時間")]
    [SerializeField] private float upTime;
    [Tooltip("上昇速度変化グラフ")]
    [SerializeField] private AnimationCurve upCurve;
    [Space(10)]

    [Header("カード前進")]
    [Tooltip("前進時間")]
    [SerializeField] private float forwardTime;
    [Tooltip("前進速度変化グラフ")]
    [SerializeField] private AnimationCurve forwardCurve;
    [Space(10)]

    [Header("フィールドカード位置調整 : どれだけ横にずらすか(カード間の半分の距離)")]
    [SerializeField] private float shiftOffset;
    [Space(10)]

   // [SerializeField] private MeshRenderer[] renderers;

    private Transform field;

    private CardMotionHelper cardMotionHelper;
    private CameraManager cameraManager;// = CameraManager.Instance;
    private CinemachineVirtualCamera virtualCamera;// = cameraManager.virtualCamera;
    private CinemachineBasicMultiChannelPerlin noise;

    private void Start()
    {
        cardMotionHelper   = SystemManager.Instance.cardMotionHelper;
        // 画面振動用カメラ取得
        cameraManager = CameraManager.Instance;
        virtualCamera = cameraManager.virtualCamera;
        noise = cameraManager.noise;
    }

    /// <summary>
    /// 召喚演出
    /// </summary>
    /// <param name="myFieldCards">自分のフィールド</param>
    /// <returns></returns>
    public IEnumerator StartSummon(Transform myFieldCards, GameObject summonCard)
    {
        //foreach (MeshRenderer m in renderers)
        //{
        //    m.shadowCastingMode = ShadowCastingMode.On;
        //}

        Vector3 forwardEndPosition;
        // 一番右のカード(最新の子オブジェクト)を取得
        // 1. 直前の座標に頼らず、インデックス（何枚目か）から計算する
        int childCount = myFieldCards.childCount - 1;

        // 2. フィールドの基準点（1枚目が入る場所）
        Vector3 baseCenter = AdjustCardPosition.Instance.fieldCenter;

        // 3. このカードが最終的に行くべき「ローカル座標」を計算
        // childCount枚目の位置 = 中心 + (ずらし量 * 枚数)
        forwardEndPosition = baseCenter + new Vector3(shiftOffset * childCount, 0, 0);

        // 画面中央まで上昇
        Vector3 upStartPosition = summonCard.transform.localPosition;
        yield return StartCoroutine(cardMotionHelper.MoveTarget(summonCard.transform, upTime, upStartPosition, upEndPosition, upCurve));

        yield return new WaitForSeconds(0.1f);

        // 前進しフィールドに置く
        Vector3 forwardStartPosition = summonCard.transform.localPosition;
        yield return StartCoroutine(cardMotionHelper.MoveTarget(summonCard.transform, forwardTime, forwardStartPosition, forwardEndPosition, forwardCurve));

        // 全てのエフェクトを少し上に上げる
        float yAxisOffset = 0.05f;
        Vector3 effectPosition = summonCard.transform.localPosition;
        effectPosition.z += yAxisOffset;

        // エフェクト取得
        EffectPoolManager effectPool = EffectPoolManager.Instance;

        GameObject flashObj = effectPool.Get(flashEffect.gameObject, effectPosition);
        ParticleSystem flashEffectInstance = flashObj.GetComponent<ParticleSystem>();

        GameObject smokeObj = effectPool.Get(smokeEffect.gameObject, effectPosition);
        ParticleSystem smokeEffectInstance = smokeObj.GetComponent<ParticleSystem>();

        GameObject rockObj = effectPool.Get(rockEffect.gameObject, effectPosition);
        ParticleSystem rockEffectInstance = rockObj.GetComponent<ParticleSystem>();

        Vector3 effectScale = new Vector3(0.05f, 0.05f, 0.05f);
        flashEffectInstance.transform.localScale = effectScale;
        smokeEffectInstance.transform.localScale = effectScale;
        rockEffectInstance.transform.localScale  = effectScale;

        //エフェクト再生
        flashEffectInstance.Play();
        smokeEffectInstance.Play();
        rockEffectInstance.Play();

        // 画面振動開始
        float elapsedTime = 0f;
        while (elapsedTime < shakeTime)
        {
            elapsedTime += Time.deltaTime;

            float t = elapsedTime / shakeTime;
            noise.m_AmplitudeGain = Mathf.Lerp(0.0f, amplitude, t);
            noise.m_FrequencyGain = Mathf.Lerp(0.0f, frequency, t);
            yield return null;
        }
        noise.m_AmplitudeGain = 5.0f;
        noise.m_FrequencyGain = 6.0f;

        // 画面振動を徐々に抑える
        elapsedTime = 0f;
        while (elapsedTime < shakeTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / shakeTime;
            noise.m_AmplitudeGain = Mathf.Lerp(amplitude, 0.0f, t);
            noise.m_FrequencyGain = Mathf.Lerp(frequency, 0.0f, t);
            yield return null;
        }
        noise.m_AmplitudeGain = 0.0f;
        noise.m_FrequencyGain = 0.0f;
    }
}
