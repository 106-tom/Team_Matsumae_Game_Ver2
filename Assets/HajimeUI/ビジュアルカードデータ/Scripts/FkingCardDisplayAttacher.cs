
using UnityEngine;

[RequireComponent(typeof(FkingCardDisplay))]
public class FkingCardDisplayAttacher : MonoBehaviour
{
	public FkingCardDatabase cardDatabase;

	public string cardIDToDisplay;

	//public Sprite backSprite;  // Inspector で裏面画像をセット

	//public Sprite backSprite;  // Inspector で裏面画像をセット


	public enum DebugDisplayMode
	{
		HandState,
		BattleFrame,
		BattleFramePersistent
	}

	[Header("デバッグ用 表示状態")]
	public DebugDisplayMode displayMode = DebugDisplayMode.HandState;

	private FkingCardDisplay cardDisplay;
	private FkingCardData currentData;

	// ============================
	// Unity
	// ============================

	void Awake()
	{
		cardDisplay = GetComponent<FkingCardDisplay>();

		// ★ Transform事故防止（④対策・最低限）
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		transform.localScale = Vector3.one;

		// Canvasが子にある前提
		var canvas = GetComponentInChildren<Canvas>();
		if (canvas != null)
		{
			canvas.renderMode = RenderMode.WorldSpace;
			canvas.transform.localRotation = Quaternion.identity;
			canvas.transform.localScale = Vector3.one * 0.01f;
		}

		//cardDisplay.backSprite = backSprite;
	}

	void Start()
	{
		if (!string.IsNullOrEmpty(cardIDToDisplay))
		{
			ChangeCard(cardIDToDisplay);
		}
	}

	// ============================
	// 外部呼び出し
	// ============================

	public void ChangeCard(string newID)
	{
		//Debug.Log("[Fking] ChangeCard: " + newID);

		if (string.IsNullOrEmpty(newID))
		{
			Debug.LogError("ChangeCard に空のIDが渡されました");
			return;
		}

		// ★ ① Deck から渡された ID を保持する
		cardIDToDisplay = newID;

		if (cardDatabase == null)
		{
			Debug.LogError("CardDatabase が設定されていません");
			return;
		}

		var data = cardDatabase.GetCardByID(newID);
		if (data == null)
		{
			Debug.LogWarning($"カードID {newID} が見つかりません");
			return;
		}

		currentData = data;

		// ★ ② 見た目更新
		cardDisplay.FkingUpdateDisplay(currentData);

		ApplyDebugMode(displayMode);
	}


	public void SetDebugMode(DebugDisplayMode mode)
	{
		displayMode = mode;
		ApplyDebugMode(mode);
	}

	// ============================
	// 見た目制御（このCS内で完結）
	// ============================

	private void ApplyDebugMode(DebugDisplayMode mode)
	{
		switch (mode)
		{
			case DebugDisplayMode.HandState:
				ApplyHandState();
				break;

			case DebugDisplayMode.BattleFrame:
				ApplyBattleState(false);
				break;

			case DebugDisplayMode.BattleFramePersistent:
				ApplyBattleState(true);
				break;
		}
	}

	private void ApplyHandState()
	{
		// 手札用：少し小さめ
		transform.localScale = Vector3.one * 0.9f;
	}

	private void ApplyBattleState(bool persistent)
	{
		// 盤面用：少し大きく
		transform.localScale = Vector3.one * 1.1f;

		// 常在呪文などの区別（とりあえずログ）
		if (persistent)
		{
			//Debug.Log("BattleFrame (Persistent)");
		}
		else
		{
			//Debug.Log("BattleFrame");
		}
	}

	public string GetCardID()
	{
		return cardIDToDisplay;
	}

	public FkingCardData GetCardData()
	{
		return currentData;
	}

	// FkingCardDisplayAttacher
	public void ApplyCardData(CardAI card, bool isAI = false)
	{
		if (card == null) return;

		// 表面データをセット
		ChangeCard(card.cardID);

		// AIなら裏面にする
		if (isAI)
			cardDisplay.ShowBack();
		else
			cardDisplay.ShowFront();
	}

	// FkingCardDisplayAttacher.cs
	public void ShowBack()
	{
		cardDisplay.ShowBack();
	}


	public void SetAIFaceDownMode()
	{
		if (cardDisplay != null)
			cardDisplay.SetAIFaceDownMode();
	}




}
