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

		// ★カードがフィールドにない場合は攻撃不可
		PlayerFieldAI selfPlayerField_ = side == PlayerSide.Self
			? SummonManagerAI.Instance.playerField
			: SummonManagerAI.Instance.enemyField;

		if (!selfPlayerField_.GetAllCards().Contains(card))
		{
			return;
		}

		if (card.IsRested)
		{
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

		attackingCard.RefreshStatsUI();
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
			yield break;
		}

		// 防御側のカード以外は無視（超重要）
		if (blockCard.OwnerSide != defendingSide)
		{
			yield break;
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

		blockCard.RefreshStatsUI();
		
		if (attackingCard == null)
		{
            yield break;
		}

		int attackBP = attackingCard.Attack;
		int blockBP = blockCard.Attack;

		// ============================
		// ★ AllWin（必ず勝利）判定
		// ============================

		bool attackerAllWin =
			attackingCard.CardData.HasEffect(EffectType.AllWin);

		bool blockerAllWin =
			blockCard.CardData.HasEffect(EffectType.AllWin);
 
        // 攻撃モーション
        yield return StartCoroutine(MotionManager.Instance.attack.StartAttack(attackingCard.transform, blockCard.transform, attackerSide, false));
       
		// 両方が持っているなら相打ち
        if (attackerAllWin && blockerAllWin)
		{
			StartCoroutine(MotionManager.Instance._break.StartBreak(attackingCard.transform));
            yield return StartCoroutine(MotionManager.Instance._break.StartBreak(blockCard.transform));
			DestroyCard(attackingCard);
			DestroyCard(blockCard);

			EndAttack();

			yield break;
		}

		// 攻撃側だけAllWin → 防御破壊
		if (attackerAllWin)
		{
            yield return StartCoroutine(MotionManager.Instance._break.StartBreak(blockCard.transform));
			DestroyCard(blockCard);

			// 攻撃側は生き残るのでレスト
			attackingCard.Rest();

			EndAttack();
			yield break;
		}

		// 防御側だけAllWin → 攻撃破壊
		if (blockerAllWin)
		{
            yield return StartCoroutine(MotionManager.Instance._break.StartBreak(attackingCard.transform));
			DestroyCard(attackingCard);

			// 防御側はブロック後レスト
			blockCard.Rest();

			EndAttack();
			yield break;
		}

		bool attackerDestroyed = false;
		bool blockerDestroyed = false;

		if (attackBP > blockBP)
		{
            yield return StartCoroutine(MotionManager.Instance._break.StartBreak(blockCard.transform));
			
			DestroyCard(blockCard);
			
			blockerDestroyed = true;
			// ============================
			// ★道連れ発動チェック
			// ============================
			if (blockCard.sacrificeOnBlockLose)
			{
                yield return StartCoroutine(MotionManager.Instance._break.StartBreak((attackingCard.transform)));
				
				DestroyCard(attackingCard);
				
				attackerDestroyed = true;
			}
		}
		else if (attackBP < blockBP)
		{
            yield return StartCoroutine(MotionManager.Instance._break.StartBreak(attackingCard.transform));
			
			DestroyCard(attackingCard);
			
			attackerDestroyed = true;
		}
		else
		{
			StartCoroutine(MotionManager.Instance._break.StartBreak(blockCard.transform));
            yield return StartCoroutine(MotionManager.Instance._break.StartBreak(attackingCard.transform));
			
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

		// 戦闘後
		attackingCard?.RefreshStatsUI();
		blockCard?.RefreshStatsUI();


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

		// ★破壊時効果で他カードのステータス変わる
		selfPlayerField.RefreshAllStats();
		enemyPlayerField.RefreshAllStats();

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
			return;
		}
	
		if (attackingCard == null)
		{
			return;
		}
	
		// ★ 攻撃カードの攻撃力をダメージとして適用
		int damage = attackingCard.Defense;

		// 攻撃する側を引数に入れる
		PlayerSide attackerSide = (clickedSide == PlayerSide.Self)
			? attackerSide = PlayerSide.Enemy
			: attackerSide = PlayerSide.Self;
        StartCoroutine(MotionManager.Instance.attack.StartAttack(attackingCard.transform, null, attackerSide, true));
	
		GetHp(clickedSide).TakeDamage(damage);

		attackingCard.RefreshStatsUI();

		// ★ 攻撃カードをレスト
		attackingCard.Rest();
	
		EndAttack();

		//HPゼロの時のエンドフラグーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーー
		if (GetHp(clickedSide).hp <= 0)
		{
			bool isWin =
				(clickedSide == PlayerSide.Enemy);

			ResultUI.Instance.ShowResult(isWin);
		}
		//ーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーー
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
			attackingCard.RefreshStatsUI();
		}
			
		attackingCard = null;
		state = AttackState.None;

		PhaseManagerAI.Instance.currentPhase = PhaseManagerAI.Phase.Attack;
	}

	IEnumerator EnemyAutoBlock()
	{
		yield return new WaitForSeconds(1f);

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
			StartCoroutine(BlockWithCard(blocker));
			yield break;
		}

		// -----------------------------
		// ブロックできないならライフで受ける
		// -----------------------------
		TakeLifeDamage(PlayerSide.Enemy);
	}
}
