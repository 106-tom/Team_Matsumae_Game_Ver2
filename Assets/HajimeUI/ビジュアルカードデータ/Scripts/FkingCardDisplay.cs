using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections; // Listを使うために必要

// FkingCostUIElement クラス
[System.Serializable]
public class FkingCostUIElement
{
    public GameObject group;
    public TextMeshProUGUI text;
}

// どの色の、どのフレーム(通常/バトル)かを設定するためのペア設定
[System.Serializable]
public class FkingColorFrameSet
{
    [Tooltip("この設定が対応するマナの色")]
    public FkingColorType color; // FkingCardDataのenumと合わせる

    [Tooltip("通常状態のフレーム (手札や永続効果の時)")]
    public GameObject normalFrame;

    [Tooltip("バトル状態のフレーム")]
    public GameObject battleFrame;
}


public class FkingCardDisplay : MonoBehaviour
{
    // --- 基本UI要素 ---
    [Header("基本UIの参照")]
    public TextMeshProUGUI nameText;
    public Image artworkImage; // 通常イラスト (CardIllust)
    [Tooltip("KeyCardIllust用のイメージ")]
    public Image keyCardIllustImage; // バトル用イラスト (KeyCardIllust)
    public TextMeshProUGUI statsText;      // 通常Stats (AP/BP)
    public TextMeshProUGUI effectText;     // 通常Effect

    // ▼▼▼ 修正点 (テキスト背景画像を追加) ▼▼▼
    [Tooltip("効果テキストの背景画像")]
    public Image textBackgroundImage; // テキストの背景
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

    [Header("カラーフレーム参照")]
    [Tooltip("カードの色に対応するフレームのセットをリストで設定します")]
    public List<FkingColorFrameSet> colorFrameSets = new List<FkingColorFrameSet>();

    [Header("バトルステータス参照")]
    public GameObject battleStatsGroup;    // "BattleStats" の親GameObject
    public TextMeshProUGUI battleAPText;    // "APCount" の Text
    public TextMeshProUGUI battleBPText;    // "BPCount" の Text

    [Header("コストUIの参照")]
    public FkingCostUIElement genericCostUI;
    public FkingCostUIElement redCostUI;
    public FkingCostUIElement greenCostUI;
    public FkingCostUIElement blueCostUI;
    public FkingCostUIElement yellowCostUI;
    public FkingCostUIElement purpleCostUI;
    public FkingCostUIElement rainbowCostUI;

    // ▼▼▼ 修正点 (KeyCard時の色設定を追加) ▼▼▼
    [Header("KeyCard 色設定")]
    [Tooltip("KeyCard時のテキスト色 (例: 白)")]
    public Color keyCardTextColor = Color.white;
    [Tooltip("KeyCard時のテキスト背景色 (例: 黒の半透明)")]
    public Color keyCardBackgroundColor = new Color(0f, 0f, 0f, 0.5f); // 50%の黒
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

    [Header("背景Quad")]
    [Tooltip("BattleFrameになったときに縮小する背景のQuad（Transform）")]
    public Transform backgroundQuad;

    [Header("BattleFrame設定")]
    [Tooltip("BattleFrame時の背景Quadのスケール（デフォルト: 1.0）")]
    [SerializeField] private float battleFrameScale = 0.9f;

    private FkingCardData currentCardData;
    private Vector3 originalBackgroundScale;
    private bool originalBackgroundScaleInitialized = false;

    // ▼▼▼ 修正点 (元の色を記憶する変数を追加) ▼▼▼
    private Color originalTextColor;
    private Color originalBackgroundColor;
    private bool originalColorsInitialized = false;

	public Sprite backSprite;  // Inspector で裏面画像をセット


	public static FkingCardDisplay Instance;

	void Awake()
	{
		Instance = this;
	}
	// ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲


	//battlestatsを表示する

