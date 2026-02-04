using Photon.Pun.Demo.PunBasics;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static CardAI;
//using static UnityEditor.Timeline.TimelinePlaybackControls;

public class EffectManager : MonoBehaviour
{
	public static EffectManager Instance;

	void Awake()
	{
		Instance = this;
	}

	// -----------------------------
	// 選択モード用の状態
	// -----------------------------
	private bool isSelectingTarget = false;
	private CardAI effectCard;
	private EffectContextAI effectContext;
	private List<FieldCardDisplayAI> selectableTargets;
	private int remainingDestroyCount = 0;
	private int remainingRestCount = 0;
	private bool pendingDefenseZero = false;
	private bool pendingDestroyThenDraw = false;
	private bool pendingDestroyThenRest = false;
	private bool pendingRest = false;
	private bool pendingDebuffBP = false;
	private int debuffValue = 0;
	private int remainingDebuffCount = 0;





	public bool IsSelectingTarget => isSelectingTarget;


	public void Resolve(
		CardAI.EffectTiming timing,
		CardAI card,
		EffectContextAI context
	)
	{
		if (card.effects == null || card.effects.Count == 0)
			return;

		//Debug.Log($"[Effect] {card.cardName} の効果チェック ({timing})");

		foreach (var effect in card.effects)
		{
			// タイミングが一致しないならスキップ
			if (effect.effectTimings != timing)
				continue;

			//Debug.Log($"[Effect] 発動 → {effect.effectType}");

			switch (effect.effectType)
			{
				case CardAI.EffectType.Draw:
					ResolveDraw(effect, card, context);
					break;

				case CardAI.EffectType.Buff:
					ResolveBuff(effect, card, context);
					break;

				case CardAI.EffectType.DestroySingle:
					ResolveDestroy(effect, card, context);
					break;

				case EffectType.DestroyAll:
					ResolveDestroyAll(effect, card, context);
					break;

				case EffectType.DrawThenDestroy:
					ResolveDrawThenDestroy(effect, card, context);
					break;

				case EffectType.DrawThenConditional:
					ResolveDrawThenConditionalDraw(effect, card, context);
					break;

				case EffectType.DestroyPerHandCount:
					ResolveDestroyPerHandCount(effect, card, context);
					break;

				case EffectType.DestroyByHandCountBP:
					ResolveDestroyByHandCountBP(effect, card, context);
					break;

				case EffectType.SetBPByHandCount:
					ResolveSetBPByHandCount(effect, card, context);
					break;
				case EffectType.DamageEnemyLifeByHandCount:
					ResolveDamageEnemyLifeByHandCount(card, context);
					break;
				case EffectType.DestroyPerHandCountSelect:
					ResolveDestroyPerHandCountSelect(effect, card, context);
					break;
				case EffectType.DrawThenSetEnemyDefenseZero:
					ResolveDrawThenSetEnemyDefenseZero(effect, card, context);
					break;
				case EffectType.DestroyThenDraw:
					ResolveDestroyThenDraw(effect, context);
					break;
				case EffectType.Rest:
					ResolveRest(effect, card, context);
					break;
				case EffectType.DestroyThenRest:
					ResolveDestroyThenRest(effect, context);
					break;
				case EffectType.DrawPerRestEnemy:
					ResolveDrawPerRestEnemy(card, context);
					break;
				case EffectType.DestroyAllRestEnemy:
					ResolveDestroyAllRestEnemy(card, context);
					break;
				case EffectType.BuffSelfPerRestEnemy:
					ResolveBuffSelfPerRestEnemy(card, context);
					break;
				case EffectType.RestAllThenGainAP:
					ResolveRestAllThenGainAP(card, context);
					break;
				case EffectType.Heal:
					ResolveHeal(effect, card, context);
					break;
				case EffectType.DebuffBP:
					ResolveDebuffBP(effect, card, context);
					break;
				case EffectType.SacrificeOnBlockLose:
					ResolveSacrificeOnBlockLose(effect, context);
					break;
				case EffectType.NoRestAfterBlock:
					ResolveNoRestAfterBlock(effect, context);
					break;
				case EffectType.FieldCountBuff:
					ResolveBuffByOwnField(effect, card, context);
					break;
				case EffectType.DestroyAllHeal:
					ResolveDestroyAllAndHeal(effect, card, context);
					break;
				case EffectType.DebuffAP:
					ResolveSpellSetBPZero(card, context);
					break;
			}
		}	
	}

