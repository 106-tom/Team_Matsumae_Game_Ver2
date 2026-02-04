using Effekseer;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 呪文演出クラス
/// </summary>
public class Spell : MonoBehaviour
{
    [Header("表示したいカードの見た目を映すカメラ")]
    [SerializeField] private GameObject UICamera;
    [Space(10)]

    [Header("表示したいカードの見た目を保存する先")]
    [SerializeField] private RenderTexture renderTexture;
    [Space(10)]

    [Header("ディゾルブする際に邪魔なので非アクティブにする : 破壊用オブジェクト")]
    [SerializeField] private GameObject breakObject;
    [Space(10)]

    //[Header("実際に表示するカード")]
    //[SerializeField] private GameObject mainCardIDDisplay;
    //[Space(10)]

    [Header("エフェクト類")]
    [Tooltip("円エフェクト")]
    [SerializeField] private EffekseerEffectAsset lingEffect;
    [Tooltip("煙エフェクト")]
    [SerializeField] private ParticleSystem smokeEffect;
    [Tooltip("火球エフェクト")]
    [SerializeField] private ParticleSystem fireBallEffect;
    [Tooltip("爆発の閃光エフェクト")]
    [SerializeField] private ParticleSystem explosionFlashEffect;
    [Tooltip("爆発の炎エフェクト")]
    [SerializeField] private ParticleSystem explosionEmberEffect;
    [Tooltip("フォークスプライト")]
    [SerializeField] private GameObject forkSprite;
    [Space(10)]

    [Header("煙エフェクトの色")]
    [Tooltip("赤色カードの煙の色")]
    [SerializeField] private Color redSmokeColor;
    [Tooltip("緑色カードの煙の色")]
    [SerializeField] private Color greenSmokeColor;
    [Space(10)]

    [Header("カード移動")]
    [Tooltip("移動位置")]
    [SerializeField] private Vector3 moveEndPosition;
    [Tooltip("移動時間")]
    [SerializeField] private float moveTime;
    [Tooltip("移動速度変化グラフ")]
    [SerializeField] private AnimationCurve moveCurve;
    [Space(10)]

    [Header("ディゾルブ")]
    [Tooltip("ディゾルブ時間")]
    [SerializeField] private float dissolveTime;
    [Tooltip("ディゾルブ速度変化グラフ")]
    [SerializeField] private AnimationCurve dissolveCurve;
    [Space(10)]

   //[Header("カットイン")]
   //[SerializeField] private GameObject cutInAnim;
   //[Space(10)]
    private GameObject cutInAnim;
    private GameObject mainCardIDDisplay;

    private static readonly int thresholdID = Shader.PropertyToID("_Threshold");

    private GameObject glassPlate;
    private FkingCardFXManager cardFXManager;

    private CardEffectHelper cardEffectHelper;
    private CardMotionHelper cardMotionHelper;
    private UIHelper uiHelper;
    private CutIn cutIn;

    private void Start()
    {
        cardEffectHelper = SystemManager.Instance.cardEffectHelper;
        cardMotionHelper = SystemManager.Instance.cardMotionHelper;
        uiHelper = SystemManager.Instance.uiHelper;
        cardFXManager = GetComponent<FkingCardFXManager>();
    }

    public IEnumerator StartSpell(bool isPlayerTurn, Transform cardTransform)
    {
        Debug.Log("呪文演出開始");
        // カットインアニメーション開始
        cutInAnim = cardTransform.Find("CutInAnim").gameObject;
        cutIn = cutInAnim.GetComponent<CutIn>();
        yield return StartCoroutine(cutIn.StartCutIn(isPlayerTurn));

        // 移動の開始位置
        //Vector3 startPosition = transform.position;
        // 移動
        //yield return cardMotionHelper.MoveTarget(transform, moveTime, startPosition, moveEndPosition, moveCurve);
      
        mainCardIDDisplay = cardTransform.Find("GameObject/CardVisualHajime").gameObject;
        // ディゾルブ処理
        //yield return StartCoroutine(Dissolve(mainCardIDDisplay));
    }

