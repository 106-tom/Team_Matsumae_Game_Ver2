using Photon.Pun;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class ManaUI : MonoBehaviour
{
	public static ManaUI Instance;

	public GameObject manaPanel;
	public Button redButton;
	public Button blueButton;
	public Button greenButton;
	public Button yellowButton;
	public Button purpleButton;

	public TMP_Text redText;
	public TMP_Text blueText;
	public TMP_Text greenText;
	public TMP_Text yellowText;
	public TMP_Text purpleText;

	[SerializeField] private Transform manaArea;
	[SerializeField] private GameObject manaPrefab;

	[Header("各マナ用Sprite")]
	[SerializeField] private Sprite redManaSprite;
	[SerializeField] private Sprite blueManaSprite;
	[SerializeField] private Sprite greenManaSprite;
	[SerializeField] private Sprite yellowManaSprite;
	[SerializeField] private Sprite purpleManaSprite;

	private Queue<GameObject> manaSlots = new Queue<GameObject>();
	private const int MaxMana = 10;

	private void Awake()
	{
		Instance = this;
	}

	// GameStartInitializer から呼ぶ
	public void Init()
	{
		// ボタンイベント登録（Start ではなくここでやる）
		redButton.onClick.AddListener(() => OnManaSelected(ManaColor.Red));
		blueButton.onClick.AddListener(() => OnManaSelected(ManaColor.Blue));
		greenButton.onClick.AddListener(() => OnManaSelected(ManaColor.Green));
		yellowButton.onClick.AddListener(() => OnManaSelected(ManaColor.Yellow));
		purpleButton.onClick.AddListener(() => OnManaSelected(ManaColor.Purple));

		manaPanel.SetActive(false);
		Debug.Log("[ManaUI] Init 完了");
	}

	public void ShowManaOptions()
	{
		Debug.Log("ShowManaOptions called. IsMyTurn: " + PlayerManager.LocalPlayer.IsMyTurn);
		if (!PlayerManager.LocalPlayer.IsMyTurn) return;
		manaPanel.SetActive(true);
	}

	private void OnManaSelected(ManaColor color)
	{
		// 選択した色のマナを 1 追加
		ManaManager2.Instance.AddMana(color);

		// マナパネルを閉じる
		manaPanel.SetActive(false);

		// フェーズ進行
		//PhaseManager.Instance.ProceedPhase();
	}


	public void UpdateUI(Dictionary<ManaColor, int> manaPool)
	{
		if (manaPool == null) return;

		redText.text = manaPool[ManaColor.Red].ToString();
		blueText.text = manaPool[ManaColor.Blue].ToString();
		greenText.text = manaPool[ManaColor.Green].ToString();
		yellowText.text = manaPool[ManaColor.Yellow].ToString();
		purpleText.text = manaPool[ManaColor.Purple].ToString();
	}

	public void AddManaToUI(ManaColor color)
	{
		if (manaSlots.Count >= MaxMana)
		{
			GameObject oldMana = manaSlots.Dequeue();
			Destroy(oldMana);
		}

		GameObject newMana = Instantiate(manaPrefab, manaArea);
		newMana.GetComponent<Image>().sprite = GetSprite(color);
		manaSlots.Enqueue(newMana);
	}

	private Sprite GetSprite(ManaColor color)
	{
		switch (color)
		{
			case ManaColor.Red: return redManaSprite;
			case ManaColor.Blue: return blueManaSprite;
			case ManaColor.Green: return greenManaSprite;
			case ManaColor.Yellow: return yellowManaSprite;
			case ManaColor.Purple: return purpleManaSprite;
			default: return null;
		}
	}
}