	void ResolveDraw(CardEffect effect, CardAI card, EffectContextAI context)
	{
		for (int i = 0; i < effect.effectValue; i++)
		{
			context.ownerDeck.DrawCard(
				context.ownerSide == SummonSide.Player
					? PlayerSide.Self
					: PlayerSide.Enemy
			);
		}

		Debug.Log($"[Effect] {card.cardName}：ドロー {effect.effectValue}枚");
	}


	void ResolveBuff(CardEffect effect, CardAI card, EffectContextAI context)
	{
		switch (effect.target)
		{
			case EffectTarget.SelfMonster:
			case EffectTarget.AllSelfMonster:
				ApplyBuffToSelf(effect, context);
				break;

			case EffectTarget.EnemyMonster:
			case EffectTarget.AllEnemyMonsters:
				ApplyBuffToTarget(effect, context);
				break;
		}
	}

	// -----------------------------
	// 召喚時破壊（クリック選択式）
	// -----------------------------
	void ResolveDestroy(CardEffect effect, CardAI card, EffectContextAI context)
	{
		PlayerFieldAI targetField = null;

		// -------------------------
		// 破壊対象フィールドを判定
		// -------------------------
		switch (effect.target)
		{
			case EffectTarget.SelfMonster:
				targetField = context.selfPlayerField;
				break;
			case EffectTarget.EnemyMonster:
				targetField = context.enemyPlayerField;
				break;
			default:
				Debug.LogError("[Effect] SelfMonster または EnemyMonster を指定してください");
				return;
		}

		if (targetField == null)
		{
			Debug.Log("[Effect] 破壊対象フィールドが存在しません");
			return;
		}

		// -------------------------
		// 破壊対象カードの取得
		// -------------------------
		selectableTargets = targetField.GetAllCards()
			.Where(c => c.Attack <= effect.effectValue)
			.ToList();

		if (selectableTargets.Count == 0)
		{
			Debug.Log("[Effect] 破壊対象なし");
			return;
		}

		// ハイライト
		foreach (var c in selectableTargets)
			c.Highlight(true);

		// -------------------------
		// 選択フラグを設定
		// -------------------------
		remainingDestroyCount = 1; // 1体選択
		isSelectingTarget = true;
		effectCard = card;
		effectContext = context;

		Debug.Log("[Effect] 破壊対象をクリックしてください");
	}


	void ResolveDestroyAll(CardEffect effect, CardAI card, EffectContextAI context)
	{
		// 破壊対象フィールドを判定
		var fieldsToDestroy = new List<PlayerFieldAI>();

		switch (effect.target)
		{
			case EffectTarget.SelfMonster:
			case EffectTarget.AllSelfMonster:
				if (context.selfPlayerField != null)
					fieldsToDestroy.Add(context.selfPlayerField);
				break;

			case EffectTarget.EnemyMonster:
			case EffectTarget.AllEnemyMonsters:
				if (context.enemyPlayerField != null)
					fieldsToDestroy.Add(context.enemyPlayerField);
				break;

			case EffectTarget.None: // 両方破壊
				if (context.selfPlayerField != null)
					fieldsToDestroy.Add(context.selfPlayerField);
				if (context.enemyPlayerField != null)
					fieldsToDestroy.Add(context.enemyPlayerField);
				break;
		}

		// 各フィールドのカードを破壊
		foreach (var field in fieldsToDestroy)
		{
			var targets = field.GetAllCards()
				.Where(c => c.Attack <= effect.effectValue) // 効果値で条件付き破壊
				.ToList();

			if (targets.Count == 0)
			{
				Debug.Log("[Effect] 破壊対象なし");
				continue;
			}

			foreach (var c in targets)
			{
				field.RemoveFieldCard(c);
				AttackManagerAI.Instance.DestroyCard(c);
				Debug.Log($"[Effect] {c.CardName} を破壊");
			}
		}
	}



