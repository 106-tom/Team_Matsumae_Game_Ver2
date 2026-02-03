using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

//状態の同期処理マネージャー
//手札・山札・墓地・体力・マナ

public class SituationSyncManager : MonoBehaviourPunCallbacks
{
	public static SituationSyncManager Instance { get; private set; }

	private void Awake()
	{
		if (Instance == null) Instance = this;
		else Destroy(gameObject);
	}

	//同期データ
	[Header("デッキ・手札・墓地")]
	public List<int> playerDeck			= new();
	public List<int> playerHand			= new();
	public List<int> playerGraveyard	= new();

	public List<int> opponentDeck		= new();
	public List<int> opponentHand		= new();
	public List<int> opponentGraveyard	= new();

	[Header("ターン情報")]
	public int currentTurnPlayerId;		//現在のターンのplayerID
	public int turnCount;               //ターン数

	[Header("リソース / ステータス")]
	public int playerMana;
	public int opponentMana;
	public int playerHP;
	public int opponentHP;
	public int playerScore;
	public int opponentScore;

	[Header("フィールド上のカード")]
	public List<CardState> fieldCards	= new();

	[Header("その他の同期要素")]
	public string	lastAction;
	public string	weatherEffect;
	public float	remainingTime;
	public bool		isPaused;

	//構造体・クラス
	[System.Serializable]
	public class CardState
	{
		public int		cardId;
		public int		ownerId;
		public Vector3	position;
		public bool		isFaceUp;
		public bool		isTapped;
	}

	//同期用メソッド

	//デッキ同期
	public void SyncDeck(List<int> newDeck, bool isPlayer)
	{
		if (isPlayer) playerDeck = new List<int>(newDeck);
		else opponentDeck = new List<int>(newDeck);

		photonView.RPC(nameof(RPC_SyncDeck), RpcTarget.Others, newDeck.ToArray(), isPlayer);
	}

	[PunRPC]
	private void RPC_SyncDeck(int[] newDeck, bool isPlayer)
	{
		if (isPlayer) playerDeck = new List<int>(newDeck);
		else opponentDeck = new List<int>(newDeck);
	}

	//手札同期
	public void SyncHand(List<int> newHand, bool isPlayer)
	{
		if (isPlayer) playerHand = new List<int>(newHand);
		else opponentHand = new List<int>(newHand);

		photonView.RPC(nameof(RPC_SyncHand), RpcTarget.Others, newHand.ToArray(), isPlayer);
	}

	[PunRPC]
	private void RPC_SyncHand(int[] newHand, bool isPlayer)
	{
		if (isPlayer) playerHand = new List<int>(newHand);
		else opponentHand = new List<int>(newHand);
	}

	//墓地同期
	public void SyncGraveyard(List<int> newGraveyard, bool isPlayer)
	{
		if (isPlayer) playerGraveyard = new List<int>(newGraveyard);
		else opponentGraveyard = new List<int>(newGraveyard);

		photonView.RPC(nameof(RPC_SyncGraveyard), RpcTarget.Others, newGraveyard.ToArray(), isPlayer);
	}

	[PunRPC]
	private void RPC_SyncGraveyard(int[] newGraveyard, bool isPlayer)
	{
		if (isPlayer) playerGraveyard = new List<int>(newGraveyard);
		else opponentGraveyard = new List<int>(newGraveyard);
	}

	//ターン情報同期
	public void SyncTurn(int currentTurnPlayer, int turnCountNow)
	{
		currentTurnPlayerId = currentTurnPlayer;
		turnCount = turnCountNow;

		photonView.RPC(nameof(RPC_SyncTurn), RpcTarget.Others, currentTurnPlayer, turnCountNow);
	}

	[PunRPC]
	private void RPC_SyncTurn(int currentTurnPlayer, int turnCountNow)
	{
		currentTurnPlayerId = currentTurnPlayer;
		turnCount = turnCountNow;
	}

	//HP・マナ・スコア同期
	public void SyncStatus(int pHP, int oHP, int pMana, int oMana, int pScore, int oScore)
	{
		playerHP = pHP;
		opponentHP = oHP;
		playerMana = pMana;
		opponentMana = oMana;
		playerScore = pScore;
		opponentScore = oScore;

		photonView.RPC(nameof(RPC_SyncStatus), RpcTarget.Others, pHP, oHP, pMana, oMana, pScore, oScore);
	}

	[PunRPC]
	private void RPC_SyncStatus(int pHP, int oHP, int pMana, int oMana, int pScore, int oScore)
	{
		playerHP = pHP;
		opponentHP = oHP;
		playerMana = pMana;
		opponentMana = oMana;
		playerScore = pScore;
		opponentScore = oScore;
	}

	//フィールド上のカード同期
	public void SyncFieldCards(List<CardState> newField)
	{
		fieldCards = new List<CardState>(newField);
		photonView.RPC(nameof(RPC_SyncFieldCards), RpcTarget.Others, JsonUtility.ToJson(new FieldWrapper(newField)));
	}

	[PunRPC]
	private void RPC_SyncFieldCards(string json)
	{
		var wrapper = JsonUtility.FromJson<FieldWrapper>(json);
		fieldCards = wrapper.cards;
	}

	[System.Serializable]
	private class FieldWrapper
	{
		public List<CardState> cards;
		public FieldWrapper(List<CardState> cards) { this.cards = cards; }
	}

	//ゲーム全体同期
	public void SyncGameState()
	{
		photonView.RPC(nameof(RPC_SyncGameState), RpcTarget.Others,
			currentTurnPlayerId, turnCount,
			playerHP, opponentHP,
			playerMana, opponentMana,
			playerScore, opponentScore,
			lastAction, weatherEffect,
			remainingTime, isPaused);
	}

	[PunRPC]
	private void RPC_SyncGameState(
		int turnPlayer, int turnCountNow,
		int pHP, int oHP,
		int pMana, int oMana,
		int pScore, int oScore,
		string lastAct, string weather,
		float time, bool paused)
	{
		currentTurnPlayerId = turnPlayer;
		turnCount = turnCountNow;
		playerHP = pHP;
		opponentHP = oHP;
		playerMana = pMana;
		opponentMana = oMana;
		playerScore = pScore;
		opponentScore = oScore;
		lastAction = lastAct;
		weatherEffect = weather;
		remainingTime = time;
		isPaused = paused;
	}
}