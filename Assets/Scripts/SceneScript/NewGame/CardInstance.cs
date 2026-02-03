using UnityEngine;

public class CardInstance
{
	public CardData2 cardData;   // 元データ
	public int currentAp;
	public int currentHp;
	public bool canAttack;
	public bool isOnField;
	public string instanceId;

	// ★ 新規追加：所有者 PlayerManager
	public PlayerManager ownerPlayer;

	// 元のコンストラクタ
	public CardInstance(CardData2 data)
	{
		this.cardData = data;
		this.currentAp = data.attack;
		this.currentHp = data.hp;
		this.canAttack = false;
		this.isOnField = false;
		this.instanceId = System.Guid.NewGuid().ToString();
	}

	// ★ 新規コンストラクタ：owner もセットできる
	public CardInstance(CardData2 data, PlayerManager owner) : this(data)
	{
		this.ownerPlayer = owner;
	}

	// ★ 後からセットするメソッド（必要に応じて）
	public void SetOwner(PlayerManager owner)
	{
		this.ownerPlayer = owner;
	}

	public int attack { get { return currentAp; } }
	public int hp { get { return currentHp; } set { currentHp = value; } }
}
