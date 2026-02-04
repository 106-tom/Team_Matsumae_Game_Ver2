using UnityEngine;
using UnityEngine.UI;

public class TurnManagerAI : MonoBehaviour
{
	public enum PlayerType { Human, AI }

	// Instanceを使えるようにする
	public static TurnManagerAI Instance { get; private set; }

	public PlayerType currentTurn = PlayerType.Human;

	[Header("UI")]
	public Text turnText;

	// ============================
	// Instanceセット
	// ============================
	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
	}

	// ============================
	// ターンを切り替える
	// ============================
	public void SwitchTurn()
	{
		currentTurn =
			currentTurn == PlayerType.Human
			? PlayerType.AI
			: PlayerType.Human;

		RestoreAllTemporaryDefense();

		//Debug.Log(currentTurn + "のターン開始");
		UpdateUI();
	}

	// ============================
	// UI更新
	// ============================
	void UpdateUI()
	{
		if (turnText != null)
			turnText.text = $"Turn: {currentTurn}";
	}

	// ============================
	// Defense0解除
	// ============================
	void RestoreAllTemporaryDefense()
	{
		foreach (var card in SummonManagerAI.Instance.playerField.GetAllCards())
			card.RestoreDefenseIfNeeded();

		foreach (var card in SummonManagerAI.Instance.enemyField.GetAllCards())
			card.RestoreDefenseIfNeeded();

		//Debug.Log("[Turn] ターン終了：Defense0効果を解除");
	}

	// ============================
	// 今のターンのPlayerSideを返す
	// ============================
	public PlayerSide CurrentTurnSide
	{
		get
		{
			return (currentTurn == PlayerType.Human)
				? PlayerSide.Self
				: PlayerSide.Enemy;
		}
	}
}
