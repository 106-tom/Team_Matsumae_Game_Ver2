using Photon.Pun;   
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Photon.Realtime;

//ターンフェイズ
public enum TurnPhase
{
	Summon,   // カード召喚フェーズ
	Attack    // 攻撃フェーズ
}

//ゲーム全体管理のシングルトンクラス
//ターン管理
//カード操作
//勝敗判定
//マルチ同期
//
//【注意】このクラスは後方互換性のために残されています。
//新しい実装では TurnPhaseManager を優先的に使用してください。
//TurnPhaseManager が存在する場合は、そちらが優先されます。
public class GameManager : MonoBehaviour
{
	#region Singleton & 初期化
	public static GameManager Instance;
	public InGamePlayer[] players = new InGamePlayer[2];
	private void Awake()
	{
		Instance = this;
		InitializePlayers();
		StartTurn(); // ゲーム開始
	}
	
	//プレイヤーの初期化
	void InitializePlayers()
	{
		//player0 = new InGamePlayer(0);
		//player1 = new InGamePlayer(1);
		//
		//Debug.Log("プレイヤー初期化完了");
		Player[] photonPlayers = PhotonNetwork.PlayerList;
		for (int i = 0; i < photonPlayers.Length; i++)
		{
			Player p = photonPlayers[i];
			string name = p.NickName; // PlayerName は使わない
			int deckID = p.CustomProperties.ContainsKey("DeckID") ? (int)p.CustomProperties["DeckID"] : 0;

			players[i] = new InGamePlayer(p.ActorNumber, name, deckID);
			Debug.Log($"Player {p.ActorNumber} 初期化: {name}, Deck {deckID}");
		}
	}
	#endregion

	#region プレイヤー管理
	private int currentPlayerIndex = 0; // 配列インデックスで管理

	//private int currentPlayerId = 0;

	public TurnPhase CurrentPhase { get; private set; } = TurnPhase.Summon; // 現在のフェーズ
	//public int CurrentPlayerId { get; private set; } = 0;

	//プレイヤー取得
	// ActorNumber でプレイヤー取得
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
	
	//現在のターンプレイヤー
	public InGamePlayer CurrentPlayer => players[currentPlayerIndex];

	//プレイヤーがターン中か
	public bool IsPlayerTurn(int actorNumber) => CurrentPlayer.PlayerID == actorNumber;
	#endregion

	#region ターン・フェーズ管理
	//ターン開始
	//ドロー
	//マナチャージ
	//フェイズ初期化
	//マルチ同期送信
	public void StartTurn()
	{
		var player = CurrentPlayer;
		player.DrawCard();
		
		if (PhotonNetwork.IsConnected)
		{
			int lastCardId = player.hand[player.hand.Count - 1].id;
			SyncAction(ActionSyncManager.ActionType.DrawCard, player.PlayerID, lastCardId);
		}
		
		player.ChargeMana();
		if (PhotonNetwork.IsConnected)
			SyncAction(ActionSyncManager.ActionType.ChargeMana, player.PlayerID);

		CurrentPhase = TurnPhase.Summon;
		Debug.Log($"Player {player.PlayerID} のターン開始");
		
		if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
			SyncAction(ActionSyncManager.ActionType.StartTurn, player.PlayerID);
	}

	public void EndPhase()
	{
		if (CurrentPhase == TurnPhase.Summon)
		{
			CurrentPhase = TurnPhase.Attack;
			Debug.Log($"Player {CurrentPlayer.PlayerID} 攻撃フェーズへ");

			if (PhotonNetwork.IsConnected)
			{
				var action = new ActionSyncManager.PlayerActionData
				{
					actionType = ActionSyncManager.ActionType.ChangePhase,
					actorId = CurrentPlayer.PlayerID,
					phase = TurnPhase.Attack
				};
				ActionSyncManager.Instance.SyncPlayerAction(action);
			}
		}
		else
		{
			EndTurn();
		}
	}

	public void EndTurn()
	{
		currentPlayerIndex = (currentPlayerIndex + 1) % players.Length;
		CurrentPhase = TurnPhase.Summon;
		Debug.Log($"Next turn: Player {CurrentPlayer.PlayerID}");

		if (PhotonNetwork.IsConnected)
			SyncAction(ActionSyncManager.ActionType.EndTurn, CurrentPlayer.PlayerID);

		StartTurn();
	}

	public void ApplyRemoteEndTurn(int nextActorId)
	{
		var player = GetPlayer(nextActorId);
		currentPlayerIndex = System.Array.IndexOf(players, player);
		CurrentPhase = TurnPhase.Summon;
		Debug.Log($"[Remote] ターン変更: Player {nextActorId}（受信）");
	}

