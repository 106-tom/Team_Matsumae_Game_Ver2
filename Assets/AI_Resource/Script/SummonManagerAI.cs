using UnityEngine;

public class SummonManagerAI : MonoBehaviour
{
	public static SummonManagerAI Instance;

	[Header("Player")]
	public PlayerManaManagerAI playerMana;
	public PlayerFieldAI playerField;
	public PlayerDeckAI playerDeck;

	[Header("Enemy")]
	public PlayerManaManagerAI enemyMana;
	public PlayerFieldAI enemyField;
	public PlayerDeckAI enemyDeck;

	void Awake()
	{
		Instance = this;
	}

	public bool TrySummon(CardAI card, CardDisplayAI handCard, SummonSide side)
	{
		//Debug.Log($"card:{card.cardID}");

		PlayerSide currentTurn = TurnManagerAI.Instance.CurrentTurnSide;
		if (handCard.OwnerSide != currentTurn)
		{
			Debug.Log("今のターンのプレイヤー以外は召喚できません！");
			return false;
		}

		side =
		(currentTurn == PlayerSide.Self)
		? SummonSide.Player
		: SummonSide.Enemy;

		PlayerManaManagerAI targetMana =
			(side == SummonSide.Player) ? playerMana : enemyMana;

		PlayerFieldAI targetField =
			(side == SummonSide.Player) ? playerField : enemyField;

		PlayerDeckAI targetDeck =
			(side == SummonSide.Player) ? playerDeck : enemyDeck;

		// フェーズチェック
		if (PhaseManagerAI.Instance.currentPhase != PhaseManagerAI.Phase.Summon)
		{
			Debug.Log("今は召喚フェーズではありません！");
			return false;
		}

		// 盤面チェック
		if (!targetField.CanSummon())
		{
			Debug.Log("盤面がいっぱいです");
			return false;
		}

		// マナチェック
		if (!targetMana.CanPayCost(card))
		{
			Debug.Log("必要マナが足りません！");
			return false;
		}

		// ★ 特殊カードチェック
		if (card.cardID == "55") // ← 対象カードのID
		{
			// 全色2個ずつ揃っているかチェック
			if (!(targetMana.HasColor(ManaColor.Red, 2)
				&& targetMana.HasColor(ManaColor.Blue, 2)
				&& targetMana.HasColor(ManaColor.Green, 2)
				&& targetMana.HasColor(ManaColor.Yellow, 2)
				&& targetMana.HasColor(ManaColor.Purple, 2)))
			{
				Debug.Log("[Summon] 特殊カードは全色2個ずつ必要です");
				return false;
			}
		}


		// ===== 召喚成功 =====
		targetMana.PayCost(card);

		// 召喚カードを盤面に配置して、戻り値を取得
		FieldCardDisplayAI summonedCard = targetField.AcceptSummonedCard(card, side);

		if (summonedCard == null)
		{
			Debug.LogError("召喚に失敗しました");
			return false;
		}


		// ← ここで Location を Field に更新
		summonedCard.Location = CardLocation.Field;   // 追加
		handCard.Location = CardLocation.Field;
		summonedCard.OwnerSide = currentTurn;
		targetDeck.RemoveFromHand(handCard);

		// ===== 敵カードなら召喚した瞬間に表に戻す =====
		if (currentTurn == PlayerSide.Enemy)
		{
			FkingCardDisplayAttacher visual =
				summonedCard.GetComponentInChildren<FkingCardDisplayAttacher>();

			if (visual != null)
			{
				visual.ChangeCard(card.cardID); // ★表に戻す
			}
		}


		//summonedCard = targetField.AcceptSummonedCard(card, side);
		if (summonedCard != null)
		{
			StartCoroutine(FkingCardDisplay.Instance.ShowStats(summonedCard));
		}



		// Effect を解決
		EffectManager.Instance.Resolve(
			CardAI.EffectTiming.Summon,
			card,
			new EffectContextAI
			{
				ownerSide = side,
				ownerDeck = targetDeck,
				self = card,
				selfField = summonedCard,
				selfPlayerField = targetField,
				enemyPlayerField = side == SummonSide.Player ? enemyField : playerField,
				selfLife =
			(side == SummonSide.Player)
				? AttackManagerAI.Instance.selfHP
				: AttackManagerAI.Instance.enemyHP,
				enemyLife =
			(side == SummonSide.Player)
				? AttackManagerAI.Instance.enemyHP
				: AttackManagerAI.Instance.selfHP
			}
		);

		//FkingCardDisplay.Instance.ShowStats();

			Debug.Log("[Summon] 成功");

		return true;
	}

	public bool TrySpell(CardAI card, CardDisplayAI handCard, SummonSide side)
	{
		PlayerManaManagerAI targetMana =
			(side == SummonSide.Player) ? playerMana : enemyMana;

		PlayerDeckAI targetDeck =
			(side == SummonSide.Player) ? playerDeck : enemyDeck;

		// フェーズチェック（必要なら）
		if (PhaseManagerAI.Instance.currentPhase != PhaseManagerAI.Phase.Summon)
		{
			Debug.Log("今はメインフェーズではありません！");
			return false;
		}

		// マナチェック
		if (!targetMana.CanPayCost(card))
		{
			Debug.Log("必要マナが足りません！");
			return false;
		}

		// ===== 使用成功 =====
		targetMana.PayCost(card);

		//MotionManager.Instance.spell.StartRedSpell();

		// 手札から墓地へ
		Destroy(handCard.gameObject);
		//handDeck.RemoveFromHand(handCard);


		// 効果を解決
		EffectManager.Instance.Resolve(
			CardAI.EffectTiming.Spell,
			card,
			new EffectContextAI
			{
				ownerSide = side,
				ownerDeck = targetDeck,
				self = card,
				selfField = null,         // 場には置かない
				selfPlayerField = (side == SummonSide.Player) ? playerField : enemyField,
				enemyPlayerField = (side == SummonSide.Player) ? enemyField : playerField,
				selfLife =
			(side == SummonSide.Player)
				? AttackManagerAI.Instance.selfHP
				: AttackManagerAI.Instance.enemyHP,
				enemyLife =
			(side == SummonSide.Player)
				? AttackManagerAI.Instance.enemyHP
				: AttackManagerAI.Instance.selfHP
			}
		);

		//Debug.Log("[Spell] 成功");

		return true;
	}
}

public enum SummonSide
{
	Player,
	Enemy
}
