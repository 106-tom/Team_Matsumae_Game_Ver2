using UnityEngine;
using Photon.Pun;

public class GameManager2 : MonoBehaviourPunCallbacks
{
	public static GameManager2 Instance;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		// ゲームシーンに来たら、MasterClient が開始処理を担当
		if (PhotonNetwork.IsMasterClient)
		{
			StartCoroutine(WaitPlayersReadyInGame());
		}
	}

	// ============================================
	// ゲームシーン内で PlayerManager が2つ揃うのを待つ
	// ============================================
	private System.Collections.IEnumerator WaitPlayersReadyInGame()
	{
		while (PlayerManager.LocalPlayer == null || PlayerManager.RemotePlayer == null)
		{
			yield return null;
		}

		StartGame();
	}

	// ============================================
	// ゲーム開始処理
	// ============================================
	private void StartGame()
	{
		Debug.Log("Game Start!");

		// 先攻後攻を MasterClient が決める
		bool firstTurn = Random.Range(0, 2) == 0;

		photonView.RPC(nameof(RPC_SetFirstTurn), RpcTarget.All, firstTurn);

		// 初期手札ドロー
		photonView.RPC(nameof(RPC_InitialDraw), RpcTarget.All);
	}

	// ============================================
	// 先攻・後攻を同期
	// ============================================
	[PunRPC]
	private void RPC_SetFirstTurn(bool masterFirst)
	{
		bool myTurn;

		if (PhotonNetwork.IsMasterClient)
		{
			myTurn = masterFirst;
		}
		else
		{
			myTurn = !masterFirst;
		}

		PlayerManager.LocalPlayer.SetTurn(myTurn);
	}

	// ============================================
	// 初期ドロー
	// ============================================
	[PunRPC]
	private void RPC_InitialDraw()
	{
		for (int i = 0; i < 5; i++)
		{
			DrawOneCard(PlayerManager.LocalPlayer);
		}
	}

	private void DrawOneCard(PlayerManager player)
	{
		if (player.Deck.Count == 0) return;

		// デッキからカードデータを取得
		CardData2 cardData = player.Deck[0];
		player.Deck.RemoveAt(0);

		// CardInstance に変換して手札に追加
		CardInstance cardInstance = new CardInstance(cardData);
		player.Hand.Add(cardInstance);
	}
}
