//using System.Collections.Generic;
//using UnityEngine;

//public class PlayerFieldAI : MonoBehaviour
//{
//	[Header("Field Settings")]
//	public Transform fieldParent;              // 3D盤面の親
//	public GameObject fieldCardPrefab;          // ★ 3DカードPrefab

//	private List<GameObject> fieldCards = new List<GameObject>();

//	private const int MAX_FIELD = 5;

//	[Header("Layout")]
//	[SerializeField] private float offsetX = 1.5f;
//	[SerializeField] private float baseY = 0f;
//	[SerializeField] private float baseZ = 0f;

//	// =========================
//	// 召喚受付
//	// =========================
//	//public void AcceptSummonedCard(CardAI cardData, SummonSide side)
//	//{
//	//	Debug.Log("[Field] AcceptSummonedCard 呼ばれた");
//	//
//	//	CleanupNullCards();
//	//
//	//	if (fieldCards.Count >= MAX_FIELD)
//	//	{
//	//		Debug.LogWarning("フィールドが満杯です");
//	//		return;
//	//	}
//	//
//	//	GameObject fieldCardGO = Instantiate(fieldCardPrefab, fieldParent);
//	//	Debug.Log("[Field] Prefab生成完了: " + fieldCardGO.name);
//	//
//	//
//	//	fieldCardGO.transform.localPosition = Vector3.zero;
//	//	fieldCardGO.transform.localRotation = Quaternion.identity;
//	//	fieldCardGO.transform.localScale = Vector3.one * 2.0f;
//	//
//	//	CardDisplayAI fieldCard = fieldCardGO.GetComponent<CardDisplayAI>();
//	//	if (fieldCard == null)
//	//	{
//	//		Debug.LogError("CardDisplayAI が fieldCardPrefab に付いていない");
//	//		return;
//	//	}
//	//
//	//	fieldCard.Setup(cardData);
//	//	Debug.Log($"[Field] cardID = {cardData.cardID}");
//	//	fieldCard.OnSummonedToField();
//	//
//	//	// ===== ★ 見た目側データ（超重要） =====
//	//	var visual = fieldCardGO.GetComponentInChildren<FkingCardDisplayAttacher>();
//	//	if (visual != null)
//	//	{
//	//		visual.ChangeCard(cardData.cardID);
//	//		visual.SetDebugMode(
//	//			FkingCardDisplayAttacher.DebugDisplayMode.BattleFrame
//	//		);
//	//	}
//	//	else
//	//	{
//	//		Debug.LogError("FkingCardDisplayAttacher が見つからない");
//	//	}
//	//
//	//
//	//	fieldCards.Add(fieldCardGO);
//	//
//	//	UpdateFieldLayout();
//	//}


//	// =========================
//	// 召喚可能チェック
//	// =========================

//	public void AcceptSummonedCard(CardAI cardData, SummonSide side)
//	{
//		Debug.Log("[Field] AcceptSummonedCard 呼ばれた");

//		CleanupNullCards();

//		if (fieldCards.Count >= MAX_FIELD)
//		{
//			Debug.LogWarning("フィールドが満杯です");
//			return;
//		}

//		GameObject fieldCardGO = Instantiate(fieldCardPrefab, fieldParent);
//		Debug.Log("[Field] Prefab生成完了: " + fieldCardGO.name);

//		fieldCardGO.transform.localPosition = Vector3.zero;
//		fieldCardGO.transform.localRotation = Quaternion.identity;
//		fieldCardGO.transform.localScale = Vector3.one * 2.0f;

//		// ===== 論理カード =====
//		FieldCardDisplayAI fieldCard =
//			fieldCardGO.GetComponent<FieldCardDisplayAI>();

//		if (fieldCard == null)
//		{
//			Debug.LogError("FieldCardDisplayAI が fieldCardPrefab に付いていない");
//			return;
//		}

//		// ★ ここだけで ownerSide を渡す（唯一の入口）
//		fieldCard.Setup(
//			cardData,
//			side == SummonSide.Player
//				? PlayerSide.Self
//				: PlayerSide.Enemy
//		);


//		// ===== 見た目 =====
//		var visual = fieldCardGO.GetComponentInChildren<FkingCardDisplayAttacher>();
//		if (visual != null)
//		{
//			visual.ChangeCard(cardData.cardID);
//			visual.SetDebugMode(
//				FkingCardDisplayAttacher.DebugDisplayMode.BattleFrame
//			);
//		}
//		else
//		{
//			Debug.LogError("FkingCardDisplayAttacher が見つからない");
//		}

//		fieldCards.Add(fieldCardGO);
//		UpdateFieldLayout();
//	}



//	public bool CanSummon()
//	{
//		CleanupNullCards();
//		return fieldCards.Count < MAX_FIELD;
//	}

//	// =========================
//	// 盤面レイアウト更新
//	// =========================
//	private void UpdateFieldLayout()
//	{
//		CleanupNullCards();

//		int count = fieldCards.Count;
//		if (count == 0) return;

//		float centerIndex = (count - 1) * 0.5f;

//		for (int i = 0; i < count; i++)
//		{
//			GameObject cardGO = fieldCards[i];
//			if (cardGO == null) continue;

//			float x = (i - centerIndex) * offsetX;
//			x *= 200;

//			Transform t = cardGO.transform;
//			t.localPosition = new Vector3(x, baseY, baseZ);
//			t.localRotation = Quaternion.identity;
//		}
//	}


//	// =========================
//	// null掃除
//	// =========================
//	private void CleanupNullCards()
//	{
//		for (int i = fieldCards.Count - 1; i >= 0; i--)
//		{
//			if (fieldCards[i] == null)
//			{
//				fieldCards.RemoveAt(i);
//			}
//		}
//	}

