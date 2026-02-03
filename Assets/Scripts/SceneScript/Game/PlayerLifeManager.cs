using UnityEngine;
using Photon.Pun;

public class PlayerLifeManager : MonoBehaviourPun
{
	public int maxHP = 20;
	public int currentHP;

	void Start()
	{
		currentHP = maxHP;
	}

	// ダメージ処理
	public void TakeDamage(int amount)
	{
		if (!photonView.IsMine) return; // 自分のプレイヤーだけが処理

		currentHP -= amount;
		if (currentHP < 0) currentHP = 0;

		// 同期
		photonView.RPC(nameof(RPC_UpdateHP), RpcTarget.All, currentHP);

		// 勝敗判定
		if (currentHP == 0)
		{
			string winner = photonView.Owner.NickName == PhotonNetwork.NickName ? "Opponent" : "Self";
			PhotonView target = photonView; // 自分の PhotonView
			target.RPC("RPC_GameOver", RpcTarget.All, winner);
		}
	}

	// 回復処理
	public void Heal(int amount)
	{
		if (!photonView.IsMine) return;

		currentHP += amount;
		if (currentHP > maxHP) currentHP = maxHP;

		photonView.RPC(nameof(RPC_UpdateHP), RpcTarget.All, currentHP);
	}

	[PunRPC]
	void RPC_UpdateHP(int hp)
	{
		currentHP = hp;
		Debug.Log($"HP更新: {currentHP}/{maxHP}");
		// ここでUI更新可能
	}

	[PunRPC]
	void RPC_GameOver(string winner)
	{
		Debug.Log($"ゲーム終了！ 勝者: {winner}");

		// シーン遷移（例: GameOverScene へ）
		//UnityEngine.SceneManagement.SceneManager.LoadScene("GameOverScene");
	}
}
