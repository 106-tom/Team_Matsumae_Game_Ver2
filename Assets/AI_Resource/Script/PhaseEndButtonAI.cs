using UnityEngine;

public class PhaseEndButtonAI : MonoBehaviour
{
	public PhaseManagerAI phaseManager; // PhaseManager への参照

	// ボタンを押すとエンドフェーズに進める
	public void OnClickEndPhase()
	{
		if (phaseManager != null)
		{
			phaseManager.currentPhase = PhaseManagerAI.Phase.End; // Endフェーズに直接設定
			//phaseManager.EndTurn();           // 必要ならTurn終了処理も呼ぶ
		}
		else
		{
			Debug.LogError("PhaseManager が設定されていません！");
		}
	}
}
