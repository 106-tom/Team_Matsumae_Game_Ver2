using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using TMPro;
using Effekseer;
using Cinemachine;

// 視覚的なスクリプトの挙動を制御するコントローラー
[RequireComponent(typeof(MathHelper))] // 動作ヘルパー
// [RequireComponent(typeof(SpriteHelper))]
public class VisualCardController : MonoBehaviour
{
    // === 1. 静的(グローバル)な手札リストと参照 ===

    // 【重要】このリストは、シーン上の全てのカード間で共有されます。
    private static List<GameObject> AllHandCards = new List<GameObject>();

    // ★ 修正: 手札の基準位置をInspectorで設定可能にする ★
    [Header("--- 手札アンカー設定 ---")]
    [Tooltip("手札の中心位置となるTransform。このTransformの位置が基準になります。")]
    public Transform HandAnchor;

    // === 2. ドロー用プロパティ (DeckManager連携用も含む) ===

    [Header("--- 0. ドロー設定 ---")]
    [Tooltip("全カードデータが登録されたFkingCardDatabaseオブジェクト")]
    public FkingCardDatabase cardDatabase; // DeckManagerの代わりにここで参照
    [Tooltip("山札オブジェクト (ドロー開始位置)")]
    public Transform Deck;
    [Tooltip("生成するカードのPrefab")]
    public GameObject NewCardPrefab;

    [Tooltip("ドロー時の移動アニメーション曲線")]
    public AnimationCurve Curve;
    [Tooltip("ドロー時の回転アニメーション時間")]
    public float RotateTime = 2f;
    [Tooltip("手札カード間の間隔")]
    public float CardToCardLength = 0.6f;
    [SerializeField] private float drawMoveTime = 0.5f; // 移動の総時間 (Curve評価用)


    // === 3. 既存のプロパティと参照 ===

    // --- 必須コンポーネント参照 ---
    private MathHelper mathHelper;
    private SpriteHelper spriteHelper;
    private MeshRenderer meshRenderer;
    private Material cardMaterial;
    private FkingCardData currentCardData; // FkingCardDisplayで使用するデータ

