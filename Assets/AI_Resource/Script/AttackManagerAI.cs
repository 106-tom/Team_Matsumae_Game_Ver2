using System.Collections;   // ★これ追加
using System.Drawing;
using UnityEngine;
using static CardAI;
//using UnityEngine.UIElements;

public enum AttackState
{
	None,
	Attacking,   // 攻撃カード選択中
	Blocking     // 防御側の選択待ち
}

public class AttackManagerAI : MonoBehaviour
{
	public static AttackManagerAI Instance;

	[Header("Attack State")]
	public FieldCardDisplayAI attackingCard;
	public PlayerSide attackerSide;   // 攻撃側
	public PlayerSide defendingSide;  // 防御側
	public AttackState state = AttackState.None;

	[Header("HP")]
	public PlayerHpUIAI selfHP;
	public PlayerHpUIAI enemyHP;

	void Awake()
	{
		Instance = this;
	}

	public bool IsAttacking => attackingCard != null;

	// -----------------------------
	// 攻撃開始
	// -----------------------------
	public void StartAttack(PlayerSide side, FieldCardDisplayAI card)
	{
		if (state != AttackState.None) return;
		// ★レスト中なら攻撃できない
		if (card.IsRested)
		{
			Debug.Log("このカードはレスト中で攻撃できません");
			return;
		}

		attackingCard = card;
		attackerSide = side;
		defendingSide = (side == PlayerSide.Self)
			? PlayerSide.Enemy
			: PlayerSide.Self;

		state = AttackState.Blocking;
		PhaseManagerAI.Instance.currentPhase = PhaseManagerAI.Phase.Block;
		card.SetAttacking(true);

		Debug.Log($"[Attack] 攻撃側={attackerSide} 防御側={defendingSide}");

		// 攻撃側のデッキを取得
		PlayerDeckAI ownerDeck = attackerSide == PlayerSide.Self
			? SummonManagerAI.Instance.playerDeck
			: SummonManagerAI.Instance.enemyDeck;

		// 攻撃側のフィールド
		PlayerFieldAI selfPlayerField = attackerSide == PlayerSide.Self
			? SummonManagerAI.Instance.playerField
			: SummonManagerAI.Instance.enemyField;

		// 相手側のフィールド
		PlayerFieldAI enemyPlayerField = attackerSide == PlayerSide.Self
			? SummonManagerAI.Instance.enemyField
			: SummonManagerAI.Instance.playerField;

		// 攻撃側・防御側のライフ
		PlayerHpUIAI selfLife = attackerSide == PlayerSide.Self
			? AttackManagerAI.Instance.selfHP
			: AttackManagerAI.Instance.enemyHP;

		PlayerHpUIAI enemyLife = attackerSide == PlayerSide.Self
			? AttackManagerAI.Instance.enemyHP
			: AttackManagerAI.Instance.selfHP;

		// デバッグログで確認
		if (ownerDeck == null)
		{
			Debug.LogError("[Debug] 攻撃側デッキが null です！");
		}
		else
		{
			Debug.Log($"[Debug] 攻撃側デッキ取得成功。手札枚数: {ownerDeck.HandCount}");
		}

		if (defendingSide == PlayerSide.Enemy)
		{
			StartCoroutine(EnemyAutoBlock());
		}


		EffectManager.Instance.Resolve(
		CardAI.EffectTiming.Attack,
		attackingCard.CardData,
		new EffectContextAI
		{
			self = attackingCard.CardData,
			//target = blockCard.CardData,
			selfField = attackingCard,
			//targetField = blockCard,
			ownerDeck = ownerDeck,
			selfPlayerField = selfPlayerField,
			enemyPlayerField = enemyPlayerField
		}
		);
	}