	// --- FkingUpdateDisplay (手札表示 / 初期表示) ---
	public void FkingUpdateDisplay(FkingCardData data)
    {
        if (data == null) { Debug.LogError("データ無し"); return; }
        this.currentCardData = data;

        // --- 基本情報の更新 ---
        if (nameText != null) nameText.text = data.cardName;
        if (effectText != null) effectText.text = data.effectText;
        if (artworkImage != null) artworkImage.sprite = data.cardImage;
        if (keyCardIllustImage != null) keyCardIllustImage.sprite = data.keyCardImage;

        // --- 状態の切り替え: 【手札・通常表示モード】に設定 ---
        SetHandState(); // この中でフレームとイラストの表示/非表示も行われる

        //if (statsText != null)
        //{
        //    if (data.cardType == "ユニットスター")
        //    {
        //        statsText.gameObject.SetActive(true);
        //        statsText.text = $"AP {data.ap} / BP {data.bp}";
        //    }
        //    else
        //    {
        //        statsText.gameObject.SetActive(false);
        //    }
        //}

        UpdateCostDisplay(data);
    }

    // --- 召喚時に呼び出されるメソッド (TriggerPostSummonState) ---
    public void TriggerPostSummonState(int currentAP, int currentBP)
    {
        if (currentCardData == null) return;

        // --- 1. コストと通常Statsを非表示 ---
        HideAllCostUI();
        //if (statsText != null) statsText.gameObject.SetActive(false);

        if (currentCardData.hasPostSummonEffect == false)
        {
            // --- 2a. 永続効果を持たない場合 (バトルフレームへ) ---
            SetBattleFrameMode(currentAP, currentBP);
        }
        else
        {
            // --- 2b. 永続効果を持つ場合 (永続効果表示フレームへ) ---
            SetPersistentEffectMode();
        }
    }


    // -----------------------------------------------------------------
    // ★ 状態遷移ロジック (privateに戻すことで外部からの不正な変更を防ぐ) ★
    // -----------------------------------------------------------------

    // ▼▼▼ 修正点 (元の色を初期化する関数を追加) ▼▼▼
    /// <summary>
    /// 実行時に一度だけ、テキストと背景の元の色を記憶する
    /// </summary>
    private void InitializeOriginalColors()
    {
        if (originalColorsInitialized) return;

        if (effectText != null)
        {
            originalTextColor = effectText.color;
        }
        if (textBackgroundImage != null)
        {
            originalBackgroundColor = textBackgroundImage.color;
        }
        originalColorsInitialized = true;
    }
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

    /// <summary>
    /// UIを【手札・通常表示モード】にします
    /// </summary>
    private void SetHandState()
    {
        InitializeOriginalColors(); // 元の色を記憶

        // フレーム切り替え
        UpdateFrameDisplay(false); // 'false' = バトルモードではない

        // イラスト切り替え (手札では常に通常イラスト)
        if (artworkImage != null) artworkImage.gameObject.SetActive(true);
        if (keyCardIllustImage != null) keyCardIllustImage.gameObject.SetActive(false);

        // テキスト/Stats設定
        if (effectText != null) effectText.gameObject.SetActive(true);
        if (battleStatsGroup != null) battleStatsGroup.SetActive(false);

        // ▼▼▼ 修正点 (テキスト背景表示 & 色リセット) ▼▼▼
        if (textBackgroundImage != null)
        {
            textBackgroundImage.gameObject.SetActive(true);
            textBackgroundImage.color = originalBackgroundColor; // 元の背景色
        }
        if (effectText != null)
        {
            effectText.color = originalTextColor; // 元のテキスト色
        }
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

        // 背景Quadのスケールを元に戻す
        if (backgroundQuad != null)
        {
            // (中略 ... backgroundQuadのスケールリセット処理 ...)
            if (!originalBackgroundScaleInitialized)
            {
                originalBackgroundScale = backgroundQuad.localScale;
                originalBackgroundScaleInitialized = true;
            }
            backgroundQuad.localScale = originalBackgroundScale;
        }
    }

