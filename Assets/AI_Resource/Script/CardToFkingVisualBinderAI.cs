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
		if (card == null) return;

		// このカードの持ち主を取得
		CardDisplayAI cd = GetComponent<CardDisplayAI>();

		// ★敵かどうか判定
		bool isAI = (cd.OwnerSide == PlayerSide.Enemy);

		// ★裏か表かを渡す
		visualAttacher.ApplyCardData(card, isAI);
	}

}
