using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FieldCardDisplayAI : MonoBehaviour, IPointerClickHandler
{
	[Header("Field UI")]
	public Image artworkImage;
	public Text nameText;
	public Text attackText;
	public Text defenseText;

	[SerializeField]
	private CardAI cardData;
	private bool isAttacking = false;
	private int tempAttackBonus = 0;
	private int permanentAttackBonus = 0;
	private int tempBattleBonus = 0;
	private int permanentBattleBonus = 0;
	private int originalDefense;
	private bool defenseZeroUntilTurnEnd = false;
	public bool sacrificeOnBlockLose = false;
	public bool noRestAfterBlock = false;
	public bool usedDestroyCancel = false;

	public static FieldCardDisplayAI Instance;


	public PlayerSide OwnerSide { get; set; }

	public CardLocation Location { get; set; }

	public CardAI CardData => cardData;
	public string CardName => cardData.cardName;
	public int BaseAttack => cardData.attack;
	public int Attack => BaseAttack + tempAttackBonus + permanentAttackBonus;
	public int Defense => cardData.defense + tempBattleBonus + permanentBattleBonus;

	private bool isRested = false;
	public bool IsRested => isRested;

	private CardVisualController visual;

	void Awake()
	{
		Instance = this;
		visual = GetComponentInChildren<CardVisualController>();
		//if (visual == null) Debug.Log("ないよおおおおおおおおおおおおお");
	}

	public void Setup(CardAI card, PlayerSide ownerSide)
	{
		cardData = card;
		OwnerSide = ownerSide;

		if (nameText) nameText.text = card.cardName;
		if (attackText) attackText.text = card.attack.ToString();
		if (defenseText) defenseText.text = card.defense.ToString();
		if (artworkImage) artworkImage.sprite = card.artwork;

		if (visual == null)
		{
			//Debug.LogError("CardVisualController が設定されていません", this);
			return;
		}

		visual.ApplyLayout();
		visual.SetCardData(card);
		UpdateRestVisual();
	}

	void Update()
	{
		//RefreshStatsUI();
	}


	public void Rest()
	{
		isRested = true;
		UpdateRestVisual();
	}

	public void Unrest()
	{
		isRested = false;
		UpdateRestVisual();
	}

	// ★レスト状態の見た目更新
	// ★レスト状態の見た目更新
	public void UpdateRestVisual()
	{
		if (artworkImage == null || cardData == null) return;

		if (isRested)
		{
			// レスト時はカードを横向きに
			transform.localRotation = Quaternion.Euler(0f, 0f, 90f);

			// マナ足りるかチェック
			PlayerManaManagerAI targetMana = OwnerSide == PlayerSide.Self
				? SummonManagerAI.Instance.playerMana
				: SummonManagerAI.Instance.enemyMana;

			if (targetMana != null)
			{
				// 攻撃中は赤、マナ不足ならグレー、それ以外は白
				if (isAttacking)
					artworkImage.color = Color.red;
				else
					artworkImage.color = targetMana.CanPayCost(cardData) ? Color.white : Color.gray;
			}
		}
		else
		{
			// アンレスト時は通常の向きに戻す
			transform.localRotation = Quaternion.identity;

			// 攻撃中は赤、それ以外は白
			artworkImage.color = isAttacking ? Color.red : Color.white;
		}
	}



	public void SetAttacking(bool value)
	{
		isAttacking = value;

		if (artworkImage)
			artworkImage.color = value ? Color.red : Color.white;
	}

	public void AddPermanentAttack(int value, int value_)
	{
		permanentAttackBonus += value;
		permanentBattleBonus += value_;
		UpdateAttackUI();

		if (Attack <= 0)
		{
			if (AttackManagerAI.Instance != null)
			{
				AttackManagerAI.Instance.DestroyCard(this);
			}
		}
	}


	public void AddTempAttack(int value, int value_)
	{
		tempAttackBonus += value;
		tempBattleBonus += value_;
		UpdateAttackUI();

		if (Attack <= 0)
		{
			if (AttackManagerAI.Instance != null)
			{
				AttackManagerAI.Instance.DestroyCard(this);
			}
		}
	}

	public void ResetTempAttack()
	{
		tempAttackBonus = 0;
		tempBattleBonus = 0;
		UpdateAttackUI();
	}

	private void UpdateAttackUI()
	{
		if (attackText)
			attackText.text = Attack.ToString();
	}

	public void Highlight(bool on)
	{
		if (artworkImage)
			artworkImage.color = on ? Color.yellow : Color.white;
	}

	public void SetColor(Color color)
	{
		if (artworkImage != null)
		{
			artworkImage.color = color;
		}
	}

	public void SetAttack(int value)
	{
		cardData.attack = value;
		//UpdateUI();
	}

	public void SetDefense(int value)
	{
		cardData.defense = value;
		//UpdateUI();
	}

	public void SetDefenseZeroUntilTurnEnd()
	{
		// すでに0化されてるなら二重にしない
		if (defenseZeroUntilTurnEnd) return;

		originalDefense = Defense;
		CardData.defense= 0;

		defenseZeroUntilTurnEnd = true;

		//UpdateUI();

		Debug.Log($"[Effect] {CardName} の打撃力をターン終了まで0にした");
	}

	public void RestoreDefenseIfNeeded()
	{
		if (!defenseZeroUntilTurnEnd) return;

		cardData.defense = originalDefense;
		defenseZeroUntilTurnEnd = false;

		//UpdateUI();

		Debug.Log($"[Effect] {CardName} の打撃力が元に戻った");
	}



	public void OnPointerClick(PointerEventData eventData)
	{
		Debug.Log("[Effect]きてる",this);
		if (EffectManager.Instance != null && EffectManager.Instance.IsSelectingTarget)
		{
			EffectManager.Instance.OnTargetCardClicked(this);
			return;
		}

		OnClick();
	}


	public void OnClick()
	{
		if (PhaseManagerAI.Instance == null) return;

		PlayerSide currentTurn = TurnManagerAI.Instance.CurrentTurnSide;
		var phase = PhaseManagerAI.Instance.currentPhase;

		// ============================
		// Attackフェーズ：攻撃側だけ操作可能
		// ============================
		if (phase == PhaseManagerAI.Phase.Attack)
		{
			if (OwnerSide != currentTurn)
			{
				Debug.Log("攻撃フェーズ：他人のカードは操作できません");
				return;
			}

			if (AttackManagerAI.Instance.IsAttacking) return;

			AttackManagerAI.Instance.StartAttack(OwnerSide, this);

			// 攻撃したらブロックへ移行
			//PhaseManagerAI.Instance.currentPhase = PhaseManagerAI.Phase.Block;
			return;
		}

		// ============================
		// Blockフェーズ：防御側だけ操作可能
		// ============================
		if (phase == PhaseManagerAI.Phase.Block)
		{
			// 防御側を取得
			PlayerSide defendingSide =
				AttackManagerAI.Instance.defendingSide;

			// 防御側のカードしか押せない
			if (OwnerSide != defendingSide)
			{
				Debug.Log("ブロックフェーズ：防御側のカードを選んでください");
				return;
			}

			AttackManagerAI.Instance.BlockWithCard(this);
			return;
		}

		// ============================
		// Summonフェーズ：フィールドは無操作
		// ============================
		if (phase == PhaseManagerAI.Phase.Summon)
		{
			return;
		}
	}

	public void RefreshStatsUI()
	{
		FkingCardDisplay display = GetComponentInChildren<FkingCardDisplay>();
		if (display != null)
		{
			display.UpdateBattleStats(Defense, Attack);
		}
	}


}
