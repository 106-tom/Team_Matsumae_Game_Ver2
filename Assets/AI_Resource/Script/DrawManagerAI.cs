using UnityEngine;

public class DrawManagerAI : MonoBehaviour
{
	[Header("デッキ管理")]
	public PlayerDeckAI playerDeck; // Inspectorでセット

	[Header("フェーズ管理")]
	public PhaseManagerAI phaseManager; // Drawフェーズで呼ぶ想定

	void Update()
	{
		// デバッグ用: エンターキーでドロー
		if (Input.GetKeyDown(KeyCode.D))
		{
			if (phaseManager != null && phaseManager.currentPhase == PhaseManagerAI.Phase.Draw)
			{
				DrawCard();
			}
		}
	}

	// ドロー処理
	public void DrawCard()
	{
		if (playerDeck == null)
		{
			Debug.LogWarning("PlayerDeckがセットされていません");
			return;
		}

		PlayerSide drawingPlayer = TurnManagerAI.Instance.CurrentTurnSide;

		Debug.Log("★★★★ drawingPlayer = " + drawingPlayer);

		playerDeck.DrawCard(drawingPlayer);
	}

	// 複数枚ドローしたい場合
	public void DrawCards(int count)
	{
		for (int i = 0; i < count; i++)
		{
			DrawCard();
		}
	}
}
