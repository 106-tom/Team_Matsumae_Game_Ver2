using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

// ===============================
// �v���C���[�̍s�������}�l�[�W���[
// ===============================
public class ActionSyncManager : MonoBehaviourPun
{
	public static ActionSyncManager Instance { get; private set; }

	private void Awake()
	{
		if (Instance == null) Instance = this;
		else Destroy(gameObject);
	}

	// �v���C���[�A�N�V�����̎��
	public enum ActionType
	{
		PlayCard,
		DrawCard,
		DiscardCard,
		DestroyCard,
		Destroy,
		Tap,
		Untap,
		Attack,
		ChargeMana,
		SelectCard,
		ChangePhase,
		StartTurn,
		EndTurn,
		Damage,
		GameOver
	}

	// �A�N�V�����f�[�^
	[System.Serializable]
	public class PlayerActionData
	{
		public ActionType actionType;
		public int actorId;
		public int targetPlayerId;
		public int cardId;
		public List<int> targetCardIds;
		public string extraData;
		public Vector3 position;
		public TurnPhase phase;
		public bool isTapped;
		public int lifeDelta;
	}

	// ===============================
	// �ėp�������\�b�h
	// ===============================
	public void SyncPlayerAction(PlayerActionData action)
	{
		string json = JsonUtility.ToJson(action);
		Debug.Log($"[ActionSync] Send: {action.actionType} by P{action.actorId}");
		photonView.RPC(nameof(RPC_ReceivePlayerAction), RpcTarget.Others, json);
	}

	[PunRPC]
	private void RPC_ReceivePlayerAction(string json)
	{
		PlayerActionData action = JsonUtility.FromJson<PlayerActionData>(json);
		Debug.Log($"[ActionSync] Receive: {action.actionType} from P{action.actorId}");
		ApplyPlayerAction(action);
	}

	// ===============================
	// �A�N�V�����K�p����
	// ===============================
	private void ApplyPlayerAction(PlayerActionData action)
	{
		// TurnPhaseManagerを優先的に使用（新しいシステム）
		if (TurnPhaseManager.Instance != null)
		{
			ApplyPlayerActionToTurnPhaseManager(action);
		}
		// 後方互換性のためGameManagerもサポート
		else if (GameManager.Instance != null)
		{
			ApplyPlayerActionToGameManager(action);
		}
	}

	private void ApplyPlayerActionToTurnPhaseManager(PlayerActionData action)
	{
		switch (action.actionType)
		{
			case ActionType.PlayCard:
				TurnPhaseManager.Instance?.ApplyRemotePlayCard(action.actorId, action.cardId, action.position);
				break;
			case ActionType.DrawCard:
				TurnPhaseManager.Instance?.ApplyRemoteDraw(action.actorId, action.cardId);
				break;
			case ActionType.DiscardCard:
				// 未実装
				break;
			case ActionType.DestroyCard:
				TurnPhaseManager.Instance?.ApplyRemoteDestroy(action.actorId, action.cardId);
				break;
			case ActionType.Tap:
				TurnPhaseManager.Instance?.ApplyRemoteTap(action.actorId, action.cardId, action.isTapped);
				break;
			case ActionType.Untap:
				TurnPhaseManager.Instance?.ApplyRemoteTap(action.actorId, action.cardId, false);
				break;
			case ActionType.Attack:
				TurnPhaseManager.Instance?.ApplyRemoteAttack(action.actorId, action.cardId, action.targetCardIds);
				break;
			case ActionType.ChargeMana:
				// マナ色の情報をextraDataから取得
				TurnPhaseManager.Instance?.ApplyRemoteManaCharge(action.actorId, action.extraData ?? "");
				break;
			case ActionType.SelectCard:
				TurnPhaseManager.Instance?.ApplyRemoteSelectCard(action.actorId, action.cardId);
				break;
			case ActionType.ChangePhase:
				// TurnPhaseManagerでは内部でフェーズ管理
				break;
			case ActionType.StartTurn:
				TurnPhaseManager.Instance?.ApplyRemoteStartTurn(action.actorId);
				break;
			case ActionType.EndTurn:
				TurnPhaseManager.Instance?.ApplyRemoteEndTurn(action.actorId);
				break;
			case ActionType.Damage:
				// ライフ変更処理
				TurnPhaseManager.Instance?.ApplyRemoteLifeChange(action.targetPlayerId, action.lifeDelta);
				break;
			case ActionType.GameOver:
				TurnPhaseManager.Instance?.ShowRemoteGameOver(action.actorId, action.targetPlayerId);
				break;
			default:
				Debug.LogWarning($"Unhandled Action: {action.actionType}");
				break;
		}
	}