	// プレイヤーがクリックしたら破壊
	public void OnTargetCardClicked(FieldCardDisplayAI clicked)
	{
		if (!isSelectingTarget) return;
		if (!selectableTargets.Contains(clicked)) return;

		// ============================
		// ★ 2回目：レスト処理
		// ============================
		if (pendingRest)
		{
			clicked.Rest();
			selectableTargets.Remove(clicked);

			remainingRestCount--;

			Debug.Log($"[Effect] {clicked.CardName} をレストした 残り{remainingRestCount}体");

			// ハイライト解除
			clicked.Highlight(false);

			// ★敵がいなくなったら終了
			if (selectableTargets.Count == 0)
			{
				Debug.Log("[Effect] 対象がいないので終了");
				pendingRest = false;
				EndDestroySelection();
				return;
			}

			// ★まだ回数が残っているなら続行
			if (remainingRestCount > 0)
			{
				Debug.Log("[Effect] 続けてレスト対象を選んでください");
				return;
			}

			// ★全部終わったら終了
			Debug.Log("[Effect] レスト選択完了");
			pendingRest = false;
			EndDestroySelection();
			return;
		}

		// ============================
		// ★ BPデバフ処理（複数対応）
		// ============================
		if (pendingDebuffBP)
		{
			clicked.AddPermanentAttack(debuffValue, 0);

			Debug.Log($"[Effect] {clicked.CardName} のBPを {debuffValue} 下げた");

			// 選択済みを除外
			selectableTargets.Remove(clicked);
			clicked.Highlight(false);

			remainingDebuffCount--;

			// ★まだ選ぶなら続行
			if (remainingDebuffCount > 0 && selectableTargets.Count > 0)
			{
				Debug.Log($"[Effect] 残り {remainingDebuffCount}体 選んでください");
				return;
			}

			// ★終了
			pendingDebuffBP = false;
			EndDestroySelection();
			return;
		}



		// ============================
		// ★ Defense0 効果
		// ============================
		if (pendingDefenseZero)
		{
			clicked.SetDefense(0);

			Debug.Log($"[Effect] {clicked.CardName} の打撃力を0にした");

			pendingDefenseZero = false;
			EndDestroySelection();
			return;
		}

		// ============================
		// ★ 1回目：破壊処理
		// ============================
		effectContext.enemyPlayerField.RemoveFieldCard(clicked);
		selectableTargets.Remove(clicked);

		Debug.Log($"[Effect] {clicked.CardName} を破壊した");

		// ============================
		// ★ DestroyThenRest の場合
		// ============================
		if (pendingDestroyThenRest)
		{
			pendingDestroyThenRest = false;

			Debug.Log("[Effect] 次にレストする敵モンスターを選んでください");

			// ★残った敵を再取得
			selectableTargets =
				effectContext.enemyPlayerField.GetAllCards().ToList();

			// 敵が残っていないなら終了
			if (selectableTargets.Count == 0)
			{
				Debug.Log("[Effect] 敵が残っていないのでレストできない");
				EndDestroySelection();
				return;
			}

			// ハイライト更新
			foreach (var c in selectableTargets)
				c.Highlight(true);

			// ★ここで終了しない！！
			pendingRest = true;
			return;
		}

		// ============================
		// ★ 通常破壊終了
		// ============================
		EndDestroySelection();

		// ============================
		// ★ DestroyThenDraw の場合
		// ============================
		if (pendingDestroyThenDraw)
		{
			pendingDestroyThenDraw = false;

			Debug.Log("[Effect] 破壊完了 → 1枚ドロー");

			effectContext.ownerDeck.DrawCard(
				effectContext.ownerSide == SummonSide.Player
					? PlayerSide.Self
					: PlayerSide.Enemy
			);
		}
	}




	void EndDestroySelection()
	{
		if (selectableTargets != null)
		{
			foreach (var c in selectableTargets)
			{
				c.Highlight(false);
				c.SetColor(Color.white);
			}

			selectableTargets.Clear();
		}

		isSelectingTarget = false;

		pendingRest = false;
		pendingDestroyThenRest = false;
		pendingDestroyThenDraw = false;
		pendingDefenseZero = false;

		Debug.Log("[Effect] 選択終了");
	}




	void ApplyBuffToSelf(CardEffect effect, EffectContextAI context)
	{
		// ★ 全体バフ
		if (effect.target == EffectTarget.AllSelfMonster)
		{
			if (context.selfPlayerField == null)
			{
				Debug.LogError("selfPlayerField が null");
				return;
			}

			foreach (var fieldCard in context.selfPlayerField.GetAllCards())
			{
				ApplyBuffToFieldCard(fieldCard, effect);
			}
			return;
		}

		// ★ 単体バフ
		if (context.selfField == null)
		{
			Debug.LogError("selfField が null");
			return;
		}

		ApplyBuffToFieldCard(context.selfField, effect);
	}