    // --- 動作コンポーネント (Inspectorで設定) ---
    [Header("--- 汎用・UI ---")]
    [Tooltip(@"Guardテキスト表示用のUIキャンバス")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject worldSpaceUI;

    // --- Attack ---
    [Header("--- 1. Attack ---")]
    [SerializeField] private float attack_forwardTime = 0.5f;
    [SerializeField] private AnimationCurve attack_forwardCurve;
    [SerializeField] private float attack_upTime = 0.3f;
    [SerializeField] private float attack_upOffset = 0.5f;
    [SerializeField] private AnimationCurve attack_upCurve;
    [SerializeField] private float attack_downTime = 0.3f;
    [SerializeField] private AnimationCurve attack_downCurve;
    [SerializeField] private float attack_rotateTime = 0.2f;
    [SerializeField] private AnimationCurve attack_rotateCurve;

    // --- Break ---
    [Header("--- 2. Break ---")]
    [SerializeField] private CinemachineVirtualCamera break_virtualCamera;
    [SerializeField] private GameObject break_cardPrefab;
    [SerializeField] private ParticleSystem break_flashEffect;
    [SerializeField] private EffekseerEffectAsset break_lingEffect;
    [SerializeField] private Vector3 break_explodeVel = new Vector3(0, 0, 0.1f);
    [SerializeField] private float break_explodeForce = 200f;
    [SerializeField] private float break_explodeRange = 10f;
    [SerializeField] private float break_shakeTime = 0.5f;
    [SerializeField] private float break_amplitude = 5.0f;
    [SerializeField] private float break_frequency = 6.0f;
    [SerializeField] private float break_dissolveTime = 1.0f;
    [SerializeField] private float break_uiScale = 0.1f;
    [SerializeField] private float break_uiFadeOutTime = 0.5f;

    // --- Change Texture ---
    [Header("--- 3. Change Texture ---")]
    [SerializeField] private Texture change_newTexture;

    // --- Guard ---
    [Header("--- 4. Guard ---")]
    [SerializeField] private EffekseerEffectAsset guard_whiteLing;
    [SerializeField] private EffekseerEffectAsset guard_blackLing;
    [SerializeField] private AnimationCurve guard_rotateCurve;
    [SerializeField] private TextMeshProUGUI guard_textPrefab;
    [SerializeField] private float guard_fadeOutTime = 0.4f;
    [SerializeField] private float guard_scalingTime = 0.3f;
    [SerializeField] private float guard_rotationTime = 0.3f;

    // --- Heal ---
    [Header("--- 5. Heal ---")]
    [SerializeField] private float heal_upTime = 0.5f;
    [SerializeField] private AnimationCurve heal_upCurve;
    [SerializeField] private float heal_downTime = 0.5f;
    [SerializeField] private AnimationCurve heal_downCurve;
    [SerializeField] private float heal_rotateTime = 0.3f;
    [SerializeField] private AnimationCurve heal_rotateCurve;

    // --- Magic ---
    [Header("--- 6. Magic ---")]
    [SerializeField] private ParticleSystem magic_explosionFlashEffect;
    [SerializeField] private ParticleSystem magic_explosionEmberEffect;
    [SerializeField] private ParticleSystem magic_fireBallEffect;
    [SerializeField] private AnimationCurve magic_curve;
    [SerializeField] private float magic_time = 1.0f;

    // --- Summon ---
    [Header("--- 7. Summon ---")]
    [SerializeField] private ParticleSystem summon_flashEffect;
    [SerializeField] private ParticleSystem summon_smokeEffect;
    [SerializeField] private ParticleSystem summon_rockEffect;
    [SerializeField] private CinemachineVirtualCamera summon_virtualCamera;
    [SerializeField] private float summon_amplitude = 5.0f;
    [SerializeField] private float summon_frequency = 6.0f;
    [SerializeField] private float summon_shakeTime = 0.5f;
    [SerializeField] private Vector3 summon_upEndPosition = new Vector3(0, 1.5f, -0.5f);
    [SerializeField] private float summon_upTime = 0.5f;
    [SerializeField] private AnimationCurve summon_upCurve;
    [SerializeField] private float summon_forwardTime = 0.5f;
    [SerializeField] private AnimationCurve summon_forwardCurve;
    [SerializeField] private float summon_shiftOffset = 0.5f;


    // =================================================================
    // --- 初期化 ---
    // =================================================================

    void Awake()
    {
        // 必須コンポーネントを取得
        mathHelper = GetComponent<MathHelper>();
        spriteHelper = GetComponent<SpriteHelper>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer != null)
        {
            cardMaterial = meshRenderer.material;
        }

        // ★ 修正: HandAnchorが設定されていなければ、このオブジェクトを基準として使う ★
        if (HandAnchor == null)
        {
            HandAnchor = this.transform;
            Debug.LogWarning("Hand Anchorが設定されていません。このオブジェクトの位置を手札の基準点とします。", this);
        }

        // --- NULLチェック (警告) ---
        if (mathHelper == null) Debug.LogError("MathHelperが見つかりません！", this);
        if (spriteHelper == null) Debug.LogWarning("SpriteHelperが見つかりません (Guardで必須)", this);
    }

    // =================================================================
    // --- ドロー機能 (手札全体の管理) ---
    // =================================================================

    /// <summary>
    /// 【外部呼び出し用】山札エリアにカードを生成し、手札に移動させます。
    /// </summary>
    public void DrawCard(string cardID)
    {
        if (cardDatabase == null || NewCardPrefab == null || Deck == null || mathHelper == null)
        {
            Debug.LogError("ドロー設定、またはCardDatabaseが不足しています。");
            return;
        }

        // 0. カードデータと生成するカード情報を取得CARD
        FkingCardData dataToDisplay = cardDatabase.GetCardByID(cardID);
        if (dataToDisplay == null)
        {
            Debug.LogError($"ID: {cardID} のカードデータが見つかりません。");

            return;
        }

        // 1. 山札エリアにカードを生成する
        GameObject newCardObject = Instantiate(NewCardPrefab, Deck.position, Deck.rotation);

        // 2. 生成したカードにデータを適用
        FkingCardDisplay cardDisplay = newCardObject.GetComponent<FkingCardDisplay>();
        VisualCardController newCardVC = newCardObject.GetComponent<VisualCardController>();

        if (cardDisplay != null)
        {
            // FkingCardDisplayAttacherのロジックを模倣して、カード情報をUIに反映
            cardDisplay.FkingUpdateDisplay(dataToDisplay);
            newCardVC.currentCardData = dataToDisplay; // 生成されたカードのVCにもデータを保持させる
        }

        // 3. 生成したカードを手札リストに追加し、親をHandAnchorに設定
        newCardObject.transform.SetParent(HandAnchor, true); // ★ HandAnchorを親に設定 ★
        AllHandCards.Add(newCardObject);

        // 4. 手札への移動と整列を開始
        if (newCardVC != null)
        {
            StartCoroutine(newCardVC.AnimateDrawAndAdjustHand(newCardObject.transform, Deck.rotation));
        }
        else
        {
            Debug.LogError("生成されたカードにVisualCardControllerが付いていません！");
        }
    }

