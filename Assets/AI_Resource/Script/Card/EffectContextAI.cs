public class EffectContextAI
{
	public SummonSide ownerSide;

	// 盤面情報（攻撃・ブロック用）
	public FieldCardDisplayAI selfField;
	public FieldCardDisplayAI targetField;

	// ★ 効果で使う実体
	public PlayerDeckAI ownerDeck;

	// カードデータ
	public CardAI self;
	public CardAI target;

	public PlayerFieldAI selfPlayerField;
	public PlayerFieldAI enemyPlayerField;

	public PlayerHpUIAI enemyLife;
	public PlayerHpUIAI selfLife;


}