	void ApplyBuffToTarget(CardEffect effect, EffectContextAI context)
	{
		// ★ 全体バフ（敵全体）
		if (effect.target == EffectTarget.AllEnemyMonsters)
		{
			if (context.enemyPlayerField == null)
			{
				Debug.LogError("enemyPlayerField が null");
				return;
			}

			foreach (var fieldCard in context.enemyPlayerField.GetAllCards())
			{
				ApplyBuffToFieldCard(fieldCard, effect);
			}
			return;
		}

		// ★ 単体バフ（敵1体）
		if (context.targetField == null)
		{
			Debug.LogError("targetField が null");
			return;
		}

		ApplyBuffToFieldCard(context.targetField, effect);
	}



	void ApplyBuffToFieldCard(FieldCardDisplayAI fieldCard, CardEffect effect)
	{
		if (fieldCard == null) return;

		switch (effect.buffDuration)
		{
			case BuffDuration.Temporary:
				fieldCard.AddTempAttack(effect.effectValue, effect.extraValue);
				break;

			case BuffDuration.Permanent:
				fieldCard.AddPermanentAttack(effect.effectValue, effect.extraValue);
				break;
		}

		Debug.Log($"[Effect] BP+{effect.effectValue}");
	}

	public void ResolveTurnStartEffects(SummonSide side)
	{
		// 自分・相手のフィールドを決定
		PlayerFieldAI selfField =
			side == SummonSide.Player
			? SummonManagerAI.Instance.playerField
			: SummonManagerAI.Instance.enemyField;

		PlayerFieldAI enemyField =
			side == SummonSide.Player
			? SummonManagerAI.Instance.enemyField
			: SummonManagerAI.Instance.playerField;

		foreach (var fieldCard in selfField.GetAllCards())
		{
			Resolve(
				CardAI.EffectTiming.TurnStart,
				fieldCard.CardData,
				new EffectContextAI
				{
					ownerSide = side,              // ← SummonSideで統一
					self = fieldCard.CardData,
					selfField = fieldCard,
					selfPlayerField = selfField,
					enemyPlayerField = enemyField
				}
			);
		}
	}

	void ResolveDrawThenDestroy(CardEffect effect, CardAI card, EffectContextAI context)
	{
		PlayerSide playerSide =
			context.ownerSide == SummonSide.Player
				? PlayerSide.Self
				: PlayerSide.Enemy;

		List<CardDisplayAI> drawnDisplays = new();

		for (int i = 0; i < effect.effectValue; i++)
		{
			var display = context.ownerDeck.DrawCardAndGetDisplay(playerSide);
			if (display != null)
				drawnDisplays.Add(display);
		}

		foreach (var display in drawnDisplays)
		{
			if (display.cardData.colorType != "Blue")
				context.ownerDeck.RemoveFromHand(display);
		}

		Debug.Log("[Effect] 青以外を破壊");
	}

	void ResolveDrawThenConditionalDraw(CardEffect effect, CardAI card, EffectContextAI context)
	{
		PlayerSide playerSide =
			context.ownerSide == SummonSide.Player
				? PlayerSide.Self
				: PlayerSide.Enemy;

		var firstDraw = context.ownerDeck.DrawCardAndGetDisplay(playerSide);
		if (firstDraw == null) return;

		bool shouldDraw = false;

		switch (effect.conditionalDrawType)
		{
			case ConditionalDrawType.OddCost:
				shouldDraw = (firstDraw.cardData.TotalCost % 2 == 1);
				break;

			case ConditionalDrawType.BlueColor:
				shouldDraw = (firstDraw.cardData.colorType == "Blue");
				break;
		}

		if (shouldDraw)
			context.ownerDeck.DrawCardAndGetDisplay(playerSide);

	}

	void ResolveDestroyPerHandCount(CardEffect effect, CardAI card, EffectContextAI context)
	{
		int handCount = context.ownerDeck.HandCount;

		remainingDestroyCount = handCount / effect.effectValue;

		if (remainingDestroyCount <= 0)
		{
			Debug.Log("[Effect] 破壊なし");
			return;
		}

		selectableTargets = context.enemyPlayerField.GetAllCards().ToList();

		foreach (var c in selectableTargets)
			c.Highlight(true);

		isSelectingTarget = true;
		effectContext = context;

		Debug.Log($"[Effect] {remainingDestroyCount}体破壊を選んでください");
	}