    /// <summary>
    /// カードを手札に移動させ、全ての手札を再整列するコルーチン
    /// </summary>
    private IEnumerator AnimateDrawAndAdjustHand(Transform cardTransform, Quaternion startRotation)
    {
        int cardIndex = AllHandCards.Count - 1; // 常に末尾のカード

        // 1. 新しいカードの最終目標位置と回転を計算
        Vector3 targetPosition = CalculateHandPosition(cardIndex);
        Quaternion targetRotation = Quaternion.Euler(0, 0, 0); // 手札でのデフォルトの回転

        // --- 移動と回転アニメーションを並行して実行 ---

        // 回転アニメーション: 山札の回転から手札の回転へ
        StartCoroutine(mathHelper.Rotation(RotateTime, startRotation, targetRotation, Curve));

        // 移動アニメーション: 山札の位置から手札の最終位置へ
        yield return StartCoroutine(mathHelper.MoveTarget(cardTransform, drawMoveTime, cardTransform.position, targetPosition, Curve));

        // 最後に位置と回転を確実に補正 (アニメーションの精度保証)
        cardTransform.position = targetPosition;
        cardTransform.rotation = targetRotation;

        // 2. 全ての手札を再整列
        AdjustHandPositions();
    }

    /// <summary>
    /// 手札の全てのカードを現在の枚数に応じて整列させる（瞬時）
    /// </summary>
    public void AdjustHandPositions()
    {
        int count = AllHandCards.Count;
        if (count == 0) return;

        for (int i = 0; i < count; i++)
        {
            Transform cardTransform = AllHandCards[i].transform;
            Vector3 targetPos = CalculateHandPosition(i);

            // 瞬時の位置補正
            cardTransform.position = targetPos;
            cardTransform.rotation = Quaternion.Euler(0, 0, 0); // 回転もリセット
        }
    }

    /// <summary>
    /// インデックスに基づいたカードのワールドポジションを計算
    /// </summary>
    private Vector3 CalculateHandPosition(int index)
    {
        int count = AllHandCards.Count;

        // 総幅とXオフセットを計算
        float totalWidth = (count - 1) * CardToCardLength;
        float xOffset = index * CardToCardLength - totalWidth / 2f;

        // HandAnchorの位置を基準にする
        return HandAnchor.position + new Vector3(xOffset, 0, 0); // ★ HandAnchor.positionを使用 ★
    }


    // =================================================================
    // --- 既存アクションロジック ---
    // =================================================================

    // FkingCardDisplayで使用するメソッドは省略
    // public void FkingUpdateDisplay(FkingCardData data) { /* ... */ }
    // public void TriggerPostSummonState(int currentAP, int currentBP) { /* ... */ }

    #region 1. Attack Logic
    private IEnumerator AttackCoroutine(Transform target)
    {
        // ... (省略: 攻撃ロジック)
        yield break;
    }
    #endregion