//	// =========================
//	// 破壊処理
//	// =========================
//	public void RemoveFieldCard(GameObject cardGO)
//	{
//		if (fieldCards.Contains(cardGO))
//		{
//			fieldCards.Remove(cardGO);
//		}

//		Destroy(cardGO);
//		UpdateFieldLayout();
//	}



//}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerFieldAI : MonoBehaviour
{
	[Header("Field Settings")]
	public Transform fieldParent;
	public GameObject fieldCardPrefab;

	private List<FieldCardDisplayAI> fieldCards
		= new List<FieldCardDisplayAI>();

	private const int MAX_FIELD = 5;

	[Header("Layout")]
	[SerializeField] private float offsetX = 1.5f;
	[SerializeField] private float baseY = 0f;
	[SerializeField] private float baseZ = 0f;

	// =========================
	// 召喚受付
	// =========================
	public FieldCardDisplayAI AcceptSummonedCard(CardAI cardData, SummonSide side)
	{
		Debug.Log("[Field] AcceptSummonedCard");

		CleanupNullCards();

		if (fieldCards.Count >= MAX_FIELD)
		{
			Debug.LogWarning("フィールドが満杯です");
			return null;
		}

		GameObject fieldCardGO =
			Instantiate(fieldCardPrefab, fieldParent);
		fieldCardGO.name = cardData.cardName + "フィールド";
		Debug.Log($"fieldCardGO.name {fieldCardGO.name}",this);

		fieldCardGO.transform.localRotation = Quaternion.identity;
		fieldCardGO.transform.localScale = Vector3.one * 2.0f;
		fieldCardGO.transform.position = cardData.position;
		StartCoroutine(MotionManager.Instance.summon.StartSummon(fieldParent, fieldCardGO));
		//HandManagerAI.Instance.ArrangeHand();
		//fieldCardGO.transform.localPosition = Vector3.zero;

	

		FieldCardDisplayAI fieldCard =
			fieldCardGO.GetComponent<FieldCardDisplayAI>();

		if (fieldCard == null)
		{
			Debug.LogError(
				"FieldCardDisplayAI が fieldCardPrefab に付いていない"
			);
			Destroy(fieldCardGO);
			return null;
		}

		fieldCard.Setup(
			cardData,
			side == SummonSide.Player
			? PlayerSide.Self
			: PlayerSide.Enemy
		);

		fieldCard.Location = CardLocation.Field;

		fieldCard.OwnerSide =
		side == SummonSide.Player
			? PlayerSide.Self
			: PlayerSide.Enemy;

		// ===== 見た目 =====
		var visual =
			fieldCardGO.GetComponentInChildren<FkingCardDisplayAttacher>();

		if (visual != null)
		{
			visual.ChangeCard(cardData.cardID);
			visual.SetDebugMode(
				FkingCardDisplayAttacher.DebugDisplayMode.BattleFrame
			);
		}
		else
		{
			Debug.LogError("FkingCardDisplayAttacher が見つからない");
		}

		fieldCards.Add(fieldCard);
		//UpdateFieldLayout();

		this.StartCoroutine(fieldCardGO.GetComponentInChildren<FkingCardDisplay>().ShowStats());

		return fieldCard; // ★ 召喚された実体を返す
	}


	// =========================
	// 召喚可能チェック
	// =========================
	public bool CanSummon()
	{
		CleanupNullCards();
		return fieldCards.Count < MAX_FIELD;
	}

	// =========================
	// 盤面レイアウト更新
	// =========================
	private void UpdateFieldLayout()
	{
		CleanupNullCards();

		int count = fieldCards.Count;
		if (count == 0) return;

		float centerIndex = (count - 1) * 0.5f;

		for (int i = 0; i < count; i++)
		{
			FieldCardDisplayAI card = fieldCards[i];
			if (card == null) continue;

			float x = (i - centerIndex) * offsetX * 200f;

			Transform t = card.transform;
			t.localPosition = new Vector3(x, baseY, baseZ);
			t.localRotation = Quaternion.identity;
		}
	}

	// =========================
	// null掃除
	// =========================
	private void CleanupNullCards()
	{
		for (int i = fieldCards.Count - 1; i >= 0; i--)
		{
			if (fieldCards[i] == null)
			{
				fieldCards.RemoveAt(i);
			}
		}
	}

	// =========================
	// 破壊処理（正式ルート）
	// =========================
	public void RemoveFieldCard(FieldCardDisplayAI card)
	{
		if (card == null) return;

		if (fieldCards.Contains(card))
		{
			fieldCards.Remove(card);
		}

		Destroy(card.gameObject);
		UpdateFieldLayout();
	}

	// =========================
	// BP指定で破壊（★ 今回の本命）
	// =========================
	public void DestroyMonsterWithBPBelow(int maxBP)
	{
		CleanupNullCards();

		FieldCardDisplayAI target =
			fieldCards.FirstOrDefault(c => c.Attack <= maxBP);

		if (target == null)
		{
			Debug.Log($"[Field] BP{maxBP}以下の対象なし");
			return;
		}

		Debug.Log(
			$"[Field] {target.CardName} (BP {target.Attack}) を破壊"
		);

		RemoveFieldCard(target);
	}

	// =========================
	// 外部参照用
	// =========================
	public List<FieldCardDisplayAI> GetAllCards()
	{
		CleanupNullCards();
		return new List<FieldCardDisplayAI>(fieldCards);
	}

	// =========================
	// ★ レスト解除（ターン開始用）
	// =========================
	public void UnrestAllCards()
	{
		CleanupNullCards();

		foreach (var card in fieldCards)
		{
			if (card == null) continue;

			card.Unrest();
		}

		Debug.Log("[Field] 全カードをアンレストしました");
	}

}