    /// <summary>
    /// UIを【バトルフレームモード】（効果非表示、AP/BP表示）にします
    /// </summary>
    private void SetBattleFrameMode(int currentAP, int currentBP)
    {
        InitializeOriginalColors(); // 元の色を記憶

        // 状態の初期化
        HideAllCostUI();
        //if (statsText != null) statsText.gameObject.SetActive(false);

        // フレーム切り替え
        UpdateFrameDisplay(true); // 'true' = バトルモードである

        // ▼▼▼ 修正点 (テキストと背景を非表示) ▼▼▼
        // (hasPostSummonEffect == false の時に呼ばれるため)
        if (effectText != null) effectText.gameObject.SetActive(false);
        if (textBackgroundImage != null) textBackgroundImage.gameObject.SetActive(false);
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

        // イラスト切り替え
        if (currentCardData != null && artworkImage != null && keyCardIllustImage != null)
        {
            if (currentCardData.hasKeyCardIllust)
            {
                artworkImage.gameObject.SetActive(false);
                keyCardIllustImage.gameObject.SetActive(true);
            }
            else
            {
                artworkImage.gameObject.SetActive(true);
                keyCardIllustImage.gameObject.SetActive(false);
            }
        }

        // バトルStats表示
        if (battleStatsGroup != null)
        {
            battleStatsGroup.SetActive(true);
            //if (battleAPText != null) battleAPText.text = FieldCardDisplayAI.Instance.Attack.ToString();
            //if (battleBPText != null) battleBPText.text = FieldCardDisplayAI.Instance.Defense.ToString();
        }

        // (中略 ... 背景Quadのスケール縮小処理 ...)
        if (backgroundQuad != null)
        {
            if (!originalBackgroundScaleInitialized)
            {
                originalBackgroundScale = backgroundQuad.localScale;
                originalBackgroundScaleInitialized = true;
            }
            backgroundQuad.localScale = originalBackgroundScale * battleFrameScale;
        }
    }

	public void ShowbattleStats()
	{
        battleStatsGroup.SetActive(true);
	}

	// FkingCardDisplay.cs
	//public IEnumerator ShowStats(FieldCardDisplayAI fieldCard)
	//{
	//	Debug.Log("アクティブ待機");
	//	yield return new WaitUntil(() => this.battleStatsGroup != null);
	//
	//	battleStatsGroup.SetActive(true);
	//
	//	if (battleAPText != null) battleAPText.text = fieldCard.Attack.ToString();
	//	if (battleBPText != null) battleBPText.text = fieldCard.Defense.ToString();
	//
	//	Debug.Log("Statsアクティブ化");
	//}

	public IEnumerator ShowStats(FieldCardDisplayAI fieldCard)
	{
		// fieldCard の子にある battleStatsGroup を取得
		FkingCardDisplay fkd = fieldCard.GetComponentInChildren<FkingCardDisplay>();
		if (fkd == null)
		{
			Debug.LogError("[ShowStats] FkingCardDisplay が見つからない");
			yield break;
		}

		GameObject statsGroup = fkd.battleStatsGroup;
		TMP_Text apText = fkd.battleAPText;
		TMP_Text bpText = fkd.battleBPText;

		// statsGroup が生成されるまで待機
		yield return new WaitUntil(() => statsGroup != null);

		// Statsを表示
		statsGroup.SetActive(true);

		if (apText != null) apText.text = fieldCard.Defense.ToString();
		if (bpText != null) bpText.text = fieldCard.Attack.ToString();
	}

	public void UpdateBattleStats(int ap, int bp)
	{
		if (battleStatsGroup != null)
			battleStatsGroup.SetActive(true);

		if (battleAPText != null)
			battleAPText.text = ap.ToString();

		if (battleBPText != null)
			battleBPText.text = bp.ToString();
	}