	private void ApplyPlayerActionToGameManager(PlayerActionData action)
	{
		switch (action.actionType)
		{
			case ActionType.PlayCard:
				GameManager.Instance?.ApplyRemotePlayCard(action.actorId, action.cardId, action.position);
				break;
			case ActionType.DrawCard:
				GameManager.Instance?.ApplyRemoteDraw(action.actorId, action.cardId);
				break;
			case ActionType.DiscardCard:
				//GameManager.Instance?.ApplyRemoteDiscard(action.actorId, action.cardId);
				break;
			case ActionType.DestroyCard:
				GameManager.Instance?.ApplyRemoteDestroy(action.actorId, action.cardId);
				break;
			case ActionType.Tap:
				GameManager.Instance?.ApplyRemoteTap(action.actorId, action.cardId, action.isTapped);
				break;
			case ActionType.Untap:
				GameManager.Instance?.ApplyRemoteTap(action.actorId, action.cardId, false);
				break;
			case ActionType.Attack:
				GameManager.Instance?.ApplyRemoteAttack(action.actorId, action.cardId, action.targetCardIds);
				break;
			case ActionType.ChargeMana:
				GameManager.Instance?.ApplyRemoteManaCharge(action.actorId, action.cardId, action.position);
				break;
			case ActionType.SelectCard:
				GameManager.Instance?.ApplyRemoteSelectCard(action.actorId, action.cardId);
				break;
			case ActionType.ChangePhase:
				GameManager.Instance?.ApplyRemoteChangePhase(action.phase); 
				break;
			case ActionType.StartTurn:
				GameManager.Instance?.ApplyRemoteStartTurn(action.actorId);
				break;
			case ActionType.EndTurn:
				GameManager.Instance?.ApplyRemoteEndTurn(action.actorId);
				break;
			case ActionType.Damage:
				int delta = action.lifeDelta;
				GameManager.Instance?.ApplyRemoteLifeChange(action.targetPlayerId, delta);
				break;
			case ActionType.GameOver:
				GameManager.Instance?.ShowRemoteGameOver(action.actorId, action.targetPlayerId);
				break;
			default:
				Debug.LogWarning($"Unhandled Action: {action.actionType}");
				break;
		}
	}

	// ===============================
	// �֗�Send���\�b�h
	// ===============================
	public void SendAction(ActionType type, int actorId, int cardId = -1, int targetPlayerId = -1,
						   List<int> targetCardIds = null, string extraData = "", Vector3 position = default, TurnPhase phase = TurnPhase.Summon, bool isTapped = false)
	{
		var action = new PlayerActionData
		{
			actionType = type,
			actorId = actorId,
			cardId = cardId,
			targetPlayerId = targetPlayerId,
			targetCardIds = targetCardIds,
			extraData = extraData,
			position = position,
			phase = phase,
			isTapped = isTapped
		};
		SyncPlayerAction(action);
	}

	public void SendLifeChange(int targetPlayerId, int delta)
	{
		var action = new PlayerActionData
		{
			actorId = PhotonNetwork.LocalPlayer.ActorNumber,
			targetPlayerId = targetPlayerId,
			actionType = ActionType.Damage,
			lifeDelta = delta
		};

		SyncPlayerAction(action);
	}
}