	void ResolveDestroyByHandCountBP(CardEffect effect, CardAI card, EffectContextAI context)
	{
		int handCount = context.ownerDeck.HandCount;

		int bpLimit = handCount * effect.effectValue;

		selectableTargets = context.enemyPlayerField.GetAllCards()
			.Where(c => c.Attack <= bpLimit)
			.ToList();

		if (selectableTargets.Count == 0)
		{
			Debug.Log("[Effect] 対象なし");
			return;
		}

		remainingDestroyCount = 1;

		foreach (var c in selectableTargets)
			c.Highlight(true);

		isSelectingTarget = true;
		effectContext = context;

		Debug.Log($"[Effect] BP{bpLimit}以下を破壊してください");
	}

	void ResolveSetBPByHandCount(CardEffect effect, CardAI card, EffectContextAI context)
	{
		if (context.ownerDeck == null) return;
		if (context.selfField == null) return;

		// ① 手札枚数取得
		int handCount = context.ownerDeck.HandCount;

		// ② BP倍率（effectValue）
		int attackMultiplier = effect.effectValue;

		// ③ 打撃力倍率（extraValue）
		int defenseMultiplier = effect.extraValue;

		// ④ 新しいBPと打撃力を計算
		int newAttack = handCount * attackMultiplier;
		int newDefense = handCount * defenseMultiplier;

		// ⑤ BPを上書き
		if (attackMultiplier > 0)
		{
			context.selfField.SetAttack(newAttack);
			Debug.Log($"[Effect] {card.cardName}：BP = 手札{handCount}枚 × {attackMultiplier} → {newAttack}");
		}

		// ⑥ 打撃力を上書き
		if (defenseMultiplier > 0)
		{
			context.selfField.SetDefense(newDefense);
			Debug.Log($"[Effect] {card.cardName}：打撃力 = 手札{handCount}枚 × {defenseMultiplier} → {newDefense}");
		}
	}


	void ResolveDamageEnemyLifeByHandCount(CardAI card, EffectContextAI context)
	{
		if (context.ownerDeck == null) return;
		if (context.enemyLife == null) return;

		// ① 手札枚数取得
		int handCount = context.ownerDeck.HandCount;

		// ② 相手ライフにダメージ
		context.enemyLife.TakeDamage(handCount);

		Debug.Log($"[Effect] 手札{handCount}枚 → 相手ライフに{handCount}ダメージ！");
	}

	void ResolveDestroyPerHandCountSelect(CardEffect effect, CardAI card, EffectContextAI context)
	{
		if (context.ownerDeck == null || context.enemyPlayerField == null)
			return;

		// ① 手札枚数を取得
		int handCount = context.ownerDeck.hand.Count;

		if (handCount <= 0)
		{
			Debug.Log("[Effect] 手札が0なので破壊できない");
			return;
		}

		// ② 敵モンスター取得
		selectableTargets = context.enemyPlayerField.GetAllCards();

		if (selectableTargets.Count == 0)
		{
			Debug.Log("[Effect] 敵モンスターがいない");
			return;
		}

		// ③ 破壊回数を設定
		remainingDestroyCount = handCount;

		Debug.Log($"[Effect] 手札{handCount}枚 → {remainingDestroyCount}体まで破壊できる");

		// ④ ハイライト
		foreach (var c in selectableTargets)
		{
			c.Highlight(true);
		}

		// ⑤ 選択モード開始
		isSelectingTarget = true;
		effectCard = card;
		effectContext = context;

		Debug.Log("[Effect] 破壊対象を選択してください");
	}