	public void ApplyRemoteStartTurn(int actorId)
	{
		var player = GetPlayer(actorId);
		currentPlayerIndex = System.Array.IndexOf(players, player);
		CurrentPhase = TurnPhase.Summon;

		Debug.Log($"[Remote] Player {actorId} ターン開始");
	}

	public void ApplyRemoteChangePhase(TurnPhase newPhase)
	{
		CurrentPhase = newPhase;
		Debug.Log($"[Remote] フェーズを {newPhase} に変更");
	}

	// 🆕 ここに追加！
	public int CurrentPlayerId => CurrentPlayer?.PlayerID ?? -1;


	#endregion

	#region カード操作
	private Card selectedCard;

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
		Debug.Log($"GameManager: {player.PlayerID} が {card.id} を召喚");
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
		Debug.Log($"Player {owner.PlayerID} のカード {card.id} を破壊"); // ← PlayerID に変更

    }

    // --------------------------------------------
    // カード操作
    // --------------------------------------------
    public void SelectCard(int playerId, Card card)
    {
        if (!IsPlayerTurn(playerId))
            return;
        selectedCard = card;
        Debug.Log($"Player {playerId} selected card: {card.id}");

		if (PhotonNetwork.IsConnected)
			SyncAction(ActionSyncManager.ActionType.Destroy, playerId, card.id); // ← PlayerID に変更
			SyncAction(ActionSyncManager.ActionType.SelectCard, playerId, card.id);
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

	#region RemoteCardOperations
	// 各種リモート操作
	public void ApplyRemoteSelectCard(int actorId, int cardId)
		=> Debug.Log($"[Remote] Player {actorId} selected card {cardId}");
	public void ApplyRemotePlayCard(int actorId, int cardId, Vector3 pos)
		=> Debug.Log($"[Remote] Player {actorId} played card {cardId} at {pos}");
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
		var attacker = player.FindCardById(attackerCardId);

		foreach (int targetId in targetCardIds)
		{
			var target = CurrentPlayer.FindCardById(targetId);
			if (attacker != null && target != null)
				ExecuteAttack(attacker, target); // 送信時の順序通りに攻撃
		}

		Debug.Log($"[Remote] Player {actorId} attacks {string.Join(",", targetCardIds)}");
	}
	public void ApplyRemoteTap(int actorId, int cardId, bool tapped)
	{
		var player = GetPlayer(actorId);
		var target = player.FindCardById(cardId);
		if (target != null)
			target.isTapped = tapped;

		Debug.Log($"[Remote] Player {actorId} {(tapped ? "タップ" : "アンタップ")} card {cardId}");
	}
	public void ApplyRemoteManaCharge(int actorId, int cardId, Vector3 position)
	{
		var player = GetPlayer(actorId);
		if (player == null) return;

		var card = player.FindCardById(cardId);
		if (card == null)
		{
			Debug.LogWarning($"[Sync] Card {cardId} not found for Player{actorId}");
			return;
		}

		// マナゾーンに配置（位置情報を使用）
		player.manaZoneManager.AddCard(card, position);

		Debug.Log($"[Sync] Player{actorId} charged card {cardId} to ManaZone at {position}");
	}

	#endregion
	#endregion

	#region ライフ・勝敗管理
	public void ApplyRemoteLifeChange(int playerId, int delta)
	{
		var player = GetPlayer(playerId);
		if (player == null) return;

		player.currentLife += delta;
		Debug.Log($"[Sync] Player {playerId} Life changed by {delta}, new life: {player.currentLife}");
	}

	public void CheckWinCondition()
	{
		if (GetPlayer(0).currentLife <= 0) EndGame(1);
		else if (GetPlayer(1).currentLife <= 0) EndGame(0);
	}

	public void EndGame(int winnerId)
	{
		int loserId = (winnerId + 1) % 2;
		Debug.Log($"Game Over! Player {winnerId} wins, Player {loserId} loses.");

		if (PhotonNetwork.IsConnected)
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
	//同期処理簡略化
	private void SyncAction(ActionSyncManager.ActionType type, int actorId, int cardId = -1)
	{
		if (!PhotonNetwork.IsConnected) return;
		var action = new ActionSyncManager.PlayerActionData
		{
			actionType = type,
			actorId = actorId,
			cardId = cardId
		};
		ActionSyncManager.Instance.SyncPlayerAction(action);
	}
	#endregion

	//追加
	public void ReferMyField()
    {

    }

	public void ReferEnemyField()
	{

	}
}