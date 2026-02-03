using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using static UnityEditor.Timeline.TimelinePlaybackControls;
#endif

public class CardVisualController : MonoBehaviour
{
	// カード全体サイズ
	private const float CARD_WIDTH = 300f;
	private const float CARD_HEIGHT = 420f;

	private RectTransform canvasRT;

	private RectTransform cardIllust;
	private RectTransform textImage;
	private RectTransform battleFrame;
	private RectTransform frame;
	private RectTransform red;
	private RectTransform nameText;
	private RectTransform effect;

	void Awake()
	{
		CacheReferences();
		ApplyLayout();
	}

	// -----------------------------
	// 参照取得（名前ベース）
	// -----------------------------
	private void CacheReferences()
	{
		//var canvas = GetComponentInChildren<Canvas>();
		//if (canvas == null)
		//{
		//	Debug.LogError("Canvas が見つからない", this);
		//	return;
		//}
		//
		//canvasRT = canvas.GetComponent<RectTransform>();
		//
		//cardIllust = FindRT("CardIllust");
		//textImage = FindRT("TextImage");
		//battleFrame = FindRT("BattleFrame");
		//frame = FindRT("Frame");
		//red = FindRT("red");
		//nameText = FindRT("NameText");
		//effect = FindRT("Effect");

		var canvas = GetComponentInChildren<Canvas>();
		if (canvas == null)
		{
			Debug.LogError("Canvas が見つからない", this);
			return;
		}

		canvasRT = canvas.GetComponent<RectTransform>();
	}

	private RectTransform FindRT(string name)
	{
		var t = canvasRT.Find(name);
		if (t == null)
		{
			Debug.LogWarning($"UI が見つからない: {name}", this);
			return null;
		}
		return t.GetComponent<RectTransform>();
	}

	// -----------------------------
	// レイアウト適用
	// -----------------------------
	public void ApplyLayout()
	{
		if (canvasRT == null) return;

		// Canvas = カードサイズ
		SetupRect(canvasRT, Vector2.zero, new Vector2(CARD_WIDTH, CARD_HEIGHT));

		// フレーム系（全面）
		Stretch(frame);
		Stretch(battleFrame);

		// イラスト
		if (cardIllust)
		{
			SetupRect(
				cardIllust,
				new Vector2(0, 40),
				new Vector2(CARD_WIDTH - 40, CARD_HEIGHT - 160)
			);
		}

		// 名前
		if (nameText)
		{
			SetupRect(
				nameText,
				new Vector2(0, CARD_HEIGHT / 2 - 30),
				new Vector2(CARD_WIDTH - 40, 40)
			);
		}

		// コスト（red）
		if (red)
		{
			SetupRect(
				red,
				new Vector2(-CARD_WIDTH / 2 + 40, CARD_HEIGHT / 2 - 40),
				new Vector2(70, 70)
			);
		}

		// テキスト画像（説明欄）
		if (textImage)
		{
			SetupRect(
				textImage,
				new Vector2(0, -40),
				new Vector2(CARD_WIDTH - 60, 80)
			);
		}

		// エフェクト（全面）
		Stretch(effect);
	}

	// -----------------------------
	// RectTransform 操作ユーティリティ
	// -----------------------------
	private void SetupRect(RectTransform rt, Vector2 pos, Vector2 size)
	{
		if (rt == null) return;

		rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
		rt.pivot = new Vector2(0.5f, 0.5f);
		rt.anchoredPosition = pos;
		rt.sizeDelta = size;
		rt.localRotation = Quaternion.identity;
		rt.localScale = Vector3.one;
	}

	private void Stretch(RectTransform rt)
	{
		if (rt == null) return;

		rt.anchorMin = Vector2.zero;
		rt.anchorMax = Vector2.one;
		rt.offsetMin = Vector2.zero;
		rt.offsetMax = Vector2.zero;
		rt.localRotation = Quaternion.identity;
		//rt.localScale = Vector3.one;
	}

	public void SetCardData(CardAI card)
	{
		//if (card == null) return;
		//
		//if (artworkImage)
		//	artworkImage.sprite = card.artwork;
		//
		//if (nameText)
		//	nameText.text = card.cardName;
		//
		//if (attackText)
		//	attackText.text = card.attack.ToString();
		//
		//if (defenseText)
		//	defenseText.text = card.defense.ToString();
	}

}