	/// <summary>
	/// UIを【永続効果モード】（通常フレーム、効果表示）にします
	/// </summary>
	private void SetPersistentEffectMode()
    {
        InitializeOriginalColors(); // 元の色を記憶

        // 状態の初期化
        HideAllCostUI();
        //if (statsText != null) statsText.gameObject.SetActive(false);

        // フレーム切り替え
        UpdateFrameDisplay(false); // 'false' = バトルモードではない

        // ▼▼▼ 修正点 (テキストと背景を表示) ▼▼▼
        // (hasPostSummonEffect == true の時に呼ばれるため)
        if (effectText != null) effectText.gameObject.SetActive(true);
        if (textBackgroundImage != null) textBackgroundImage.gameObject.SetActive(true);
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

        // イラスト切り替え (永続効果モードでは通常イラスト)
        if (artworkImage != null) artworkImage.gameObject.SetActive(true);
        if (keyCardIllustImage != null) keyCardIllustImage.gameObject.SetActive(false);

        // バトルStats非表示
        //if (battleStatsGroup != null) battleStatsGroup.SetActive(false);

        // ▼▼▼ 修正点 (KeyCardなら色を変更) ▼▼▼
        if (currentCardData != null && currentCardData.hasKeyCardIllust)
        {
            // KeyCardの場合：指定の色に変更
            if (effectText != null) effectText.color = keyCardTextColor;
            if (textBackgroundImage != null) textBackgroundImage.color = keyCardBackgroundColor;
        }
        else
        {
            // 通常カードの場合：元の色に戻す
            if (effectText != null) effectText.color = originalTextColor;
            if (textBackgroundImage != null) textBackgroundImage.color = originalBackgroundColor;
        }
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

        // (中略 ... 背景Quadのスケールリセット処理 ...)
        if (backgroundQuad != null)
        {
            if (!originalBackgroundScaleInitialized)
            {
                originalBackgroundScale = backgroundQuad.localScale;
                originalBackgroundScaleInitialized = true;
            }
            backgroundQuad.localScale = originalBackgroundScale;
        }
    }


    /// <summary>
    /// CardDataの色に基づき、正しいフレームだけを表示する
    /// </summary>
    /// <param name="isBattleMode">バトルモードかどうか</param>
    public void UpdateFrameDisplay(bool isBattleMode)
    {
        if (currentCardData == null) return;

        // 1. 念のため、すべてのフレームを一旦非表示にする
        foreach (var frameSet in colorFrameSets)
        {
            if (frameSet.normalFrame != null) frameSet.normalFrame.SetActive(false);
            if (frameSet.battleFrame != null) frameSet.battleFrame.SetActive(false);
        }

        // 2. CardDataからターゲットの色を取得
        FkingColorType targetColor = currentCardData.specificColor;

        // 3. リストから該当する色の設定を探す
        FkingColorFrameSet targetSet = colorFrameSets.Find(set => set.color == targetColor);

        if (targetSet == null)
        {
            if (targetColor != FkingColorType.None)
            {
                Debug.LogWarning($"Color '{targetColor}' のフレームセットが FkingCardDisplay に設定されていません。", this.gameObject);
            }
            return;
        }

        // 4. 見つかった設定に基づき、正しいフレームを表示する
        if (isBattleMode)
        {
            if (targetSet.battleFrame != null)
            {
                this.gameObject.name = this.currentCardData.cardName+"手札?";

				Debug.Log($"カード名のアクティブ化:{this.currentCardData.cardName}", this);
;                targetSet.battleFrame.SetActive(true);
                ShowbattleStats();
            }
        }
        else // (HandState or PersistentEffectMode)
        {
            if (targetSet.normalFrame != null)
            {
                targetSet.normalFrame.SetActive(true);
            }
        }
    }


    // -----------------------------------------------------------------
    // デバッグ用メソッド (FkingCardDisplayAttacherから呼び出し)
    // -----------------------------------------------------------------
    // (これらのメソッドは、修正済みの Set...Mode() を呼ぶため、変更不要です)

    /// <summary>
    /// デバッグ用: 1. 手札状態 (通常表示) に戻します
    /// </summary>
    public void Debug_SetHandState()
    {
        if (currentCardData == null) return;
        SetHandState();
        //if (statsText != null)
        //{
        //    if (currentCardData.cardType == "ユニットスター")
        //    {
        //        statsText.gameObject.SetActive(true);
        //        statsText.text = $"AP {currentCardData.ap} / BP {currentCardData.bp}";
        //    }
        //    else
        //    {
        //        statsText.gameObject.SetActive(false);
        //    }
        //}
        UpdateCostDisplay(currentCardData);
    }

