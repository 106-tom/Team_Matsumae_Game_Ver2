using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;

public class AttackManager : MonoBehaviourPun
{
	public static AttackManager Instance;

	private void Awake()
	{
		Instance = this;
	}

	// ================================
	// ユニット → ユニット攻撃
	// ================================
	public void AttackUnit(CardInstance attacker, CardInstance defender)
	{
		if (!PlayerManager.LocalPlayer.IsMyTurn) return;
		if (attacker == null || defender == null) return;

		photonView.RPC(nameof(RPC_AttackUnit), RpcTarget.All, attacker.instanceId, defender.instanceId);
	}

	// ================================
	// ユニット → プレイヤー本体攻撃
	// ================================
	public void AttackPlayer(CardInstance attacker)
	{
		if (!PlayerManager.LocalPlayer.IsMyTurn) return;
		if (attacker == null) return;

		photonView.RPC(nameof(RPC_AttackPlayer), RpcTarget.All, attacker.instanceId);
	}

	// =====================================================================
	// RPC：ユニット同士の戦闘処理
	// =====================================================================
	[PunRPC]
	private void RPC_AttackUnit(string attackerId, string defenderId)
	{
		var attacker = FindUnitById(attackerId);
		var defender = FindUnitById(defenderId);

		if (attacker == null || defender == null)
		{
			Debug.LogError("AttackUnit: attacker または defender が見つからない");
			return;
		}

		// 相撃ち処理
		defender.currentHp -= attacker.currentAp;
		attacker.currentHp -= defender.currentAp;

		//Debug.Log($"{attacker.data.cardName} が {defender.data.cardName} を攻撃！");

		CheckDeath(attacker);
		CheckDeath(defender);
	}

	// =====================================================================
	// RPC：プレイヤー本体への攻撃
	// =====================================================================
	[PunRPC]
	private void RPC_AttackPlayer(string attackerId)
	{
		var attacker = FindUnitById(attackerId);
		if (attacker == null)
		{
			Debug.LogError("AttackPlayer: attacker が見つからない");
			return;
		}

		var enemy = PlayerManager.RemotePlayer;
		enemy.Damage(attacker.currentAp);

		//Debug.Log($"{attacker.data.cardName} が プレイヤーに {attacker.currentAp} ダメージ！");
	}

	// =====================================================================
	// ID から CardInstance を探す
	// =====================================================================
	private CardInstance FindUnitById(string id)
	{
		foreach (var u in PlayerManager.LocalPlayer.Battlefield)
			if (u.instanceId == id) return u;

		foreach (var u in PlayerManager.RemotePlayer.Battlefield)
			if (u.instanceId == id) return u;

		return null;
	}

	// =====================================================================
	// HPが0以下なら戦場から削除
	// =====================================================================
	private void CheckDeath(CardInstance unit)
	{
		if (unit.currentHp > 0) return;

		if (PlayerManager.LocalPlayer.Battlefield.Contains(unit))
		{
			PlayerManager.LocalPlayer.Battlefield.Remove(unit);
			PlayerManager.LocalPlayer.Graveyard.Add(unit.cardData); // data ではなくそのまま追加
		}
		else if (PlayerManager.RemotePlayer.Battlefield.Contains(unit))
		{
			PlayerManager.RemotePlayer.Battlefield.Remove(unit);
			PlayerManager.RemotePlayer.Graveyard.Add(unit.cardData);
		}

		//Debug.Log($"{unit.data.cardName} は倒された！");
	}

}
