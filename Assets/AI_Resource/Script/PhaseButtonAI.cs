using UnityEngine;

public class PhaseButtonAI : MonoBehaviour
{
	public PhaseManagerAI phaseManager; // フェーズ管理クラスへの参照

	// ボタンクリックでフェーズを進める
	public void OnClickAdvancePhase()
	{
		if (phaseManager != null && phaseManager.currentPlayerIndex == 0)
		{
			phaseManager.AdvancePhase();
		}
	}
}
