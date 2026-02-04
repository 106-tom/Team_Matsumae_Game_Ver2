using UnityEngine;
using UnityEngine.EventSystems;

public enum CardLocation
{
	Hand,
	Field
}

public class CardDisplayAI : MonoBehaviour, IPointerClickHandler
{
	public CardAI cardData;

	private CardToFkingVisualBinderAI visualBinder;

	public SummonSide summonSide = SummonSide.Player;

	public PlayerSide OwnerSide { get; set; } = PlayerSide.Self;
	public CardLocation Location { get; set; } = CardLocation.Hand;



	void Awake()
	{
		visualBinder = GetComponent<CardToFkingVisualBinderAI>();
	}

	/// <summary>
	/// 手札生成時に呼ばれる
	/// </summary>
	public void Setup(CardAI card)
	{
		cardData = card;

		if (visualBinder != null)
		{
			visualBinder.ApplyCardData(card);
		}
		else
		{
			Debug.LogWarning("CardToFkingVisualBinder がありません", this);
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (PhaseManagerAI.Instance == null) return;

		Transform myParent = transform.parent;
		bool isInHand =
		(myParent == SummonManagerAI.Instance.playerDeck.handParent) ||
		(myParent == SummonManagerAI.Instance.enemyDeck.handParent);

		if (!isInHand)
		{
			Debug.Log("これは手札ではないので召喚できません");
			return;
		}


		if (Location != CardLocation.Hand) return;
		PlayerSide currentTurn = TurnManagerAI.Instance.CurrentTurnSide;

		if (OwnerSide != currentTurn) return;

		// ★召喚フェーズのみ
		if (PhaseManagerAI.Instance.currentPhase != PhaseManagerAI.Phase.Summon)
			return;

		// ★召喚カードなら召喚
		if (cardData.type == CardAI.Type.召喚カード)
		{
			summonSide =
				PhaseManagerAI.Instance.currentPlayerIndex == 0
				? SummonSide.Player
				: SummonSide.Enemy;
			cardData.position = 
				new Vector3(transform.localPosition.x, transform.localPosition.y - 150f, transform.localPosition.z); // 2026/02/03 追加 召喚する際に使う座標
			
			Debug.Log("cardDataPosition : " + cardData.position);
			Debug.Log("lcoalPosition : " + transform.localPosition);
			SummonManagerAI.Instance.TrySummon(cardData, this, summonSide);
		}
		
		// ★魔法カードなら使用
		if (cardData.type == CardAI.Type.魔法カード)
		{
			summonSide =
				PhaseManagerAI.Instance.currentPlayerIndex == 0
				? SummonSide.Player
				: SummonSide.Enemy;

			SummonManagerAI.Instance.TrySpell(cardData, this, summonSide);
		}
	}


	public void OnSummonedToField()
	{
		Location = CardLocation.Field;
		gameObject.layer = LayerMask.NameToLayer("Default");
	}
}