	// -----------------------------
	// 防御カードでブロック
	// -----------------------------
	public IEnumerator BlockWithCard(FieldCardDisplayAI blockCard)
	{
		// 状態チェック
		if (state != AttackState.Blocking)
			yield break;

		// ★レスト中のカードはブロックできない
		if (blockCard.IsRested)
		{
			Debug.Log("このカードはレスト中なのでブロックできません");
<<<<<<< HEAD
			yield break;
=======
            yield break;
>>>>>>> origin/tom
		}


		// 防御側のカード以外は無視（超重要）
		if (blockCard.OwnerSide != defendingSide)
		{
			Debug.Log("[Block] 防御側以外のカードが押された");
<<<<<<< HEAD
			yield break;
=======
            yield break;
>>>>>>> origin/tom
		}

		EffectManager.Instance.Resolve(
		CardAI.EffectTiming.Block,
		blockCard.CardData,
		new EffectContextAI
		{
			self = blockCard.CardData,
			target = attackingCard.CardData,
			selfField = blockCard,
			targetField = attackingCard
		}
		);
		
		if (attackingCard == null)
		{
			Debug.LogError("[Block] attackingCard が存在しません");
<<<<<<< HEAD
			yield break;
=======
            yield break;
>>>>>>> origin/tom
		}


		int attackBP = attackingCard.Attack;
		int blockBP = blockCard.Attack;

		Debug.Log(
			$"[Battle] 攻撃={attackingCard.CardName} ATK={attackBP} / " +
			$"防御={blockCard.CardName} ATK={blockBP}"
		);

		// ============================
		// ★ AllWin（必ず勝利）判定
		// ============================

		bool attackerAllWin =
			attackingCard.CardData.HasEffect(EffectType.AllWin);

		bool blockerAllWin =
			blockCard.CardData.HasEffect(EffectType.AllWin);

<<<<<<< HEAD
		// 攻撃モーション
		yield return StartCoroutine(MotionManager.Instance.attack.StartAttack(attackingCard.transform, blockCard.transform, attackerSide));
		// ブロックモーション
		yield return StartCoroutine(MotionManager.Instance.block.StartBlock(attackingCard.transform));

=======
        // 攻撃モーション
        yield return StartCoroutine(MotionManager.Instance.attack.StartAttack(attackingCard.transform, blockCard.transform, attackerSide, false));
        // ブロックモーション
		//yield return StartCoroutine(MotionManager.Instance.block.StartBlock(attackingCard.transform));
        
>>>>>>> origin/tom
		// 両方が持っているなら相打ち
        if (attackerAllWin && blockerAllWin)
		{
			Debug.Log("[Battle] 両方 AllWin → 相打ち");
			MotionManager.Instance._break.StartBreak(attackingCard.transform);
			MotionManager.Instance._break.StartBreak(blockCard.transform);
			DestroyCard(attackingCard);
			DestroyCard(blockCard);

			EndAttack();
<<<<<<< HEAD
			yield break;
=======
            yield break;
>>>>>>> origin/tom
		}

		// 攻撃側だけAllWin → 防御破壊
		if (attackerAllWin)
		{
			Debug.Log("[Battle] 攻撃側 AllWin → 防御破壊");

			MotionManager.Instance._break.StartBreak(blockCard.transform);
			DestroyCard(blockCard);

			// 攻撃側は生き残るのでレスト
			attackingCard.Rest();

			EndAttack();
<<<<<<< HEAD
			yield break;
=======
            yield break;
>>>>>>> origin/tom
		}

		// 防御側だけAllWin → 攻撃破壊
		if (blockerAllWin)
		{
			Debug.Log("[Battle] 防御側 AllWin → 攻撃破壊");

			MotionManager.Instance._break.StartBreak(attackingCard.transform);
			DestroyCard(attackingCard);

			// 防御側はブロック後レスト
			blockCard.Rest();

			EndAttack();
<<<<<<< HEAD
			yield break;
=======
            yield break;
>>>>>>> origin/tom
		}

		bool attackerDestroyed = false;
		bool blockerDestroyed = false;

		if (attackBP > blockBP)
		{
			Debug.Log("[Battle] 攻撃側勝利（防御破壊）");
			MotionManager.Instance._break.StartBreak(blockCard.transform);
			DestroyCard(blockCard);
			blockerDestroyed = true;
			// ============================
			// ★道連れ発動チェック
			// ============================
			if (blockCard.sacrificeOnBlockLose)
			{
				Debug.Log("[Effect] 道連れ発動！攻撃側も破壊");

				MotionManager.Instance._break.StartBreak(attackingCard.transform);
				DestroyCard(attackingCard);
				attackerDestroyed = true;
			}
		}
		else if (attackBP < blockBP)
		{
			Debug.Log("[Battle] 防御側勝利（攻撃破壊）");
			MotionManager.Instance._break.StartBreak(attackingCard.transform);
			DestroyCard(attackingCard);
			attackerDestroyed = true;
		}
		else
		{
			Debug.Log("[Battle] 相打ち（両方破壊）");
			MotionManager.Instance._break.StartBreak(blockCard.transform);
			MotionManager.Instance._break.StartBreak(attackingCard.transform);
			DestroyCard(blockCard);
			DestroyCard(attackingCard);
			blockerDestroyed = true;
			attackerDestroyed = true;
		}

		// ============================
		// ★ 生き残ったカードはレスト
		// ============================

		if (!attackerDestroyed && attackingCard != null)
		{
			attackingCard.Rest();
		}

		if (!blockerDestroyed && blockCard != null)
		{
			if (blockCard.noRestAfterBlock)
			{
				Debug.Log("[Effect] ブロック後レスト無効！");
			}
			else
			{
				blockCard.Rest();
			}
			blockCard.noRestAfterBlock = false;
		}

		if (blockCard != null)
		{
			blockCard.sacrificeOnBlockLose = false;
		}

		EndAttack();
	}

