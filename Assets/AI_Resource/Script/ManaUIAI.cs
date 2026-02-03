using UnityEngine;
using UnityEngine.UI;

public class ManaUIAI : MonoBehaviour
{
	public static ManaUIAI Instance { get; private set; }

	public GameObject manaPanel;
	public Button redButton;
	public Button blueButton;
	public Button greenButton;
	public Button yellowButton;
	public Button purpleButton;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		redButton.onClick.AddListener(() => OnManaSelected("Red"));
		blueButton.onClick.AddListener(() => OnManaSelected("Blue"));
		greenButton.onClick.AddListener(() => OnManaSelected("Green"));
		yellowButton.onClick.AddListener(() => OnManaSelected("Yellow"));
		purpleButton.onClick.AddListener(() => OnManaSelected("Purple"));
	}

	/// <summary>
	/// マナを選んだ時に呼ばれる
	/// </summary>
	private void OnManaSelected(string color)
	{
		Debug.Log("選ばれたマナ: " + color);

		// 今ターンのプレイヤーを取得
		var pm = PhaseManagerAI.Instance;
		if (pm == null)
		{
			Debug.LogError("PhaseManagerAI.Instance が null");
			return;
		}

		int idx = pm.currentPlayerIndex;
		if (pm.players == null || idx < 0 || idx >= pm.players.Length)
		{
			Debug.LogError("players 配列が不正 or currentPlayerIndex が範囲外");
			return;
		}

		var currentPlayer = pm.players[idx];
		if (currentPlayer.manaManager == null)
		{
			Debug.LogError($"{currentPlayer.playerName} の manaManager が設定されていません");
			return;
		}

		// 1. マナを今ターンのプレイヤーのマナゾーンへ追加
		currentPlayer.manaManager.AddMana(color);

		// 2. パネルを閉じる
		manaPanel.SetActive(false);

		// 3. フェーズを進める
		PhaseManagerAI.Instance.OnManaSelected();
	}



	public void OpenPanel()
	{
		manaPanel.SetActive(true);
	}

}
