using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Advertisements;

public enum PlayerSide
{
	Self,
	Enemy
}

public class PlayerHpUIAI : MonoBehaviour
{
	[Header("Player Info")]
	public PlayerSide side;     // ★ 追加：このHPは誰のものか

	[Header("HP")]
	public int hp = 20;
	public Text hpText;

	public GameObject panel;

	private bool gameEnded = false; // ★追加：遷移が複数回起きないようにする

	void Start()
	{
		UpdateUI();
	}

	// -----------------------------
	// HPクリック（ライフ受け）
	// -----------------------------
	public void OnClick()
	{
		if (AttackManagerAI.Instance == null) return;
		if (AttackManagerAI.Instance.state != AttackState.Blocking) return;

		// ★ 自分のSideを渡す
		AttackManagerAI.Instance.TakeLifeDamage(side);
	}

	// -----------------------------
	// ダメージ処理
	// -----------------------------
	public void TakeDamage(int value)
	{
		if (gameEnded) return;

		hp -= value;
		UpdateUI();

		if (hp <= 0)
		{
			gameEnded = true;
			GoResult();
		}
	}

	public void Heal(int amount)
	{
		if (gameEnded) return;
		hp += amount;

		// 最大HP制限があるならここで clamp
		// hp = Mathf.Min(hp, maxHp);

		UpdateUI();

		Debug.Log($"[HP] {amount} 回復 → 現在HP={hp}");
	}

	private void GoResult()
	{
		//panel.SetActive(true);
		if(ResultUI.Instance == null) Debug.Log("kita");
		ResultUI.Instance.ShowResult(true);
		// sideがSelfなら自分が負け
		if (side == PlayerSide.Self)
		{
			ResultData.isWin = false; // LOSE
		}
		else
		{
			ResultData.isWin = true; // WIN
		}

		Debug.Log("リザルトへ遷移します");

		SceneManager.LoadScene("ResultScene");
	}

	private void UpdateUI()
	{
		if (hpText)
			hpText.text = $"{hp}";
	}
}
