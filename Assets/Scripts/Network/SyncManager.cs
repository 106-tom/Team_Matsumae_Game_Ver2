using UnityEngine;
using Photon.Pun;
using System;

public enum ActionType
{
	PlayCard,
	Attack,
	DrawCard,
	ActivateEffect,
	EndTurn
}

[Serializable]
public class GameAction
{
	public ActionType actionType;
	public int actorId;
	public int cardId;
	public int targetId;
	public int value;
	public int randomSeed;
}

public class SyncManager : MonoBehaviourPun
{
	public static SyncManager Instance;

	private void Awake()
	{
		Instance = this;
	}

	//送信（自分の行動を相手へ伝える）
	public void SendAction(ActionType type, int cardId = -1, int targetId = -1, int value = 0)
	{
		GameAction action = new GameAction()
		{
			actionType = type,
			actorId = PhotonNetwork.LocalPlayer.ActorNumber,
			cardId = cardId,
			targetId = targetId,
			value = value,
			randomSeed = UnityEngine.Random.Range(0, 99999)
		};

		// 自分でも実行
		GameActionExecutor.Execute(action);

		// 相手に送信
		string json = JsonUtility.ToJson(action);
		photonView.RPC(nameof(RPC_ReceiveAction), RpcTarget.Others, json);
	}

	//受信（相手の行動を受け取る）
	[PunRPC]
	void RPC_ReceiveAction(string json)
	{
		GameAction action = JsonUtility.FromJson<GameAction>(json);
		GameActionExecutor.Execute(action);
	}
}

public static class GameActionExecutor
{
	public static void Execute(GameAction action)
	{
		switch (action.actionType)
		{
			case ActionType.PlayCard:
				GameActionHandler.PlayCard(action.cardId);
				break;

			case ActionType.Attack:
				GameActionHandler.Attack(action.cardId, action.targetId);
				break;

			case ActionType.DrawCard:
				GameActionHandler.DrawCard(action.value);
				break;

			case ActionType.ActivateEffect:
				GameActionHandler.ActivateEffect(action.cardId, action.targetId, action.value);
				break;

			case ActionType.EndTurn:
				GameActionHandler.EndTurn();
				break;
		}
	}
}

public static class GameActionHandler
{
	public static void PlayCard(int cardId)
	{
		Debug.Log($"カード {cardId} をプレイしました。");
		// 実際のカード召喚処理をここに
	}

	public static void Attack(int attackerId, int targetId)
	{
		Debug.Log($"カード {attackerId} が {targetId} を攻撃！");
		// 攻撃演出＋HP計算処理など
	}

	public static void DrawCard(int count)
	{
		Debug.Log($"{count} 枚カードをドローしました。");
		// 山札処理
	}

	public static void ActivateEffect(int cardId, int targetId, int value)
	{
		Debug.Log($"カード {cardId} の効果を発動！（対象: {targetId}, 値: {value}）");
		// 効果発動処理
	}

	public static void EndTurn()
	{
		Debug.Log("ターン終了！");
		// ターン交代処理など
	}
}