	void ResolveDrawThenSetEnemyDefenseZero(CardEffect effect, CardAI card, EffectContextAI context)
	{
		if (context.ownerDeck == null) return;
		if (context.enemyPlayerField == null) return;

		// SummonSide → PlayerSide変換
		PlayerSide playerSide =
			context.ownerSide == SummonSide.Player
				? PlayerSide.Self
				: PlayerSide.Enemy;

		// ① 1枚ドロー
		CardDisplayAI drawn = context.ownerDeck.DrawCardAndGetDisplay(playerSide);

		if (drawn == null)
		{
			Debug.Log("[Effect] ドローできなかった");
			return;
		}

		Debug.Log($"[Effect] 1枚ドロー → {drawn.cardData.cardName}");

		// ② 青か判定
		if (drawn.cardData.colorType != "Blue")
		{
			Debug.Log("[Effect] 青ではない → 効果終了");
			return;
		}

		Debug.Log("[Effect] 青カードだった → 相手モンスター1体の打撃力を0にする");

		// ③ 相手モンスター選択モードへ
		selectableTargets = context.enemyPlayerField.GetAllCards().ToList();

		if (selectableTargets.Count == 0)
		{
			Debug.Log("[Effect] 相手フィールドにモンスターがいない");
			return;
		}

		// ハイライト
		foreach (var c in selectableTargets)
			c.Highlight(true);

		// 状態セット
		isSelectingTarget = true;
		effectCard = card;
		effectContext = context;

		// ★特殊モードとして記録
		pendingDefenseZero = true;

		Debug.Log("[Effect] 対象をクリックしてください（打撃力0）");
	}

	void ResolveDestroyThenDraw(CardEffect effect, EffectContextAI context)
	{
		if (context.enemyPlayerField == null) return;
		if (context.ownerDeck == null) return;

		// ① 相手モンスターを選択させる
		selectableTargets = context.enemyPlayerField.GetAllCards().ToList();

		if (selectableTargets.Count == 0)
		{
			Debug.Log("[Effect] 相手モンスターがいない");
			return;
		}

		foreach (var c in selectableTargets)
			c.Highlight(true);

		isSelectingTarget = true;
		effectContext = context;

		// ★④ 破壊回数は1回
		remainingDestroyCount = 1;

		// ★⑤ 破壊後ドロー予約フラグON
		pendingDestroyThenDraw = true;

		Debug.Log("[Effect] 相手モンスター1体を破壊 → その後1枚ドローします");
		Debug.Log("[Effect] 破壊対象を選んでください");
	}

	void ResolveRest(CardEffect effect, CardAI card, EffectContextAI context)
	{
		switch (effect.target)
		{
			// -----------------------
			// 敵1体をレスト（選択式）
			// -----------------------
			case EffectTarget.EnemyMonster:
				StartRestSelection(context,effect.effectValue);
				break;

			// -----------------------
			// 敵全体をレスト
			// -----------------------
			case EffectTarget.AllEnemyMonsters:
				RestAll(context.enemyPlayerField);
				break;

			// -----------------------
			// 自分1体をレスト
			// -----------------------
			case EffectTarget.SelfMonster:
				context.selfField.Rest();   // ★ここ修正
				break;

			// -----------------------
			// 自分全体をレスト
			// -----------------------
			case EffectTarget.AllSelfMonster:
				RestAll(context.selfPlayerField);
				break;
		}
	}

	void StartRestSelection(EffectContextAI context,int restCount)
	{
		selectableTargets = context.enemyPlayerField.GetAllCards().ToList();

		if (selectableTargets.Count == 0)
		{
			Debug.Log("[Effect] レスト対象がいない");
			return;
		}

		foreach (var c in selectableTargets)
			c.Highlight(true);

		isSelectingTarget = true;
		effectContext = context;

		// ★残り回数をセット
		remainingRestCount = restCount;

		pendingRest = true;

		Debug.Log($"[Effect] レストする敵モンスターを {restCount}体選んでください");
	}

	void RestAll(PlayerFieldAI field)
	{
		if (field == null) return;

		foreach (var card in field.GetAllCards())
		{
			card.Rest();   // ★ここ修正
		}

		Debug.Log("[Effect] 全体をレスト状態にした");
	}

	void ResolveDestroyThenRest(CardEffect effect, EffectContextAI context)
	{
		if (context.enemyPlayerField == null) return;

		selectableTargets = context.enemyPlayerField.GetAllCards().ToList();

		if (selectableTargets.Count == 0)
		{
			Debug.Log("[Effect] 相手モンスターがいない");
			return;
		}

		foreach (var c in selectableTargets)
			c.Highlight(true);

		// まず破壊を選ばせる
		isSelectingTarget = true;
		effectContext = context;

		remainingDestroyCount = 1;

		// ★破壊後にレストへ移行する予約
		pendingDestroyThenRest = true;

		Debug.Log("[Effect] まず相手モンスター1体を破壊してください");
	}

