using UnityEngine;
using UnityEngine.UI; // Text を使う場合
using TMPro; // TextMeshPro の場合はこちら

public class PhaseDisplayUI : MonoBehaviour
{
	public PhaseManagerAI phaseManager; // PhaseManager への参照
	//public Text phaseText;            // フェーズ表示用のText
	public TMP_Text phaseText;     // TextMeshPro の場合はこちら

	void Start()
	{
		if (phaseManager == null)
		{
			Debug.LogError("PhaseManager が設定されていません！");
			return;
		}

		// 最初のフェーズ表示を更新
		UpdatePhaseText();
	}

	public void UpdatePhaseText()
	{
		if (phaseText != null)
		{
			phaseText.text = phaseManager.currentPhase.ToString(); // フェーズ名を表示
		}
	}
}
