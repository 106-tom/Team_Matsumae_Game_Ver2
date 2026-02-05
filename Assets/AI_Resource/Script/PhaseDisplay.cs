using UnityEngine;
using TMPro;

public class PhaseDisplayUI : MonoBehaviour
{
	[Header("PhaseManager 参照")]
	public PhaseManagerAI phaseManager; // Inspectorで設定
	[Header("表示するテキスト")]
	public TMP_Text phaseText;          // 板の上に置く TextMeshPro

	private PhaseManagerAI.Phase lastPhase;

	void Start()
	{
		if (phaseManager == null)
		{
			Debug.LogError("PhaseManager が設定されていません！");
			return;
		}

		if (phaseText == null)
		{
			Debug.LogError("phaseText が設定されていません！");
			return;
		}

		// 初回更新
		UpdatePhaseText(true);
	}

	void Update()
	{
		// フェーズが変わったら更新
		if (phaseManager.currentPhase != lastPhase)
		{
			UpdatePhaseText();
		}
	}

	public void UpdatePhaseText(bool force = false)
	{
		if (phaseText == null || phaseManager == null) return;

		if (!force && lastPhase == phaseManager.currentPhase) return;

		string playerName = phaseManager.players[phaseManager.currentPlayerIndex].playerName;

		// ★ ここを英語名に変更
		string phaseName = phaseManager.currentPhase.ToString(); // 例：Start, Draw, Mana ...

		phaseText.text = phaseName;

		lastPhase = phaseManager.currentPhase;
	}

}
