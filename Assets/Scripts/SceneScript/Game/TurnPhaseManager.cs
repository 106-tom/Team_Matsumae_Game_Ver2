using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ManaUseManager;

public class TurnPhaseManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
	#region Singleton
	public static TurnPhaseManager Instance;
	#endregion

	#region UI
	public TextMeshProUGUI infoText; // UIのテキスト（現在のターンとフェーズ表示用）
	#endregion

	#region プレイヤー管理
	public InGamePlayer[] players = new InGamePlayer[2];
	private int currentPlayerIndex = 0; // 配列インデックスで管理
	#endregion

	#region ターン・フェーズ管理
	private const byte PHASE_EVENT = 1;

	private enum PlayerTurn { Host, Guest }
	private enum Phase { Start, Draw, Mana, Main, Attack, End }

	private PlayerTurn currentPlayer = PlayerTurn.Host;
	private Phase currentPhase = Phase.Start;
	public int currentPlayerTurn = 0;
	#endregion

	#region カード操作
	private Card selectedCard;
	#endregion



	private ManaUseManager manaUseManager;

	#region 初期化
	private void Awake()
	{
		// シングルトン処理
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Destroy(gameObject);
			return;
		}

		// ManaUseManager を自動で探す
		if (manaUseManager == null)
		{
			manaUseManager = FindObjectOfType<ManaUseManager>();
			if (manaUseManager == null)
			{
				Debug.LogError("[TurnPhaseManager] ManaUseManager が見つかりません！");
			}
		}
	}

	void Start()
	{
		InitializePlayers();
		UpdateText();

		this.manaUseManager = FindAnyObjectByType<ManaUseManager>();
		Debug.Assert(this.manaUseManager != null, "manaManager がない");
	}

	// プレイヤーの初期化
	void InitializePlayers()
	{
		if (!PhotonNetwork.IsConnected) return;

		Player[] photonPlayers = PhotonNetwork.PlayerList;
		for (int i = 0; i < photonPlayers.Length && i < players.Length; i++)
		{
			Player p = photonPlayers[i];
			string name = p.NickName;
			int deckID = p.CustomProperties.ContainsKey("DeckID") ? (int)p.CustomProperties["DeckID"] : 0;

			players[i] = new InGamePlayer(p.ActorNumber, name, deckID);
			Debug.Log($"Player {p.ActorNumber} 初期化: {name}, Deck {deckID}");
		}

		// 現在のプレイヤーを設定
		if (PhotonNetwork.IsMasterClient)
			currentPlayer = PlayerTurn.Host;
		else
			currentPlayer = PlayerTurn.Guest;
	}
	#endregion

	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			if (CanControlNow())
				SendNextPhaseEvent();
		}
	}

	#region === Turn / Phase Progress ===

	public void SendNextPhaseEvent()
	{
		if (PhotonNetwork.IsConnected)
		{
			PhotonNetwork.RaiseEvent(PHASE_EVENT, null, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
		}
		else
		{
			// オフライン時は直接実行
			NextPhase();
		}
	}

	public void OnEvent(EventData photonEvent)
	{
		if (photonEvent.Code == PHASE_EVENT)
		{
			NextPhase();
		}
	}

	void NextPhase()
	{
		// フェーズ遷移：Start > Draw > Mana > Main > Attack > End > (ターン交代) > Start
		currentPhase++;

		if (currentPhase > Phase.End)
		{
			// エンドフェーズの後はターン交代してスタートフェーズへ
			currentPhase = Phase.Start;
			currentPlayerIndex = (currentPlayerIndex + 1) % players.Length;
			currentPlayer = currentPlayer == PlayerTurn.Host ? PlayerTurn.Guest : PlayerTurn.Host;
			Debug.Log($"=== ターン交代: Player {CurrentPlayer?.PlayerID} ===");
		}

		UpdateText();
		HandlePhaseAction();
	}

	#endregion

	#region === UI / Control Check ===

	void UpdateText()
	{
		if (infoText != null)
		{
			infoText.text = $"Turn: {currentPlayer}\nPhase: {currentPhase}";
			infoText.text += CanControlNow() ? "\n(Your Turn ✅)" : "\n(Wait...)";
		}
	}

	bool CanControlNow()
	{
		if (!PhotonNetwork.IsConnected) return true;
		return (currentPlayer == PlayerTurn.Host && PhotonNetwork.IsMasterClient) ||
			   (currentPlayer == PlayerTurn.Guest && !PhotonNetwork.IsMasterClient);
	}

	// プレイヤー取得
	public InGamePlayer GetPlayer(int actorNumber)
	{
		foreach (var p in players)
		{
			if (p != null && p.PlayerID == actorNumber)
				return p;
		}
		Debug.LogError($"Player {actorNumber} は存在しません");
		return null;
	}

	// 現在のターンプレイヤー
	public InGamePlayer CurrentPlayer => players[currentPlayerIndex];

	// プレイヤーがターン中か
	public bool IsPlayerTurn(int actorNumber)
	{
		if (CurrentPlayer == null) return false;
		return CurrentPlayer.PlayerID == actorNumber;
	}

	#endregion

	#region === Photon Callback Setup ===
	public override void OnEnable()
	{
		base.OnEnable();
		PhotonNetwork.AddCallbackTarget(this);
	}

	public override void OnDisable()
	{
		base.OnDisable();
		PhotonNetwork.RemoveCallbackTarget(this);
	}
	#endregion

	#region === Phase Actions ===

	void HandlePhaseAction()
	{
		switch (currentPhase)
		{
			case Phase.Start: OnStartPhase(); break;
			case Phase.Draw: OnDrawPhase(); break;
			case Phase.Mana: OnManaPhase(); break;
			case Phase.Main: OnMainPhase(); break;
			case Phase.Attack: OnAttackPhase(); break;
			case Phase.End: OnEndPhase(); break;
		}
	}

	void OnStartPhase()
	{
		Debug.Log("[Start] スタートフェーズ開始");
		
		if (CurrentPlayer == null)
		{
			Debug.LogWarning("[Start] 現在のプレイヤーが存在しません");
			return;
		}

		// 1. 使用済み状態のマナを全てアクティブ状態に戻す
		var manaManager = FindObjectOfType<ManaUseManager>();
		if (manaManager != null)
		{
			manaManager.RefreshMana();
			Debug.Log("[Start] マナを全てアクティブ状態に回復しました");
		}
		else
		{
			Debug.LogWarning("[Start] ManaUseManagerが見つかりません");
		}

		// 2. 疲労状態のモンスターカードを全て回復状態にする
		int untappedCount = 0;
		List<int> untappedCardIds = new List<int>();
		
		foreach (Card card in CurrentPlayer.field)
		{
			if (card.isTapped)
			{
				card.isTapped = false;
				untappedCount++;
				untappedCardIds.Add(card.id);
			}
		}

		if (untappedCount > 0)
		{
			Debug.Log($"[Start] {untappedCount}枚のモンスターカードを回復状態にしました");
			
			// ネットワーク同期（回復したカードのみ）
			if (PhotonNetwork.IsConnected && ActionSyncManager.Instance != null)
			{
				foreach (int cardId in untappedCardIds)
				{
					var action = new ActionSyncManager.PlayerActionData
					{
						actionType = ActionSyncManager.ActionType.Untap,
						actorId = CurrentPlayer.PlayerID,
						cardId = cardId,
						isTapped = false
					};
					ActionSyncManager.Instance.SyncPlayerAction(action);
				}
			}
		}
		else
		{
			Debug.Log("[Start] 回復が必要なモンスターカードはありませんでした");
		}
	}

	void OnDrawPhase()
	{
		Debug.Log("[Draw] ドローフェーズ開始");
		
		if (CurrentPlayer == null)
		{
			Debug.LogWarning("[Draw] 現在のプレイヤーが存在しません");
			return;
		}

		// デッキの枚数を確認
		if (CurrentPlayer.deck.Count == 0)
		{
			Debug.Log($"[Draw] Player {CurrentPlayer.PlayerID} のデッキが0枚です。敗北します。");
			
			// 敗北処理：相手を勝者としてゲーム終了
			int winnerId = GetOpponentPlayerId(CurrentPlayer.PlayerID);
			if (winnerId != -1)
			{
				EndGame(winnerId);
			}
			else
			{
				Debug.LogError("[Draw] 相手プレイヤーが見つかりません");
			}
			return;
		}

		// デッキからカードを1枚引く
		Card drawnCard = CurrentPlayer.deck[0];
		CurrentPlayer.deck.RemoveAt(0);
		CurrentPlayer.hand.Add(drawnCard);
		
		Debug.Log($"[Draw] Player {CurrentPlayer.PlayerID} がカード {drawnCard.id} をドローしました（残りデッキ: {CurrentPlayer.deck.Count}枚）");

		// ネットワーク同期
		if (PhotonNetwork.IsConnected && ActionSyncManager.Instance != null)
		{
			var action = new ActionSyncManager.PlayerActionData
			{
				actionType = ActionSyncManager.ActionType.DrawCard,
				actorId = CurrentPlayer.PlayerID,
				cardId = drawnCard.id
			};
			ActionSyncManager.Instance.SyncPlayerAction(action);
		}

		// ドロー後のデッキ枚数を再確認（念のため）
		if (CurrentPlayer.deck.Count == 0)
		{
			Debug.Log($"[Draw] Player {CurrentPlayer.PlayerID} のデッキが0枚になりました。敗北します。");
			int winnerId = GetOpponentPlayerId(CurrentPlayer.PlayerID);
			if (winnerId != -1)
			{
				EndGame(winnerId);
			}
		}
	}

	#region マナフェーズ関連
	private bool isWaitingForManaSelection = false;
	private ManaUseManager.ManaColor? selectedManaColor = null;

	[SerializeField] private GameObject manaPanel;

	[SerializeField] private Transform manaZone;
	[SerializeField] private GameObject manaPrefab;
	[SerializeField] private Sprite redSprite;
	[SerializeField] private Sprite blueSprite;
	[SerializeField] private Sprite greenSprite;
	[SerializeField] private Sprite yellowSprite;
	[SerializeField] private Sprite purpleSprite;

	private List<ManaUseManager.ManaColor> manaList = new List<ManaUseManager.ManaColor>();



	void OnManaPhase()
	{
		Debug.Log("[Mana] マナフェーズ開始 - 5色から1色を選択してください");
		
		if (CurrentPlayer == null)
		{
			Debug.LogWarning("[Mana] 現在のプレイヤーが存在しません");
			return;
		}

		// マナ選択待ち状態にする
		isWaitingForManaSelection = true;
		selectedManaColor = null;

		// 自分のターンの場合のみ選択可能
		if (CanControlNow())
		{
			manaPanel.SetActive(true);
		}
	}

	public void OnClickManaButton(string colorName)
	{
		if (System.Enum.TryParse(colorName, out ManaUseManager.ManaColor color))
		{
			SelectManaColor(color);
			manaPanel.SetActive(false); // ✅ 選んだらUI閉じる
		}
	}

	// --- ここから追加 ---
	public void OnClickRed()
	{
		SelectManaColor(ManaUseManager.ManaColor.Red);
	}

	public void OnClickBlue()
	{
		SelectManaColor(ManaUseManager.ManaColor.Blue);
	}

	public void OnClickGreen()
	{
		SelectManaColor(ManaUseManager.ManaColor.Green);
	}

	public void OnClickYellow()
	{
		SelectManaColor(ManaUseManager.ManaColor.Yellow);
	}

	public void OnClickPurple()
	{
		SelectManaColor(ManaUseManager.ManaColor.Purple);
	}
	// --- ここまで ---


	// テスト用：自動的にマナを選択する機能
	private void AutoSelectManaForTest()
	{
		// 使用可能なマナ色のリスト
		ManaUseManager.ManaColor[] availableColors = new ManaUseManager.ManaColor[]
		{
			ManaUseManager.ManaColor.Red,
			ManaUseManager.ManaColor.Blue,
			ManaUseManager.ManaColor.Green,
			ManaUseManager.ManaColor.Yellow,
			ManaUseManager.ManaColor.Purple
		};

		// ランダムに選択（テスト用）
		// または順番に選択する場合は、ターン数などに基づいて選択
		System.Random random = new System.Random();
		ManaUseManager.ManaColor selectedColor = availableColors[random.Next(availableColors.Length)];

		// 少し遅延を入れてから選択（UI表示の時間を確保）
		StartCoroutine(DelayedManaSelection(selectedColor));
	}

	// 遅延してマナを選択（コルーチン）
	private System.Collections.IEnumerator DelayedManaSelection(ManaUseManager.ManaColor color)
	{
		// 0.5秒待機（UI表示の時間を確保）
		yield return new WaitForSeconds(0.5f);
		
		Debug.Log($"[Mana] テスト用：自動的に{color}マナを選択します");
		SelectManaColor(color);
	}

	// マナ色を選択する（UIから呼び出される想定）
	public void SelectManaColor(ManaUseManager.ManaColor color)
	{
		if (!isWaitingForManaSelection)
		{
			Debug.LogWarning("[Mana] マナフェーズではありません");
			return;
		}

		if (!CanControlNow())
		{
			Debug.LogWarning("[Mana] あなたのターンではありません");
			return;
		}

		selectedManaColor = color;
		ChargeSelectedMana();
		manaPanel.SetActive(false);

	}

	private const int MAX_MANA_UI = 10;

	private void AddManaToScreen(ManaUseManager.ManaColor color)
	{
		// マナ上限10枚
		if (manaList.Count >= MAX_MANA_UI)
		{
			// 一番古いマナを削除
			manaList.RemoveAt(0);
			if (manaZone.childCount > 0)
				Destroy(manaZone.GetChild(0).gameObject);
		}

		manaList.Add(color);

		GameObject go = Instantiate(manaPrefab, manaZone);
		go.transform.localScale = Vector3.one;   // 必ずリセット
		Image img = go.GetComponent<Image>();
		img.sprite = GetSpriteForColor(color);
	}

	private Sprite GetSpriteForColor(ManaUseManager.ManaColor color)
	{
		return color switch
		{
			ManaUseManager.ManaColor.Red => redSprite,
			ManaUseManager.ManaColor.Blue => blueSprite,
			ManaUseManager.ManaColor.Green => greenSprite,
			ManaUseManager.ManaColor.Yellow => yellowSprite,
			ManaUseManager.ManaColor.Purple => purpleSprite,
			_ => null
		};
	}

	// 選択されたマナをチャージ
	private void ChargeSelectedMana()
	{
		if (selectedManaColor == null) return;
		if (this.manaUseManager == null) return;

		ManaUseManager.ManaColor color = selectedManaColor.Value;

		// ゲーム上のマナを追加
		manaUseManager.AddMana(color);

		// 画面にマナを追加
		AddManaToScreen(color);

		// ネットワーク同期
		if (PhotonNetwork.IsConnected && ActionSyncManager.Instance != null)
		{
			var action = new ActionSyncManager.PlayerActionData
			{
				actionType = ActionSyncManager.ActionType.ChargeMana,
				actorId = CurrentPlayer.PlayerID,
				cardId = -1,
				extraData = color.ToString()
			};
			ActionSyncManager.Instance.SyncPlayerAction(action);
		}

		// マナ選択状態を解除
		isWaitingForManaSelection = false;
		selectedManaColor = null;
	}


	// リモートプレイヤーのマナチャージを適用
	public void ApplyRemoteManaCharge(int actorId, string manaColorString)
	{
		var manaManager = FindObjectOfType<ManaUseManager>();
		if (manaManager == null)
		{
			Debug.LogError("[Mana] ManaUseManagerが見つかりません");
			return;
		}

		// 文字列からManaColorに変換
		if (System.Enum.TryParse<ManaUseManager.ManaColor>(manaColorString, out ManaUseManager.ManaColor color))
		{
			manaManager.AddMana(color);
			Debug.Log($"[Mana] Player {actorId} が{color}マナをチャージしました（リモート）");
		}
		else
		{
			Debug.LogWarning($"[Mana] 無効なマナ色: {manaColorString}");
		}
	}
	#endregion

	#region メインフェーズ関連
	private bool isMainPhaseActive = false;

	void OnMainPhase()
	{
		Debug.Log("[Main] メインフェーズ開始 - モンスターカードの召喚と呪文カードの使用が可能です");
		
		if (CurrentPlayer == null)
		{
			Debug.LogWarning("[Main] 現在のプレイヤーが存在しません");
			return;
		}

		// メインフェーズをアクティブにする
		isMainPhaseActive = true;
	}

	// カードを使用する（モンスター召喚または呪文使用）
	public bool UseCard(int playerId, Card card)
	{
		if (!isMainPhaseActive)
		{
			Debug.LogWarning("[Main] メインフェーズではありません");
			return false;
		}

		if (!IsPlayerTurn(playerId))
		{
			Debug.LogWarning("[Main] あなたのターンではありません");
			return false;
		}

		if (CurrentPlayer == null)
		{
			Debug.LogError("[Main] 現在のプレイヤーが存在しません");
			return false;
		}

		if (card == null || !CurrentPlayer.hand.Contains(card))
		{
			Debug.LogWarning("[Main] 手札にそのカードがありません");
			return false;
		}

		// マナ支払いの確認と処理
		if (!CanPayManaCost(card))
		{
			Debug.Log($"[Main] Player {playerId} マナ不足: カード {card.id} を使用できません");
			return false;
		}

		// マナを支払う
		if (!PayManaCost(card))
		{
			Debug.LogError("[Main] マナ支払いに失敗しました");
			return false;
		}

		// カードの種類に応じて処理を分岐
		bool isMonster = card.bp > 0; // bp > 0 ならモンスターカード

		if (isMonster)
		{
			// モンスターカードの召喚
			return SummonMonster(playerId, card);
		}
		else
		{
			// 呪文カードの使用
			return CastSpell(playerId, card);
		}
	}

	// モンスターカードの召喚
	private bool SummonMonster(int playerId, Card card)
	{
		// 手札からフィールドに移動
		CurrentPlayer.hand.Remove(card);
		CurrentPlayer.field.Add(card);
		card.isTapped = false; // 召喚直後は未タップ状態

		Debug.Log($"[Main] Player {playerId} がモンスターカード {card.id} (AP:{card.ap}, BP:{card.bp}) を召喚しました");

		// ネットワーク同期
		if (PhotonNetwork.IsConnected && ActionSyncManager.Instance != null)
		{
			var action = new ActionSyncManager.PlayerActionData
			{
				actionType = ActionSyncManager.ActionType.PlayCard,
				actorId = playerId,
				cardId = card.id,
				position = Vector3.zero // 位置情報は必要に応じて設定
			};
			ActionSyncManager.Instance.SyncPlayerAction(action);
		}

		// 召喚時の効果を発動（必要に応じて）
		// CardEffectManager.ActivateEffects(cardData, "onSummon");

		return true;
	}

	// 呪文カードの使用
	private bool CastSpell(int playerId, Card card)
	{
		// 手札から墓地に移動
		CurrentPlayer.hand.Remove(card);
		CurrentPlayer.graveyard.Add(card);

		Debug.Log($"[Main] Player {playerId} が呪文カード {card.id} を使用しました");

		// ネットワーク同期
		if (PhotonNetwork.IsConnected && ActionSyncManager.Instance != null)
		{
			var action = new ActionSyncManager.PlayerActionData
			{
				actionType = ActionSyncManager.ActionType.PlayCard,
				actorId = playerId,
				cardId = card.id,
				position = Vector3.zero
			};
			ActionSyncManager.Instance.SyncPlayerAction(action);
		}

		// 呪文の効果を発動（必要に応じて）
		// CardEffectManager.ActivateEffects(cardData, "onSummon");

		return true;
	}

	// マナコストを支払えるか確認
	private bool CanPayManaCost(Card card)
	{
		var manaManager = FindObjectOfType<ManaUseManager>();
		if (manaManager == null)
		{
			Debug.LogError("[Main] ManaUseManagerが見つかりません");
			return false;
		}

		// CardのマナコストをManaUseManagerの形式に変換
		List<(ManaUseManager.ManaColor color, int cost)> colorCosts = new List<(ManaUseManager.ManaColor, int)>();
		
		foreach (var entry in card.manaCostList)
		{
			// ManaColorをManaUseManager.ManaColorに変換
			if (System.Enum.TryParse<ManaUseManager.ManaColor>(entry.color.ToString(), out ManaUseManager.ManaColor manaColor))
			{
				colorCosts.Add((manaColor, entry.cost));
			}
		}

		return manaManager.CanPayCost(card.anyColorCost, colorCosts);
	}

	// マナコストを支払う
	private bool PayManaCost(Card card)
	{
		var manaManager = FindObjectOfType<ManaUseManager>();
		if (manaManager == null)
		{
			Debug.LogError("[Main] ManaUseManagerが見つかりません");
			return false;
		}

		// CardのマナコストをManaUseManagerの形式に変換
		List<(ManaUseManager.ManaColor color, int cost)> colorCosts = new List<(ManaUseManager.ManaColor, int)>();
		
		foreach (var entry in card.manaCostList)
		{
			// ManaColorをManaUseManager.ManaColorに変換
			if (System.Enum.TryParse<ManaUseManager.ManaColor>(entry.color.ToString(), out ManaUseManager.ManaColor manaColor))
			{
				colorCosts.Add((manaColor, entry.cost));
			}
		}

		bool success = manaManager.PayCost(card.anyColorCost, colorCosts);
		
		if (success)
		{
			Debug.Log($"[Main] マナコストを支払いました（総コスト: {card.TotalCost()}）");
		}
		else
		{
			Debug.LogWarning("[Main] マナコストの支払いに失敗しました");
		}

		return success;
	}
	#endregion

	#region アタックフェーズ関連
	private bool isAttackPhaseActive = false;
	private Card selectedAttacker = null;
	private bool isWaitingForAttackTarget = false;
	private bool isWaitingForBlock = false;
	private Card attackingCard = null;
	private int targetPlayerId = -1;
	private Card blockingCard = null;
	private bool attackerUsedSpell = false;
	private bool defenderUsedSpell = false;

	void OnAttackPhase()
	{
		Debug.Log("[Attack] アタックフェーズ開始 - モンスターの攻撃が可能です");
		
		if (CurrentPlayer == null)
		{
			Debug.LogWarning("[Attack] 現在のプレイヤーが存在しません");
			return;
		}

		// メインフェーズを終了
		isMainPhaseActive = false;
		// アタックフェーズをアクティブにする
		isAttackPhaseActive = true;
		// フィールドのモンスターを未タップ状態に（攻撃可能にする）
		foreach (Card card in CurrentPlayer.field)
		{
			if (card.bp > 0 && card.isTapped)
			{
				card.isTapped = false;
			}
		}
	}

	// 攻撃するモンスターを選択
	public void SelectAttacker(int playerId, Card attacker)
	{
		if (!isAttackPhaseActive)
		{
			Debug.LogWarning("[Attack] アタックフェーズではありません");
			return;
		}

		if (!IsPlayerTurn(playerId))
		{
			Debug.LogWarning("[Attack] あなたのターンではありません");
			return;
		}

		if (attacker == null || !CurrentPlayer.field.Contains(attacker))
		{
			Debug.LogWarning("[Attack] フィールドにそのモンスターがありません");
			return;
		}

		if (attacker.bp <= 0)
		{
			Debug.LogWarning("[Attack] モンスターカードではありません");
			return;
		}

		if (attacker.isTapped)
		{
			Debug.LogWarning("[Attack] そのモンスターは既に攻撃済みです");
			return;
		}

		selectedAttacker = attacker;
		isWaitingForAttackTarget = true;
		Debug.Log($"[Attack] Player {playerId} が攻撃モンスター {attacker.id} (AP:{attacker.ap}, BP:{attacker.bp}) を選択しました");

		// ネットワーク同期
		if (PhotonNetwork.IsConnected && ActionSyncManager.Instance != null)
		{
			var action = new ActionSyncManager.PlayerActionData
			{
				actionType = ActionSyncManager.ActionType.SelectCard,
				actorId = playerId,
				cardId = attacker.id,
				extraData = "attacker"
			};
			ActionSyncManager.Instance.SyncPlayerAction(action);
		}
	}

	// プレイヤーへの直接攻撃を宣言
	public void AttackPlayerDirectly(int playerId, int targetPlayerId)
	{
		if (!isWaitingForAttackTarget || selectedAttacker == null)
		{
			Debug.LogWarning("[Attack] 攻撃モンスターが選択されていません");
			return;
		}

		if (!IsPlayerTurn(playerId))
		{
			Debug.LogWarning("[Attack] あなたのターンではありません");
			return;
		}

		int opponentId = GetOpponentPlayerId(playerId);
		if (opponentId == -1 || targetPlayerId != opponentId)
		{
			Debug.LogWarning("[Attack] 無効な攻撃対象です");
			return;
		}

		SelectAttackTarget(playerId, targetMonster: null, targetPlayerId: targetPlayerId);
	}

	// 攻撃対象を選択（モンスター or プレイヤー）
	public void SelectAttackTarget(int playerId, Card targetMonster = null, int targetPlayerId = -1)
	{
		if (!isWaitingForAttackTarget || selectedAttacker == null)
		{
			Debug.LogWarning("[Attack] 攻撃モンスターが選択されていません");
			return;
		}

		if (!IsPlayerTurn(playerId))
		{
			Debug.LogWarning("[Attack] あなたのターンではありません");
			return;
		}

		// 相手プレイヤーを取得
		int opponentId = GetOpponentPlayerId(playerId);
		if (opponentId == -1)
		{
			Debug.LogError("[Attack] 相手プレイヤーが見つかりません");
			return;
		}

		var opponent = GetPlayer(opponentId);
		if (opponent == null)
		{
			Debug.LogError("[Attack] 相手プレイヤーのデータが見つかりません");
			return;
		}

		// モンスターへの攻撃
		if (targetMonster != null)
		{
			if (!opponent.field.Contains(targetMonster))
			{
				Debug.LogWarning("[Attack] 相手のフィールドにそのモンスターがありません");
				return;
			}

			// モンスター同士の戦闘を開始
			StartMonsterBattle(playerId, selectedAttacker, targetMonster);
		}
		// プレイヤーへの直接攻撃
		else if (targetPlayerId != -1 && targetPlayerId == opponentId)
		{
			attackingCard = selectedAttacker;
			this.targetPlayerId = targetPlayerId;
			isWaitingForAttackTarget = false;
			isWaitingForBlock = true;

			Debug.Log($"[Attack] Player {playerId} が Player {targetPlayerId} に直接攻撃を宣言しました");

			// ネットワーク同期（攻撃宣言）
			if (PhotonNetwork.IsConnected && ActionSyncManager.Instance != null)
			{
				var action = new ActionSyncManager.PlayerActionData
				{
					actionType = ActionSyncManager.ActionType.Attack,
					actorId = playerId,
					cardId = attackingCard.id,
					targetPlayerId = targetPlayerId,
					extraData = "player_attack"
				};
				ActionSyncManager.Instance.SyncPlayerAction(action);
			}

			// 相手がブロックするか選択（UIで選択させる想定）
			// テスト用：自動的にブロックしない（直接ダメージ）
			if (!CanControlNow()) // 相手のターンでない場合（相手が選択する）
			{
				// 相手が選択するまで待機
				Debug.Log($"[Attack] Player {targetPlayerId} はブロックするか選択してください");
			}
			else
			{
				// テスト用：自動的にブロックしない
				ResolvePlayerAttack(playerId, targetPlayerId, null);
			}
		}
		else
		{
			Debug.LogWarning("[Attack] 無効な攻撃対象です");
		}
	}

	// ブロックするモンスターを選択
	public void SelectBlocker(int defenderId, Card blocker)
	{
		if (!isWaitingForBlock)
		{
			Debug.LogWarning("[Attack] ブロック待ち状態ではありません");
			return;
		}

		if (defenderId != targetPlayerId)
		{
			Debug.LogWarning("[Attack] 防御側のプレイヤーではありません");
			return;
		}

		var defender = GetPlayer(defenderId);
		if (defender == null || !defender.field.Contains(blocker))
		{
			Debug.LogWarning("[Attack] フィールドにそのモンスターがありません");
			return;
		}

		if (blocker.bp <= 0)
		{
			Debug.LogWarning("[Attack] モンスターカードではありません");
			return;
		}

		blockingCard = blocker;
		Debug.Log($"[Attack] Player {defenderId} がブロッカー {blocker.id} (BP:{blocker.bp}) を選択しました");

		// ネットワーク同期（ブロック宣言）
		if (PhotonNetwork.IsConnected && ActionSyncManager.Instance != null)
		{
			var action = new ActionSyncManager.PlayerActionData
			{
				actionType = ActionSyncManager.ActionType.Attack,
				actorId = defenderId,
				cardId = blocker.id,
				targetCardIds = new List<int> { attackingCard.id },
				extraData = "block"
			};
			ActionSyncManager.Instance.SyncPlayerAction(action);
		}

		// ブロックした場合の戦闘を開始
		int attackerId = attackingCard.OwnerId;
		StartMonsterBattle(attackerId, attackingCard, blockingCard);
	}

	// ブロックしない（直接ダメージを受ける）
	public void DeclineBlock(int defenderId)
	{
		if (!isWaitingForBlock)
		{
			Debug.LogWarning("[Attack] ブロック待ち状態ではありません");
			return;
		}

		if (defenderId != targetPlayerId)
		{
			Debug.LogWarning("[Attack] 防御側のプレイヤーではありません");
			return;
		}

		int attackerId = attackingCard.OwnerId;
		ResolvePlayerAttack(attackerId, defenderId, null);
	}

	// モンスター同士の戦闘
	private void StartMonsterBattle(int attackerId, Card attacker, Card defender)
	{
		Debug.Log($"[Attack] 戦闘開始: 攻撃側 {attacker.id} (BP:{attacker.bp}) vs 防御側 {defender.id} (BP:{defender.bp})");

		// 戦闘中の呪文使用フラグをリセット
		attackerUsedSpell = false;
		defenderUsedSpell = false;

		// ここで呪文カードの使用タイミング（UIで選択させる想定）
		// テスト用：自動的に呪文を使用しない

		// 戦闘処理
		ResolveMonsterBattle(attacker, defender);
	}

	// モンスター同士の戦闘を解決
	private void ResolveMonsterBattle(Card attacker, Card defender)
	{
		// BP比較
		if (attacker.bp > defender.bp)
		{
			// 攻撃側の勝利
			Debug.Log($"[Attack] 攻撃側の勝利: {attacker.bp} > {defender.bp}");
			defender.TakeDamage(attacker.ap);
			attacker.isTapped = true; // 攻撃済みにする

			// ネットワーク同期（タップ）
			if (PhotonNetwork.IsConnected && ActionSyncManager.Instance != null)
			{
				var tapAction = new ActionSyncManager.PlayerActionData
				{
					actionType = ActionSyncManager.ActionType.Tap,
					actorId = attacker.OwnerId,
					cardId = attacker.id,
					isTapped = true
				};
				ActionSyncManager.Instance.SyncPlayerAction(tapAction);
			}

			// 防御側が破壊されたか確認
			if (defender.bp <= 0)
			{
				DestroyCard(GetPlayer(defender.OwnerId), defender);
			}
		}
		else if (defender.bp > attacker.bp)
		{
			// 防御側の勝利
			Debug.Log($"[Attack] 防御側の勝利: {defender.bp} > {attacker.bp}");
			attacker.TakeDamage(defender.ap);
			attacker.isTapped = true; // 攻撃済みにする

			// ネットワーク同期（タップ）
			if (PhotonNetwork.IsConnected && ActionSyncManager.Instance != null)
			{
				var tapAction = new ActionSyncManager.PlayerActionData
				{
					actionType = ActionSyncManager.ActionType.Tap,
					actorId = attacker.OwnerId,
					cardId = attacker.id,
					isTapped = true
				};
				ActionSyncManager.Instance.SyncPlayerAction(tapAction);
			}

			// 攻撃側が破壊されたか確認
			if (attacker.bp <= 0)
			{
				DestroyCard(GetPlayer(attacker.OwnerId), attacker);
			}
		}
		else
		{
			// 引き分け（両方破壊）
			Debug.Log($"[Attack] 引き分け: 両方のモンスターが破壊されます");
			attacker.TakeDamage(defender.ap);
			defender.TakeDamage(attacker.ap);
			attacker.isTapped = true;

			// ネットワーク同期（タップ）
			if (PhotonNetwork.IsConnected && ActionSyncManager.Instance != null)
			{
				var tapAction = new ActionSyncManager.PlayerActionData
				{
					actionType = ActionSyncManager.ActionType.Tap,
					actorId = attacker.OwnerId,
					cardId = attacker.id,
					isTapped = true
				};
				ActionSyncManager.Instance.SyncPlayerAction(tapAction);
			}

			if (attacker.bp <= 0)
			{
				DestroyCard(GetPlayer(attacker.OwnerId), attacker);
			}
			if (defender.bp <= 0)
			{
				DestroyCard(GetPlayer(defender.OwnerId), defender);
			}
		}

		// 戦闘終了
		ResetAttackState();
	}

	// プレイヤーへの攻撃を解決
	private void ResolvePlayerAttack(int attackerId, int targetPlayerId, Card blocker)
	{
		if (blocker != null)
		{
			// ブロックされた場合：モンスター同士の戦闘
			StartMonsterBattle(attackerId, attackingCard, blocker);
		}
		else
		{
			// 直接ダメージ
			var targetPlayer = GetPlayer(targetPlayerId);
			if (targetPlayer != null)
			{
				int damage = attackingCard.ap;
				targetPlayer.currentLife -= damage;
				
				// 体力が0未満にならないようにする
				if (targetPlayer.currentLife < 0)
				{
					targetPlayer.currentLife = 0;
				}
				
				attackingCard.isTapped = true; // 攻撃済みにする

				Debug.Log($"[Attack] Player {targetPlayerId} が {damage} ダメージを受けました（残りライフ: {targetPlayer.currentLife}/{InGamePlayer.MAX_LIFE}）");

				// ネットワーク同期（ダメージとタップ）
				if (PhotonNetwork.IsConnected && ActionSyncManager.Instance != null)
				{
					// ダメージ同期
					ActionSyncManager.Instance.SendLifeChange(targetPlayerId, -damage);
					
					// タップ同期
					var tapAction = new ActionSyncManager.PlayerActionData
					{
						actionType = ActionSyncManager.ActionType.Tap,
						actorId = attackerId,
						cardId = attackingCard.id,
						isTapped = true
					};
					ActionSyncManager.Instance.SyncPlayerAction(tapAction);
				}

				// 勝敗判定
				if (targetPlayer.currentLife <= 0)
				{
					EndGame(attackerId);
				}
			}

			// 戦闘終了
			ResetAttackState();
		}
	}

	// 戦闘中の呪文カード使用（攻撃側）
	public bool UseSpellInBattle(int playerId, Card spell, bool isAttacker)
	{
		if (!isWaitingForAttackTarget && !isWaitingForBlock)
		{
			Debug.LogWarning("[Attack] 戦闘中ではありません");
			return false;
		}

		if (isAttacker && attackerUsedSpell)
		{
			Debug.LogWarning("[Attack] 攻撃側は既に呪文を使用しています");
			return false;
		}

		if (!isAttacker && defenderUsedSpell)
		{
			Debug.LogWarning("[Attack] 防御側は既に呪文を使用しています");
			return false;
		}

		// 呪文カードの使用（メインフェーズと同じ処理）
		if (spell.bp > 0)
		{
			Debug.LogWarning("[Attack] 呪文カードではありません");
			return false;
		}

		// マナ支払いの確認と処理
		if (!CanPayManaCost(spell) || !PayManaCost(spell))
		{
			Debug.LogWarning("[Attack] マナ不足で呪文を使用できません");
			return false;
		}

		// 呪文を使用（手札から墓地へ）
		var player = GetPlayer(playerId);
		if (player != null && player.hand.Contains(spell))
		{
			player.hand.Remove(spell);
			player.graveyard.Add(spell);

			if (isAttacker)
			{
				attackerUsedSpell = true;
			}
			else
			{
				defenderUsedSpell = true;
			}

			Debug.Log($"[Attack] Player {playerId} が戦闘中に呪文 {spell.id} を使用しました（{(isAttacker ? "攻撃側" : "防御側")}）");

			// ネットワーク同期
			if (PhotonNetwork.IsConnected && ActionSyncManager.Instance != null)
			{
				var action = new ActionSyncManager.PlayerActionData
				{
					actionType = ActionSyncManager.ActionType.PlayCard,
					actorId = playerId,
					cardId = spell.id,
					position = Vector3.zero,
					extraData = isAttacker ? "attacker" : "defender"
				};
				ActionSyncManager.Instance.SyncPlayerAction(action);
			}

			return true;
		}

		return false;
	}

	// 攻撃状態をリセット
	private void ResetAttackState()
	{
		selectedAttacker = null;
		attackingCard = null;
		targetPlayerId = -1;
		blockingCard = null;
		isWaitingForAttackTarget = false;
		isWaitingForBlock = false;
		attackerUsedSpell = false;
		defenderUsedSpell = false;
	}
	#endregion

	void OnEndPhase()
	{
		Debug.Log("[End] ターン終了処理");
		// メインフェーズを終了
		isMainPhaseActive = false;
		// アタックフェーズを終了
		isAttackPhaseActive = false;
		ResetAttackState();
		// ターン終了、次のプレイヤーへ
		EndTurn();
	}

	// ターン終了処理
	void EndTurn()
	{
		currentPlayerIndex = (currentPlayerIndex + 1) % players.Length;
		currentPhase = Phase.Start;
		currentPlayer = currentPlayer == PlayerTurn.Host ? PlayerTurn.Guest : PlayerTurn.Host;
		
		Debug.Log($"=== ターン交代: Player {CurrentPlayer?.PlayerID} ===");
		
		if (PhotonNetwork.IsConnected)
		{
			SyncAction(ActionSyncManager.ActionType.EndTurn, CurrentPlayer?.PlayerID ?? -1);
		}
		
		UpdateText();
		HandlePhaseAction(); // Startフェーズを実行
	}

	#endregion

	#region カード操作

	public void SelectCard(int playerId, Card card)
	{
		if (!IsPlayerTurn(playerId))
			return;
		selectedCard = card;
		Debug.Log($"Player {playerId} selected card: {card.id}");

		if (PhotonNetwork.IsConnected)
			SyncAction(ActionSyncManager.ActionType.SelectCard, playerId, card.id);
	}

	public void SelectTarget(int actorId, Card targetCard)
	{
		if (!IsPlayerTurn(actorId) || selectedCard == null) return;

		var targets = new List<int> { targetCard.id };
		Debug.Log($"Player {actorId} attacks {targetCard.id} with {selectedCard.id}");

		if (PhotonNetwork.IsConnected)
		{
			var action = new ActionSyncManager.PlayerActionData
			{
				actionType = ActionSyncManager.ActionType.Attack,
				actorId = actorId,
				cardId = selectedCard.id,
				targetCardIds = new List<int>(targets)
			};
			ActionSyncManager.Instance.SyncPlayerAction(action);
		}

		ExecuteAttack(selectedCard, targetCard);
		selectedCard = null;
	}

	public void OnCardSummoned(InGamePlayer player, Card card)
	{
		Debug.Log($"TurnPhaseManager: {player.PlayerID} が {card.id} を召喚");
		if (PhotonNetwork.IsConnected)
			SyncAction(ActionSyncManager.ActionType.PlayCard, player.PlayerID, card.id);
	}

	private void ExecuteAttack(Card attacker, Card defender)
	{
		defender.TakeDamage(attacker.ap);
	}

	public void DestroyCard(InGamePlayer owner, Card card)
	{
		if (!owner.field.Contains(card))
		{
			Debug.LogWarning($"カード {card.id} は場に存在しません");
			return;
		}

		owner.field.Remove(card);
		owner.graveyard.Add(card);
		Debug.Log($"Player {owner.PlayerID} のカード {card.id} を破壊");
	}

	public void TapCard(int playerId, Card card, bool tapped)
	{
		if (!IsPlayerTurn(playerId))
			return;

		card.isTapped = tapped;
		Debug.Log($"Player {playerId} {(tapped ? "タップ" : "アンタップ")} {card.id}");

		if (PhotonNetwork.IsConnected)
		{
			var action = new ActionSyncManager.PlayerActionData
			{
				actionType = ActionSyncManager.ActionType.Tap,
				actorId = playerId,
				cardId = card.id,
				isTapped = tapped
			};
			ActionSyncManager.Instance.SyncPlayerAction(action);
		}
	}

	#endregion

	#region リモート操作の適用

	public void ApplyRemoteSelectCard(int actorId, int cardId)
		=> Debug.Log($"[Remote] Player {actorId} selected card {cardId}");

	public void ApplyRemotePlayCard(int actorId, int cardId, Vector3 pos)
	{
		var player = GetPlayer(actorId);
		if (player == null) return;

		Card card = player.hand.Find(c => c.id == cardId);
		if (card == null)
		{
			// 手札にない場合は既に使用済みの可能性がある
			Debug.Log($"[Remote] Player {actorId} played card {cardId} (既に処理済みの可能性)");
			return;
		}

		// カードの種類に応じて処理
		bool isMonster = card.bp > 0;

		if (isMonster)
		{
			// モンスターカード：手札からフィールドへ
			player.hand.Remove(card);
			player.field.Add(card);
			card.isTapped = false;
			Debug.Log($"[Remote] Player {actorId} がモンスターカード {cardId} を召喚しました");
		}
		else
		{
			// 呪文カード：手札から墓地へ
			player.hand.Remove(card);
			player.graveyard.Add(card);
			Debug.Log($"[Remote] Player {actorId} が呪文カード {cardId} を使用しました");
		}
	}

	public void ApplyRemoteDraw(int actorId, int cardId)
	{
		var player = GetPlayer(actorId);
		if (player == null) return;

		player.opponentHandCount++;
		Debug.Log($"[Remote] Player {actorId} drew card {cardId}");
	}

	public void ApplyRemoteDestroy(int actorId, int cardId)
	{
		var owner = GetPlayer(actorId);
		if (owner == null) return;

		Card card = owner.field.Find(c => c.id == cardId);
		if (card != null)
		{
			owner.field.Remove(card);
			owner.graveyard.Add(card);
			Debug.Log($"[Remote] Player {actorId} のカード {cardId} が破壊されました");
		}
	}

	public void ApplyRemoteAttack(int actorId, int attackerCardId, List<int> targetCardIds)
	{
		var player = GetPlayer(actorId);
		if (player == null) return;

		var attacker = player.FindCardById(attackerCardId);
		if (attacker == null)
		{
			Debug.LogWarning($"[Remote] 攻撃カード {attackerCardId} が見つかりません");
			return;
		}

		// モンスターへの攻撃
		if (targetCardIds != null && targetCardIds.Count > 0)
		{
			foreach (int targetId in targetCardIds)
			{
				// 相手プレイヤーのフィールドから探す
				int opponentId = GetOpponentPlayerId(actorId);
				var opponent = GetPlayer(opponentId);
				var target = opponent?.FindCardById(targetId);
				
				if (target != null)
				{
					StartMonsterBattle(actorId, attacker, target);
				}
				else
				{
					Debug.LogWarning($"[Remote] 対象カード {targetId} が見つかりません");
				}
			}
		}
		else
		{
			// プレイヤーへの直接攻撃（targetPlayerIdが設定されている場合）
			Debug.Log($"[Remote] Player {actorId} が攻撃を宣言しました（対象: プレイヤー）");
		}

		Debug.Log($"[Remote] Player {actorId} attacks {string.Join(",", targetCardIds ?? new List<int>())}");
	}

	public void ApplyRemoteTap(int actorId, int cardId, bool tapped)
	{
		var player = GetPlayer(actorId);
		if (player == null) return;

		var target = player.FindCardById(cardId);
		if (target != null)
			target.isTapped = tapped;

		Debug.Log($"[Remote] Player {actorId} {(tapped ? "タップ" : "アンタップ")} card {cardId}");
	}

	public void ApplyRemoteEndTurn(int nextActorId)
	{
		var player = GetPlayer(nextActorId);
		if (player == null) return;

		currentPlayerIndex = System.Array.IndexOf(players, player);
		currentPhase = Phase.Start;
		currentPlayer = currentPlayer == PlayerTurn.Host ? PlayerTurn.Guest : PlayerTurn.Host;
		Debug.Log($"[Remote] ターン変更: Player {nextActorId}（受信）");
		UpdateText();
	}

	public void ApplyRemoteStartTurn(int actorId)
	{
		var player = GetPlayer(actorId);
		if (player == null) return;

		currentPlayerIndex = System.Array.IndexOf(players, player);
		currentPhase = Phase.Start;
		currentPlayer = currentPlayer == PlayerTurn.Host ? PlayerTurn.Guest : PlayerTurn.Host;
		Debug.Log($"[Remote] Player {actorId} ターン開始");
		UpdateText();
	}

	#endregion

	#region 勝敗管理

	public void ApplyRemoteLifeChange(int playerId, int delta)
	{
		var player = GetPlayer(playerId);
		if (player == null) return;

		player.currentLife += delta;
		if (player.currentLife < 0) player.currentLife = 0;

		Debug.Log($"[Sync] Player {playerId} Life changed by {delta}, new life: {player.currentLife}");

		// 勝敗判定
		CheckWinCondition();
	}

	public void CheckWinCondition()
	{
		foreach (var player in players)
		{
			if (player != null && player.currentLife <= 0)
			{
				int winnerId = GetOpponentPlayerId(player.PlayerID);
				if (winnerId != -1)
				{
					EndGame(winnerId);
				}
				return;
			}
		}
	}

	public void EndGame(int winnerId)
	{
		int loserId = GetOpponentPlayerId(winnerId);
		if (loserId == -1)
		{
			Debug.LogError("[Game] 相手プレイヤーが見つかりません");
			return;
		}

		Debug.Log($"Game Over! Player {winnerId} wins, Player {loserId} loses.");

		if (PhotonNetwork.IsConnected && ActionSyncManager.Instance != null)
		{
			var action = new ActionSyncManager.PlayerActionData
			{
				actorId = winnerId,
				targetPlayerId = loserId,
				actionType = ActionSyncManager.ActionType.GameOver
			};
			ActionSyncManager.Instance.SyncPlayerAction(action);
		}
	}

	public void ShowRemoteGameOver(int winnerId, int loserId)
		=> Debug.Log($"[Sync] Game Over! Player {winnerId} wins, Player {loserId} loses.");

	#endregion

	#region Helper

	// 同期処理簡略化
	private void SyncAction(ActionSyncManager.ActionType type, int actorId, int cardId = -1)
	{
		if (!PhotonNetwork.IsConnected || ActionSyncManager.Instance == null) return;
		var action = new ActionSyncManager.PlayerActionData
		{
			actionType = type,
			actorId = actorId,
			cardId = cardId
		};
		ActionSyncManager.Instance.SyncPlayerAction(action);
	}

	// 相手プレイヤーのIDを取得
	private int GetOpponentPlayerId(int playerId)
	{
		foreach (var player in players)
		{
			if (player != null && player.PlayerID != playerId)
			{
				return player.PlayerID;
			}
		}
		return -1;
	}

	// GameManager互換性のためのプロパティ
	public TurnPhase CurrentPhaseEnum
	{
		get
		{
			if (currentPhase == Phase.Main) return TurnPhase.Summon;
			if (currentPhase == Phase.Attack) return TurnPhase.Attack;
			return TurnPhase.Summon;
		}
	}

	#endregion

}