	void ResolveDrawPerRestEnemy(CardAI card, EffectContextAI context)
	{
		if (context.enemyPlayerField == null) return;
		if (context.ownerDeck == null) return;

		// ① 相手のレストモンスターを数える
		int restCount = context.enemyPlayerField.GetAllCards()
			.Count(c => c.IsRested);

		if (restCount == 0)
		{
			Debug.Log("[Effect] 相手にレストモンスターがいない → ドローなし");
			return;
		}

		Debug.Log($"[Effect] 相手のレストモンスターは {restCount}体 → {restCount}枚ドロー");

		// ② 自分側判定
		PlayerSide playerSide =
			context.ownerSide == SummonSide.Player
				? PlayerSide.Self
				: PlayerSide.Enemy;

		// ③ restCount 枚ドロー
		for (int i = 0; i < restCount; i++)
		{
			context.ownerDeck.DrawCard(playerSide);
		}
	}

	void ResolveDestroyAllRestEnemy(CardAI card, EffectContextAI context)
	{
		if (context.enemyPlayerField == null) return;

		// ① 相手のレスト状態モンスターを取得
		var restEnemies = context.enemyPlayerField.GetAllCards()
			.Where(c => c.IsRested)
			.ToList();

		if (restEnemies.Count == 0)
		{
			Debug.Log("[Effect] 相手にレストモンスターがいない → 破壊なし");
			return;
		}

		Debug.Log($"[Effect] レスト状態の敵モンスター {restEnemies.Count}体を全て破壊する");

		// ② 全破壊
		foreach (var enemy in restEnemies)
		{
			context.enemyPlayerField.RemoveFieldCard(enemy);

			Debug.Log($"[Effect] {enemy.CardName} を破壊した");
		}
	}
	void ResolveBuffSelfPerRestEnemy(CardAI card, EffectContextAI context)
	{
		if (context.enemyPlayerField == null) return;
		if (context.selfField == null) return;

		// ① 相手のレストモンスター数を数える
		int restCount = context.enemyPlayerField.GetAllCards()
			.Count(c => c.IsRested);

		if (restCount == 0)
		{
			Debug.Log("[Effect] 相手にレストモンスターがいない → BP上昇なし");
			return;
		}

		// ② 上昇量を計算（1体につき1000）
		int buffAmount = restCount * 1000;

		Debug.Log($"[Effect] レスト敵{restCount}体 → BPを永続で +{buffAmount}");

		// ③ 永続バフを適用
		context.selfField.AddPermanentAttack(buffAmount, 0);
	}

	void ResolveRestAllThenGainAP(CardAI card, EffectContextAI context)
	{
		if (context.enemyPlayerField == null) return;
		if (context.selfField == null) return;

		// 敵モンスター取得
		var enemyCards = context.enemyPlayerField.GetAllCards();

		if (enemyCards.Count == 0)
		{
			Debug.Log("[Effect] 敵モンスターがいないのでレストできない");
			return;
		}

		int restedCount = 0;

		// ============================
		// 敵全体をレストさせる
		// ============================
		foreach (var c in enemyCards)
		{
			if (!c.IsRested)
			{
				c.Rest();
				restedCount++;
			}
		}

		Debug.Log($"[Effect] 敵モンスターを {restedCount}体レストさせた");

		// ============================
		// レストした数だけ打撃力＋1（永続）
		// permanentBattleBonus を増やす
		// ============================

		context.selfField.AddPermanentAttack(
			0,              // BPは増やさない
			restedCount     // 打撃力を restedCount 分増加
		);

		Debug.Log($"[Effect] 自分の打撃力を永続で +{restedCount}した！");
	}

	void ResolveHeal(CardEffect effect, CardAI card, EffectContextAI context)
	{
		if (context.enemyLife == null || context.selfLife == null)
		{
			Debug.LogError("[Effect] ライフ参照が null です");
			return;
		}

		int damage = effect.extraValue;      // 相手に与える量
		int healAmount = effect.effectValue;   // 自分が回復する量

		// 相手ライフにダメージ
		context.enemyLife.TakeDamage(damage);
		Debug.Log($"[Effect] {card.cardName}：相手ライフ -{damage}");

		// 自分のライフを回復
		context.selfLife.Heal(healAmount);
		Debug.Log($"[Effect] {card.cardName}：自分ライフ +{healAmount}");
	}