    // Break時: 破壊されたカードを静的リストから削除し、手札を再調整する
    #region 2. Break Logic
    private IEnumerator BreakCoroutine()
    {
        Rigidbody[] rigidBodies;
        Material[] materials;
        MeshRenderer[] renderers;

        // エラー修正: コルーチン全体でスコープを持つように宣言
        CinemachineVirtualCamera virtualCameraInstance = null;
        ParticleSystem flashEffectInstance = null;

        // ... (省略: カードテクスチャ取得、BreakObject生成、UI設定、BreakObjectを真っ黒にする、UIフェードアウト、本体非表示のロジック)
        Texture originalTexture = null;
        if (this.cardMaterial != null) originalTexture = this.cardMaterial.GetTexture("_MainTex");

        Quaternion rotation = Quaternion.Euler(0f, 180f, 0f);
        GameObject breakCardInstance = Instantiate(break_cardPrefab, transform.position, rotation);
        rigidBodies = breakCardInstance.GetComponentsInChildren<Rigidbody>();
        renderers = breakCardInstance.GetComponentsInChildren<MeshRenderer>();

        if (worldSpaceUI != null && rigidBodies.Length > 0)
        {
            worldSpaceUI.transform.SetParent(rigidBodies[0].transform, false);
            worldSpaceUI.transform.localScale = Vector3.one * break_uiScale;
            worldSpaceUI.transform.localPosition = Vector3.zero;
        }

        materials = renderers.Select(r => r.material).ToArray();
        foreach (Material m in materials)
        {
            if (m.HasProperty("_Color")) m.SetColor("_Color", Color.black);
            if (m.HasProperty("_MainTex")) m.SetTexture("_MainTex", null);
        }

        CanvasGroup worldSpaceUICanvasGroup = null;
        if (worldSpaceUI != null)
        {
            worldSpaceUICanvasGroup = worldSpaceUI.GetComponent<CanvasGroup>();
            if (worldSpaceUICanvasGroup == null) worldSpaceUICanvasGroup = worldSpaceUI.AddComponent<CanvasGroup>();
            StartCoroutine(FadeOutWorldSpaceUI(worldSpaceUICanvasGroup));
        }

        if (meshRenderer != null) meshRenderer.enabled = false;

        // 画面揺れ用カメラとフラッシュエフェクトを生成
        virtualCameraInstance = Instantiate(break_virtualCamera);
        CinemachineBasicMultiChannelPerlin noiseComponent =
            virtualCameraInstance.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        flashEffectInstance = Instantiate(
                break_flashEffect,
                transform.position,
                Quaternion.identity);


        // エフェクト再生
        Vector3 lingEffectPosition = new Vector3(transform.position.x, transform.position.y + 0.1f, transform.position.z);
        EffekseerHandle handle = EffekseerSystem.PlayEffect(break_lingEffect, lingEffectPosition);
        flashEffectInstance.Play();

        // 待機
        if (worldSpaceUICanvasGroup != null) yield return new WaitForSeconds(break_uiFadeOutTime);
        else yield return new WaitForSeconds(0.1f);

        // カードをバラバラにする
        foreach (Rigidbody rb in rigidBodies)
        {
            rb.isKinematic = false;
            rb.AddExplosionForce(break_explodeForce / 5, transform.position + break_explodeVel, break_explodeRange);
            rb.AddExplosionForce(break_explodeForce, transform.position + break_explodeVel, break_explodeRange);
        }

        // ... (省略: 画面揺れ開始・停止ロジック)
        // 画面揺れロジック...
        float elapsedTime = 0f;
        while (elapsedTime < break_shakeTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / break_shakeTime;
            noiseComponent.m_AmplitudeGain = Mathf.Lerp(0.0f, break_amplitude, t);
            noiseComponent.m_FrequencyGain = Mathf.Lerp(0.0f, break_frequency, t);
            yield return null;
        }
        noiseComponent.m_AmplitudeGain = break_amplitude;
        noiseComponent.m_FrequencyGain = break_frequency;

        elapsedTime = 0f;
        while (elapsedTime < break_shakeTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / break_shakeTime;
            noiseComponent.m_AmplitudeGain = Mathf.Lerp(break_amplitude, 0.0f, t);
            noiseComponent.m_FrequencyGain = Mathf.Lerp(break_frequency, 0.0f, t);
            yield return null;
        }
        noiseComponent.m_AmplitudeGain = 0f;
        noiseComponent.m_FrequencyGain = 0f;
        // 画面揺れロジックここまで

        yield return new WaitForSeconds(0.8f);

        // ディゾルブ開始
        elapsedTime = 0f;
        while (elapsedTime < break_dissolveTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / break_dissolveTime;
            foreach (Material m in materials)
            {
                m.SetFloat("_Threshold", Mathf.Lerp(0.0f, 1.0f, t));
            }
            yield return null;
        }

        // ★ 手札管理の修正 ★
        AllHandCards.Remove(gameObject);
        AdjustHandPositions();

        // 本体（このカードのGameObject）を削除
        Destroy(gameObject);
        // 破片の親（breakCardInstance）を削除
        Destroy(breakCardInstance);

        // ★ エラー修正: 宣言された変数が null でないかチェックしてから Destroy
        if (flashEffectInstance != null) Destroy(flashEffectInstance.gameObject, flashEffectInstance.main.duration);
        if (virtualCameraInstance != null) Destroy(virtualCameraInstance.gameObject);
    }
    #endregion

    #region 3. Change Texture Logic
    private IEnumerator ChangeTextureCoroutine()
    {
        // ... (省略)
        yield break;
    }
    #endregion