	public void DestroyCard(FieldCardDisplayAI card)
	{
		if (card == null) return;

		// --- OwnerSide からプレイヤーのフィールド・デッキを取得 ---
		PlayerFieldAI selfPlayerField =
			card.OwnerSide == PlayerSide.Self ? SummonManagerAI.Instance.playerField : SummonManagerAI.Instance.enemyField;

		PlayerFieldAI enemyPlayerField =
			card.OwnerSide == PlayerSide.Self ? SummonManagerAI.Instance.enemyField : SummonManagerAI.Instance.playerField;

		PlayerDeckAI ownerDeck =
			card.OwnerSide == PlayerSide.Self ? SummonManagerAI.Instance.playerDeck : SummonManagerAI.Instance.enemyDeck;

		if (card.CardData.cardID == "37") // ←対象カードID
		{
			if (!card.usedDestroyCancel)
			{
				Debug.Log("[Effect] 破壊を1回無効にした！");

				card.usedDestroyCancel = true; // ★一回使った
				return; // ★Destroyしない
			}
		}

		// --- 破壊時効果を解決 ---
		EffectManager.Instance.Resolve(
			CardAI.EffectTiming.Destroyed,
			card.CardData,
			new EffectContextAI
			{
				self = card.CardData,
				selfField = card,
				selfPlayerField = selfPlayerField,
				enemyPlayerField = enemyPlayerField,
				ownerSide = ToSummonSide(card.OwnerSide),
				ownerDeck = ownerDeck,
				selfLife = selfHP,
				enemyLife = enemyHP
			}
		);

		// カードオブジェクトを破壊
		Destroy(card.gameObject);
	}

	private SummonSide ToSummonSide(PlayerSide side)
	{
		return side == PlayerSide.Self ? SummonSide.Player : SummonSide.Enemy;
	}

	// -----------------------------
	// プレイヤーがダメージを受ける
	// -----------------------------
	public void TakeLifeDamage(PlayerSide clickedSide)
	{
		if (state != AttackState.Blocking) return;
	
		// 防御側以外は無視
		if (clickedSide != defendingSide)
		{
			Debug.Log("[Life] 防御側ではないHPが押された");
			return;
		}
	
		if (attackingCard == null)
		{
			Debug.LogError("[Life] 攻撃カードが存在しません");
			return;
		}
	
		// ★ 攻撃カードの攻撃力をダメージとして適用
		int damage = attackingCard.Defense;

		// 攻撃する側を引数に入れる
		PlayerSide attackerSide = (clickedSide == PlayerSide.Self)
			? attackerSide = PlayerSide.Enemy
			: attackerSide = PlayerSide.Self;

        StartCoroutine(MotionManager.Instance.attack.StartAttack(attackingCard.transform, null, attackerSide, true));
	
		Debug.Log($"[Life] {clickedSide} が {damage} ダメージを受ける");
	
		GetHp(clickedSide).TakeDamage(damage);
	
			// ★ 攻撃カードをレスト
		attackingCard.Rest();
	
	
		EndAttack();
	}


	// -----------------------------
	// HP取得
	// -----------------------------
	private PlayerHpUIAI GetHp(PlayerSide side)
	{
		return side == PlayerSide.Self ? selfHP : enemyHP;
	}





	// -----------------------------
	// 攻撃終了
	// -----------------------------
	private void EndAttack()
	{
		if (attackingCard != null)
		{
			attackingCard.ResetTempAttack();
			attackingCard.SetAttacking(false);
		}
			
		attackingCard = null;
		state = AttackState.None;

		PhaseManagerAI.Instance.currentPhase = PhaseManagerAI.Phase.Attack;

		Debug.Log("[Attack] 攻撃終了");
	}

	IEnumerator EnemyAutoBlock()
	{
		yield return new WaitForSeconds(1f);

		Debug.Log("[AI Block] 敵がブロック判断を開始");

		// 敵フィールドのカード取得
		var enemyCards = SummonManagerAI.Instance.enemyField.GetAllCards();

		FieldCardDisplayAI blocker = null;

		// ブロックできるカードを探す（レスト除外）
		foreach (var card in enemyCards)
		{
			if (!card.IsRested)
			{
				blocker = card;
				break;
			}
		}

		// -----------------------------
		// ブロックできるならカードでブロック
		// -----------------------------
		if (blocker != null)
		{
			Debug.Log($"[AI Block] ブロックします → {blocker.CardName}");
			StartCoroutine(BlockWithCard(blocker));
			yield break;
		}

		// -----------------------------
		// ブロックできないならライフで受ける
		// -----------------------------
		Debug.Log("[AI Block] ブロックできないのでライフで受けます");
		TakeLifeDamage(PlayerSide.Enemy);
	}

}
