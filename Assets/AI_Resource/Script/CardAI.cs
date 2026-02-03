using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CardAI
{
	public enum Type
	{
		召喚カード,
		魔法カード
	}

	public string cardID;  
	public string cardName;

	public int costColor;       // 必要な「色マナ」の数
	public string colorType;    // 色の種類 (例: "Red", "Blue" ...)
	public int costAny;         // 必要な「無色マナ」の数
	public Type type;

	public Sprite artwork;      // カードイラスト
	public int attack;          // 攻撃力
	public int defense;         // 打撃力（防御力）
								//public bool isRested = false;

	public bool requireAllColors = false;
	public int allColorsCost = 2; // 全色必要量


	// -------------------------
	// ◆ 全コスト（UI表示用）
	// -------------------------
	public int TotalCost
	{
		get { return costColor + costAny; }
	}
	public enum EffectTiming { None, Summon, TurnStart, Spell,Attack,Destroyed,Block }
	public enum EffectTarget { SelfMonster,AllSelfMonster, EnemyMonster, AllEnemyMonsters, None }
	public enum EffectType
	{ 
		Buff,
		DestroySingle,
		DestroyAll,
		Draw,
		DrawThenDestroy,
		DrawThenConditional,
		DestroyPerHandCount,
		DestroyByHandCountBP,
		SetBPByHandCount,
		DamageEnemyLifeByHandCount,
		DestroyPerHandCountSelect,
		DrawThenSetEnemyDefenseZero,
		DestroyThenDraw,
		Rest,
		DestroyThenRest,
		DrawPerRestEnemy,
		DestroyAllRestEnemy,
		BuffSelfPerRestEnemy,
		RestAllThenGainAP,
		Heal,
		DebuffBP,
		SacrificeOnBlockLose,
		NoRestAfterBlock,
		FieldCountBuff,
		DestroyAllHeal,
		DebuffAP,
		AllWin,
		None
	}

	public enum BuffDuration
	{
		Temporary,   // 一時（ターン終了・戦闘終了などで消える）
		Permanent,   // 永続
		None
	}

	public enum ConditionalDrawType
	{
		None,
		OddCost,
		BlueColor
	}

	// ★カードが持つ効果一覧（複数対応）
	public List<CardEffect> effects = new List<CardEffect>();


	[System.Serializable]
	public class CardEffect
	{
		public EffectTiming effectTimings;  // 発動タイミング
		public EffectTarget target;           // 効果対象
		public EffectType effectType;         // 効果の種類
		public int effectValue;               // 効果の値 (攻撃力増加や破壊BP上限など)
		public int extraValue;                // 複合効果用 (打撃力増加など)
		public BuffDuration buffDuration;
		public ConditionalDrawType conditionalDrawType;
	}

	public bool HasEffect(EffectType type)
	{
		foreach (var e in effects)
		{
			if (e.effectType == type)
				return true;
		}
		return false;
	}

	public Vector3 position { get; set; } // 追加　演出のための座標確保用
}