    /// <summary>
    /// 赤色呪文演出開始
    /// </summary>
    /// <param name="enemyCard">呪文対象カード</param>
    /// <param name="isPlayerTurn">プレイヤーのターンかどうか</param>
    /// <returns></returns>
    public IEnumerator StartRedSpell(GameObject enemyCard, bool isPlayerTurn)
    {
        // カットインアニメーション開始
        yield return StartCoroutine(cutIn.StartCutIn(isPlayerTurn));

        // 移動の開始位置
        Vector3 startPosition = transform.position;
        // 移動
        yield return cardMotionHelper.MoveTarget(transform, moveTime, startPosition, moveEndPosition, moveCurve);

        // 回転の始点と終点を計算
        Quaternion startRotation = Quaternion.Euler(
            transform.rotation.eulerAngles.x,
            0f,
            transform.rotation.eulerAngles.z
        );
        float targetYAngle = cardMotionHelper.GetYAngleToTarget(transform.position, enemyCard.transform.position);
        Quaternion endRotation = Quaternion.Euler(
            transform.rotation.eulerAngles.x,
            targetYAngle,
            transform.rotation.eulerAngles.z
        );

        // エフェクトプール取得
        EffectPoolManager effectPool = EffectPoolManager.Instance;

        // 火球エフェクト取得
        GameObject fireBallObj = effectPool.Get(fireBallEffect.gameObject, transform.position);
        ParticleSystem fireBallEffectInstance = fireBallObj.GetComponent<ParticleSystem>();

        // 煙エフェクト取得
        GameObject smokeObj = effectPool.Get(smokeEffect.gameObject, transform.position);
        ParticleSystem smokeEffectInstance = smokeObj.GetComponent<ParticleSystem>();
        // 煙の色を変える
        var color = smokeEffectInstance.colorOverLifetime;

        // エフェクト再生
        Vector3 lingEffectPosition = new Vector3(transform.position.x, transform.position.y + 0.1f, transform.position.z);
        SetColorOverLifetime(color,redSmokeColor);
        EffekseerHandle handle = EffekseerSystem.PlayEffect(lingEffect, lingEffectPosition);
        fireBallEffectInstance.Play();
        smokeEffectInstance.Play();

        // 少し待機
        yield return new WaitForSeconds(0.1f);

        // 火球を敵カードへ放つ
        yield return StartCoroutine(cardMotionHelper.MoveTarget(fireBallEffectInstance.transform, 1.0f, fireBallEffectInstance.transform.position, enemyCard.transform.position));
        
        // 火球エフェクトをストップ
        fireBallEffectInstance.Stop();

        // 火球爆発エフェクト取得
        GameObject explosionFlashObj = effectPool.Get(explosionFlashEffect.gameObject, enemyCard.transform.position);
        ParticleSystem explosionFlashEffectInstance = explosionFlashObj.GetComponent<ParticleSystem>();

        // 火球燃焼エフェクト取得
        GameObject explosionEmberObj = effectPool.Get(explosionEmberEffect.gameObject, enemyCard.transform.position);
        ParticleSystem explosionEmberEffectInstance = explosionEmberObj.GetComponent<ParticleSystem>();

        // 爆発エフェクト再生
        explosionFlashEffectInstance.Play();
        explosionEmberEffectInstance.Play();

        // ディゾルブ処理
        //yield return StartCoroutine(Dissolve());
    }