    #region 4. Guard Logic
    private IEnumerator StartGuardTextFade(TextMeshProUGUI guardTextInstance)
    {
        // ... (省略)
        yield break;
    }

    private IEnumerator GuardCoroutine(Transform target)
    {
        // ... (省略)
        yield break;
    }
    #endregion

    #region 5. Heal Logic
    private IEnumerator PlayShineEffect(float duration)
    {
        // ... (省略)
        yield break;
    }

    private IEnumerator HealCoroutine()
    {
        // ... (省略)
        yield break;
    }
    #endregion

    // Magic時: マジックカードを静的リストから削除し、手札を再調整する
    #region 6. Magic Logic
    private IEnumerator MagicCoroutine(Transform target)
    {
        // エラー修正: 変数宣言をコルーチンの冒頭で行う
        ParticleSystem fireBallEffectInstance = null;
        ParticleSystem explosionFlashEffectInstance = null;
        ParticleSystem explosionEmberEffectInstance = null;

        Vector3 startPosition = transform.position;

        // 画面中央の位置を取得
        Vector3 endPosition = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        endPosition = Camera.main.ScreenToWorldPoint(endPosition);
        endPosition.y -= 3.0f;
        endPosition.z += 1.0f;

        // 中央へ移動
        yield return mathHelper.Move(magic_time, startPosition, endPosition, magic_curve);

        // 攻撃対象を取得
        GameObject enemyCard = target.gameObject;

        // 回転の始点と終点を計算
        Quaternion startRotation = Quaternion.Euler(
            transform.rotation.eulerAngles.x,
            0f,
            transform.rotation.eulerAngles.z
        );
        float targetYAngle = mathHelper.GetYAngleToTarget(transform.position, enemyCard.transform.position);
        Quaternion endRotation = Quaternion.Euler(
            transform.rotation.eulerAngles.x,
            targetYAngle,
            transform.rotation.eulerAngles.z
        );

        // 炎エフェクト生成
        fireBallEffectInstance = Instantiate(
                magic_fireBallEffect,
                transform.position,
                endRotation);
        fireBallEffectInstance.Play();

        yield return new WaitForSeconds(0.1f);

        // 炎を敵カードに向けて発射
        yield return StartCoroutine(mathHelper.MoveTarget(fireBallEffectInstance.transform, 1.0f, fireBallEffectInstance.transform.position, enemyCard.transform.position));

        // 炎エフェクトをストップ
        fireBallEffectInstance.Stop();

        // 爆発エフェクト開始
        explosionFlashEffectInstance = Instantiate(
              magic_explosionFlashEffect,
              enemyCard.transform.position,
              Quaternion.identity);
        explosionEmberEffectInstance = Instantiate(
              magic_explosionEmberEffect,
              enemyCard.transform.position,
              Quaternion.identity);

        // 爆発エフェクト再生
        explosionFlashEffectInstance.Play();
        explosionEmberEffectInstance.Play();

        // ディゾルブ開始 (このカードを消す)
        cardMaterial.SetFloat("_Threshold", 0.0f);
        float elapsedTime = 0f;
        while (elapsedTime < magic_time)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / magic_time;
            float curveValue = magic_curve.Evaluate(t);
            cardMaterial.SetFloat("_Threshold", curveValue);

            yield return null;
        }

        // ★ 手札管理の修正 ★
        AllHandCards.Remove(gameObject);
        AdjustHandPositions();