    /// <summary>
    /// デバッグ用: 2. バトルフレーム状態 (永続効果なし) にします
    /// </summary>
    public void Debug_SetBattleFrameState()
    {
        if (currentCardData == null) return;
        SetBattleFrameMode(currentCardData.ap, currentCardData.bp);
    }

    /// <summary>
    /// デバッグ用: 3. 永続効果モード状態 (通常フレーム・効果表示) にします
    /// </summary>
    public void Debug_SetBattleFramePersistentState()
    {
        if (currentCardData == null) return;
        SetPersistentEffectMode();
    }


    // --- ユーティリティメソッド ---
    private void UpdateCostDisplay(FkingCardData data)
    {
        HideAllCostUI();
        if (data.genericCost > 0 && genericCostUI.group != null)
        {
            genericCostUI.group.SetActive(true);
            genericCostUI.text.text = data.genericCost.ToString();
        }
        if (data.specificCost > 0)
        {
            FkingCostUIElement targetUI = null;

            switch (data.specificColor)
            {
                case FkingColorType.Red: targetUI = redCostUI; break;
                case FkingColorType.Green: targetUI = greenCostUI; break;
                case FkingColorType.Blue: targetUI = blueCostUI; break;
                case FkingColorType.Yellow: targetUI = yellowCostUI; break;
                case FkingColorType.Purple: targetUI = purpleCostUI; break;
                case FkingColorType.Rainbow: targetUI = rainbowCostUI; break;
            }

            if (targetUI != null && targetUI.group != null)
            {
                targetUI.group.SetActive(true);
                targetUI.text.text = data.specificCost.ToString();
            }
        }
    }

    private void HideAllCostUI()
    {
        if (genericCostUI.group != null) genericCostUI.group.SetActive(false);
        if (redCostUI.group != null) redCostUI.group.SetActive(false);
        if (greenCostUI.group != null) greenCostUI.group.SetActive(false);
        if (blueCostUI.group != null) blueCostUI.group.SetActive(false);
        if (yellowCostUI.group != null) yellowCostUI.group.SetActive(false);
        if (purpleCostUI.group != null) purpleCostUI.group.SetActive(false);
        if (rainbowCostUI != null && rainbowCostUI.group != null) rainbowCostUI.group.SetActive(false);
    }

	public void ShowBack()
	{
		if (artworkImage != null && backSprite != null)
		{
			artworkImage.sprite = backSprite;
		}
	}

	public void ShowFront()
	{
		if (artworkImage != null && currentCardData != null)
		{
			artworkImage.sprite = currentCardData.cardImage;
		}
	}

	public void SetAIFaceDownMode()
	{
		// 裏面にする
		ShowBack();

		// 名前消す
		if (nameText != null)
			nameText.gameObject.SetActive(false);

		// 効果消す
		if (effectText != null)
			effectText.gameObject.SetActive(false);

		// 背景も消す
		if (textBackgroundImage != null)
			textBackgroundImage.gameObject.SetActive(false);

		// Stats消す
		if (statsText != null)
			statsText.gameObject.SetActive(false);

		// BattleStatsも消す
		if (battleStatsGroup != null)
			battleStatsGroup.SetActive(false);

		// コスト全部消す
		HideAllCostUI();

		// フレーム全部消す
		foreach (var frameSet in colorFrameSets)
		{
			if (frameSet.normalFrame != null)
				frameSet.normalFrame.SetActive(false);

			if (frameSet.battleFrame != null)
				frameSet.battleFrame.SetActive(false);
		}

		// KeyCardIllustも消す
		if (keyCardIllustImage != null)
			keyCardIllustImage.gameObject.SetActive(false);
	}

	public void RevealCard()
	{
		ShowFront();

		// UIを普通に戻す
		if (currentCardData != null)
			FkingUpdateDisplay(currentCardData);
	}


}