	void ResolveDebuffBP(CardEffect effect, CardAI card, EffectContextAI context)
	{
		// 相手フィールドのカードを取得
		selectableTargets = context.enemyPlayerField.GetAllCards().ToList();

		if (selectableTargets.Count == 0)
		{
			Debug.Log("[Effect] 相手モンスターがいない");
			return;
		}

		// ---------------------------
		// extraValueが10以上なら自動全体デバフ
		// ---------------------------
		if (effect.extraValue >= 10)
		{
			foreach (var c in selectableTargets)
			{
				c.AddPermanentAttack(effect.effectValue, 0);
				Debug.Log($"[Effect] {c.CardName} のBPを {effect.effectValue} 下げた（全体自動）");
			}
			return;
		}

		// ---------------------------
		// それ以外は選択式
		// ---------------------------
		foreach (var c in selectableTargets)
			c.Highlight(true);

		isSelectingTarget = true;
		effectContext = context;

		// ★効果予約
		pendingDebuffBP = true;

		// ★BP変化量
		debuffValue = effect.effectValue;

		// ★対象数（extraValue体）
		remainingDebuffCount = effect.extraValue;

		Debug.Log($"[Effect] 相手モンスターを {remainingDebuffCount}体選び、BPを {debuffValue} 下げてください");
	}


	void ResolveSacrificeOnBlockLose(CardEffect effect, EffectContextAI context)
	{
		if (context.selfField == null) return;

		context.selfField.sacrificeOnBlockLose = true;

		Debug.Log($"[Effect] {context.selfField.CardName} は道連れ状態になった！");
	}

	void ResolveNoRestAfterBlock(CardEffect effect, EffectContextAI context)
	{
		if (context.selfField == null) return;

		context.selfField.noRestAfterBlock = true;

		Debug.Log($"[Effect] {context.selfField.CardName} はブロック後レストしない！");
	}

	void ResolveBuffByOwnField(CardEffect effect, CardAI card, EffectContextAI context)
	{
		var ownCards = context.selfPlayerField.GetAllCards();
		if (ownCards.Count == 0)
		{
			Debug.Log("[Effect] 自分の場にモンスターがいません");
			return;
		}

		int buffValue = effect.effectValue * ownCards.Count;

		foreach (var c in ownCards)
		{
			c.AddPermanentAttack(buffValue, buffValue); // 永続バフ
			Debug.Log($"[Effect] {c.CardName} のBPを {buffValue} 上げました");
		}
	}

	void ResolveDestroyAllAndHeal(CardEffect effect, CardAI card, EffectContextAI context)
	{
		int destroyedCount = 0;

		// 破壊対象を取得（召喚したカード以外）
		var selfTargets = context.selfPlayerField.GetAllCards()
			.Where(c => c != context.selfField)
			.ToList();

		var enemyTargets = context.enemyPlayerField.GetAllCards()
			.ToList();

		// 両方のカードを破壊
		foreach (var c in selfTargets.Concat(enemyTargets))
		{
			context.selfPlayerField.RemoveFieldCard(c); // 自分のカードならRemove
			context.enemyPlayerField.RemoveFieldCard(c); // 相手のカードならRemove
			AttackManagerAI.Instance.DestroyCard(c);
			destroyedCount++;
		}

		// 破壊した数だけライフ回復
		if (context.selfLife != null && destroyedCount > 0)
		{
			context.selfLife.Heal(destroyedCount);
			Debug.Log($"[Effect] {card.cardName}：破壊した {destroyedCount} 枚分ライフ回復");
		}
	}

	// -----------------------------
	// 呪文効果：相手モンスター1体の打撃力を0にする
	// -----------------------------
	// -----------------------------
	// 呪文：相手モンスター1体のBPを0にする
	// -----------------------------
	public void ResolveSpellSetBPZero(CardAI card, EffectContextAI context)
	{
		var enemyField = context.enemyPlayerField;
		if (enemyField == null) return;

		var targets = enemyField.GetAllCards().ToList();
		if (targets.Count == 0)
		{
			Debug.Log("[Effect] 相手モンスターがいません");
			return;
		}

		// 選択可能対象にセット
		selectableTargets = targets;

		// ハイライト
		foreach (var c in selectableTargets)
			c.Highlight(true);
		isSelectingTarget = true;
		// クリック待ちフラグ
		pendingDefenseZero = true;

		// 選択用コンテキスト
		effectContext = context;

		// 選択数は1体
		remainingDebuffCount = 1;

		Debug.Log("[Effect] 相手モンスター1体をクリックしてBPを0にしてください");
	}

}