    /// <summary>
    /// 緑色呪文演出開始
    /// </summary>
    /// <param name="enemyCard">呪文対象カード</param>
    /// <param name="isPlayerTurn">プレイヤーターンかどうか</param>
    /// <returns></returns>
    public IEnumerator StartGreenSpell(GameObject enemyCard, bool isPlayerTurn)
    {
        // カットインアニメーション開始
        yield return StartCoroutine(cutIn.StartCutIn(isPlayerTurn));

        // 移動の開始位置
        Vector3 startPosition = transform.position;
        // 移動
        yield return cardMotionHelper.MoveTarget(transform, moveTime, startPosition, moveEndPosition, moveCurve);

        // 煙エフェクト作成
        GameObject smokeObj = EffectPoolManager.Instance.Get(smokeEffect.gameObject, transform.position);
        ParticleSystem smokeEffectInstance = smokeObj.GetComponent<ParticleSystem>();

        // 煙の色を変える
        ParticleSystem.ColorOverLifetimeModule color = smokeEffectInstance.colorOverLifetime;
        SetColorOverLifetime(color, greenSmokeColor);

        // エフェクト再生
        Vector3 lingEffectPosition = new Vector3(transform.position.x, transform.position.y + 0.1f, transform.position.z);
        EffekseerHandle handle = EffekseerSystem.PlayEffect(lingEffect, lingEffectPosition);
        smokeEffectInstance.Play();

        // フォーク生成
        GameObject forkSpriteInstance = Instantiate(forkSprite, transform);

        // フォーク回転
        Quaternion startRotation = Quaternion.Euler(
            forkSpriteInstance.transform.rotation.eulerAngles.x,
            0f,
            forkSpriteInstance.transform.rotation.eulerAngles.z
        );
        float targetYAngle = cardMotionHelper.GetYAngleToTarget(forkSpriteInstance.transform.position, enemyCard.transform.position);
        Quaternion endRotation = Quaternion.Euler(
            forkSpriteInstance.transform.rotation.eulerAngles.x,
            targetYAngle,
            forkSpriteInstance.transform.rotation.eulerAngles.z
        );
        yield return StartCoroutine(cardMotionHelper.RotationTarget(forkSpriteInstance.transform, 0f, startRotation, endRotation));

        // フォークをフェードイン
        forkSpriteInstance.transform.position = transform.position;
        SpriteRenderer forkRenderer = forkSpriteInstance.GetComponent<SpriteRenderer>();
        StartCoroutine(uiHelper.SpriteFadeIn(forkRenderer, 0.2f));

        // 少し待機
        yield return new WaitForSeconds(0.1f);

        // フォークを敵カードへ放つ
        yield return StartCoroutine(cardMotionHelper.MoveTarget(forkSpriteInstance.transform, 0.3f, forkSpriteInstance.transform.position, enemyCard.transform.position));
        StartCoroutine(uiHelper.SpriteFadeOut(forkSpriteInstance.GetComponent<SpriteRenderer>(), 0.5f));
       
        // ディゾルブ処理
        //yield return StartCoroutine(Dissolve());

        // エフェクト削除
        Destroy(forkSpriteInstance);
    }

    /// <summary>
    /// ディゾルブ処理
    /// </summary>
    private IEnumerator Dissolve(GameObject mainCardIDDisplay)
    {
        // レンダーテクスチャの絵柄を設定
        GameObject camera;
        GameObject cardVisual;
        Material cardMaterial;
        cardEffectHelper.SettingRenderTecture(mainCardIDDisplay, UICamera, renderTexture, cardFXManager,
            out glassPlate, out camera, out cardVisual, out cardMaterial);

        // 邪魔なのでカードUIと破壊演出用オブジェクトを非表示にする
        mainCardIDDisplay.SetActive(false);
        //breakObject.SetActive(false);

        cardMaterial.SetFloat(thresholdID, 0.0f);

        // ディゾルブ開始
        float elapsedTime = 0f;
        while (elapsedTime < dissolveTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveTime;
            float curveValue = dissolveCurve.Evaluate(t);
            cardMaterial.SetFloat(thresholdID, curveValue);

            yield return null;
        }
        // プレート位置を元に戻す
        glassPlate.transform.localPosition = new Vector3(0f, 0f, -0.03f);

        // RenderTexture用カメラと表示用カードを削除
        Destroy(camera.gameObject);
        Destroy(cardVisual.gameObject);
    }

    /// <summary>
    /// ParticleのColorOverLifeTimeを調整する
    /// </summary>
    /// <param name="colModule">調整したいParticleのColorOverLifeTime</param>
    /// <param name="targetColor">始まりの色</param>
    private void SetColorOverLifetime(ParticleSystem.ColorOverLifetimeModule colModule, Color targetColor)
    {
        // 新しいGradientを作成
        Gradient newGradient = new Gradient();

        // 色（Color）の設定
        GradientColorKey[] colorKeys = new GradientColorKey[2];
        colorKeys[0] = new GradientColorKey(targetColor, 0.0f); // 寿命の0%地点
        colorKeys[1] = new GradientColorKey(Color.clear, 1.0f); // 寿命の100%地点

        // アルファ（透明度）の設定
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[3];
        alphaKeys[0] = new GradientAlphaKey(1.0f, 0.0f); // 寿命の0%地点
        alphaKeys[1] = new GradientAlphaKey(0.4f, 0.5f); // 寿命の100%地点
        alphaKeys[2] = new GradientAlphaKey(0.0f, 1.0f); // 寿命の100%地点

        // Gradientに設定を適用
        newGradient.SetKeys(colorKeys, alphaKeys);

        // Color over LifeTimeモジュールにGradientを適用
        // グラデーションをTimeによって制御するモードに設定
        colModule.color = new ParticleSystem.MinMaxGradient(newGradient);
    }
}
