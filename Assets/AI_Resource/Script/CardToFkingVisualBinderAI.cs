using UnityEngine;

/// <summary>
/// CardAI（データ） → FkingCardDisplayAttacher（見た目）
/// をつなぐだけの橋渡し役
/// </summary>
[RequireComponent(typeof(FkingCardDisplayAttacher))]
public class CardToFkingVisualBinderAI : MonoBehaviour
{
	private FkingCardDisplayAttacher visualAttacher;

	void Awake()
	{
		visualAttacher = GetComponent<FkingCardDisplayAttacher>();
	}

	/// <summary>
	/// カードデータが確定したタイミングで呼ぶ
	/// </summary>
	public void ApplyCardData(CardAI card)
	{
		if (card == null)
		{
			Debug.LogError("CardAI が null");
			return;
		}

		if (visualAttacher == null)
		{
			Debug.LogError("FkingCardDisplayAttacher が無い");
			return;
		}

		Debug.Log($"[Visual] cardID 適用: {card.cardID}");
		Debug.Log("[Binder] ApplyCardData: " + card.cardID);
		visualAttacher.ChangeCard(card.cardID);
	}

}
