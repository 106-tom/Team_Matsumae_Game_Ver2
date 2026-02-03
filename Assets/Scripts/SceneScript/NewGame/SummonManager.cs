using Photon.Pun;
using UnityEngine;

public class SummonManager : MonoBehaviourPun
{
	public static SummonManager Instance;

	private void Awake()
	{
		Instance = this;
	}

	// ==============================================================  
	// 実際にカードを召喚する処理（簡易版）
	// ==============================================================  
	public bool Summon(CardInstance cardInstance)
	{
		var player = PlayerManager.LocalPlayer;

		// 自分のターンじゃなければ召喚不可
		if (!player.IsMyTurn)
		{
			Debug.Log("自分のターンではありません！");
			return false;
		}

		// 手札に存在しないカードを指定された場合
		if (!player.Hand.Contains(cardInstance))
		{
			Debug.Log("手札にそのカードはありません！");
			return false;
		}

		// マナが足りるか（とりあえず赤マナだけ消費）
		if (!ManaManager2.Instance.UseMana(ManaColor.Red, cardInstance.cardData.cost))
		{
			Debug.Log("マナ不足で召喚できません！");
			return false;
		}

		// 手札からフィールドへ移動
		player.Hand.Remove(cardInstance);
		player.Battlefield.Add(cardInstance);

		Debug.Log($"召喚成功：{cardInstance.cardData.cardName}");

		// 相手に同期（カードIDだけ送る）
		photonView.RPC(nameof(RPC_Summon), RpcTarget.Others, cardInstance.cardData.cardId);

		return true;
	}

	// ==============================================================  
	// 相手側の召喚同期（詳細データは相手クライアントが持っている前提）
	// ==============================================================  
	[PunRPC]
	private void RPC_Summon(int cardId)
	{
		var enemy = PlayerManager.RemotePlayer;

		// カードIDからデッキやデータベースを検索
		CardData2 card = CardDatabase2.GetCardById(cardId);

		if (card == null)
		{
			Debug.LogError("カードIDが不正");
			return;
		}

		// 相手側は CardInstance を作って戦場に置く
		CardInstance instance = new CardInstance(card);
		enemy.Battlefield.Add(instance);

		Debug.Log($"相手が召喚：{card.cardName}");
	}
}