        // 本体(マジックカード)とエフェクト削除
        Destroy(gameObject);
        if (explosionFlashEffectInstance != null) Destroy(explosionFlashEffectInstance.gameObject, 3f);
        if (explosionEmberEffectInstance != null) Destroy(explosionEmberEffectInstance.gameObject, 3f);
        if (fireBallEffectInstance != null) Destroy(fireBallEffectInstance.gameObject, 3f);
    }
    #endregion

    // Summon時: 召喚されたカードを静的リストから削除し、手札を再調整する
    #region 7. Summon Logic
    private IEnumerator SummonCoroutine()
    {
        // エラー修正: 変数宣言をコルーチンの冒頭で行う
        CinemachineVirtualCamera virtualCameraInstance = null;
        ParticleSystem flashEffectInstance = null;
        ParticleSystem smokeEffectInstance = null;
        ParticleSystem rockEffectInstance = null;

        // ★ 手札管理の修正 ★
        AllHandCards.Remove(gameObject);
        AdjustHandPositions();

        // フィールドに移動するため影を落とす
        meshRenderer.shadowCastingMode = ShadowCastingMode.On;

        // 画面揺れ用カメラを取得
        virtualCameraInstance = Instantiate(summon_virtualCamera);
        CinemachineBasicMultiChannelPerlin noiseComponent =
            virtualCameraInstance.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        //フィールドカード(親オブジェクト)を取得
        GameObject fieldCards = GameObject.FindGameObjectWithTag("Field");

        Vector3 forwardEndPosition;

        if (fieldCards != null && fieldCards.transform.childCount > 0)
        {
            Transform newestChild = fieldCards.transform.GetChild(fieldCards.transform.childCount - 1);

            // このカードをフィールドカードの子オブジェクトに設定
            transform.parent = fieldCards.transform;

            // 前進動作の終点を設定（最後のカードの隣）
            forwardEndPosition = newestChild.position + new Vector3(summon_shiftOffset, 0, 0);

            Vector3 shiftWidth = new Vector3(summon_shiftOffset / 2.0f, 0f, 0f);
            Vector3 startPosition = fieldCards.transform.position;
            Vector3 endPosition = fieldCards.transform.position - shiftWidth;
            StartCoroutine(mathHelper.MoveTarget(fieldCards.transform, summon_upTime, startPosition, endPosition, summon_upCurve));
        }
        else if (fieldCards != null)
        {
            transform.parent = fieldCards.transform;
            forwardEndPosition = Vector3.zero;
        }
        else
        {
            forwardEndPosition = Vector3.zero;
        }

        // 画面中央まで浮き上がり
        Vector3 upStartPosition = transform.position;
        yield return StartCoroutine(mathHelper.Move(summon_upTime, upStartPosition, summon_upEndPosition, summon_upCurve));

        // 前進しフィールドに配置
        Vector3 forwardStartPosition = transform.position;
        yield return StartCoroutine(mathHelper.Move(summon_forwardTime, forwardStartPosition, forwardEndPosition, summon_forwardCurve));

        float yAxisOffset = 0.1f;
        Vector3 effectPosition = transform.position;
        effectPosition.y += yAxisOffset;

        // エフェクト生成・再生
        flashEffectInstance = Instantiate(summon_flashEffect, effectPosition, Quaternion.identity);
        smokeEffectInstance = Instantiate(summon_smokeEffect, effectPosition, Quaternion.identity);
        rockEffectInstance = Instantiate(summon_rockEffect, effectPosition, Quaternion.identity);

        smokeEffectInstance.Play();
        rockEffectInstance.Play();

        // 画面揺れロジック
        float elapsedTime = 0f;
        // 画面揺れロジック...

        elapsedTime = 0f;
        while (elapsedTime < summon_shakeTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / summon_shakeTime;
            noiseComponent.m_AmplitudeGain = Mathf.Lerp(0.0f, summon_amplitude, t);
            noiseComponent.m_FrequencyGain = Mathf.Lerp(0.0f, summon_frequency, t);
            yield return null;
        }
        noiseComponent.m_AmplitudeGain = summon_amplitude;
        noiseComponent.m_FrequencyGain = summon_frequency;

        elapsedTime = 0f;
        while (elapsedTime < summon_shakeTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / summon_shakeTime;
            noiseComponent.m_AmplitudeGain = Mathf.Lerp(summon_amplitude, 0.0f, t);
            noiseComponent.m_FrequencyGain = Mathf.Lerp(summon_frequency, 0.0f, t);
            yield return null;
        }
        noiseComponent.m_AmplitudeGain = 0.0f;
        noiseComponent.m_FrequencyGain = 0.0f;
        // 画面揺れロジックここまで

        // エフェクト削除
        if (virtualCameraInstance != null) Destroy(virtualCameraInstance.gameObject, 1.0f);
        if (flashEffectInstance != null) Destroy(flashEffectInstance.gameObject, 3.0f);
        if (smokeEffectInstance != null) Destroy(smokeEffectInstance.gameObject, 3.0f);
        if (rockEffectInstance != null) Destroy(rockEffectInstance.gameObject, 3.0f);
    }
    #endregion

    // worldSpaceUIをフェードアウトさせるコルーチン
    private IEnumerator FadeOutWorldSpaceUI(CanvasGroup canvasGroup)
    {
        if (canvasGroup == null) yield break;

        float elapsedTime = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsedTime < break_uiFadeOutTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / break_uiFadeOutTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }
}