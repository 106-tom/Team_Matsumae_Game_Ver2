using System.Collections;   // ★これ追加
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class PhaseManagerAI : MonoBehaviour
{
	public enum Phase { Start, Draw, Mana, Summon, Attack, Block, End }
	public static PhaseManagerAI Instance { get; private set; }

	[Header("UI")]
	public Text phaseText;

	[Header("参照")]
	public TurnManagerAI turnManager;
	public PlayerAI[] players; // 0 = 自分, 1 = 相手

	public int currentPlayerIndex = 0;
	public Phase currentPhase = Phase.Start;

	void Awake()
	{
		Instance = this;
	}

	void Start()
	{
		// manaManager 自動セット
		for (int i = 0; i < players.Length; i++)
		{
			if (players[i].manaManager == null)
			{
				PlayerManaManagerAI found = null;

				if (players[i].deck != null)
				{
					found = players[i].deck.GetComponentInChildren<PlayerManaManagerAI>();
				}

				if (found == null)
				{
					found = FindObjectOfType<PlayerManaManagerAI>();
				}

				if (found != null)
				{
					players[i].manaManager = found;
					Debug.Log($"Auto-assigned manaManager for player {i}");
				}
				else
				{
					Debug.LogError($"players[{i}].manaManager が設定されていません！");
				}
			}
		}

		// ★ 初期手札を配る
		DrawStartingHands();

		// 最初のフェーズ開始
		SetPhase(Phase.Start);
	}

	void Update()
	{
		// Enterキーでフェーズ進行（デバッグ用）
		if (Input.GetKeyDown(KeyCode.Return))
		{
			AdvancePhase();
		}
	}

	//==================================================
	// フェーズ進行
	//==================================================
	public void AdvancePhase()
	{
		switch (currentPhase)
		{
			case Phase.Start:
				SetPhase(Phase.Draw);
				break;

			case Phase.Draw:
				SetPhase(Phase.Mana);
				break;

			case Phase.Mana:
				SetPhase(Phase.Summon);
				break;

			case Phase.Summon:
				SetPhase(Phase.Attack);
				break;

			case Phase.Attack:
				SetPhase(Phase.End);
				break;

			case Phase.End:
				EndTurn();
				break;
		}
	}

	//==================================================
	// ターン終了処理
	//==================================================
	void EndTurn()
	{
		// ターン切替
		if (turnManager != null)
			turnManager.SwitchTurn();

		// プレイヤー交代
		currentPlayerIndex = 1 - currentPlayerIndex;

		// 次のターン開始
		SetPhase(Phase.Start);
	}

	//==================================================
	// フェーズ変更処理（統一）
	//==================================================
	void SetPhase(Phase next)
	{
		currentPhase = next;

		// フェーズ開始時処理を1回だけ呼ぶ
		OnPhaseEntered(next);

		// UI更新
		UpdateUI();
	}

	//==================================================
	// フェーズ開始時処理
	//==================================================
	void OnPhaseEntered(Phase phase)
	{
		PlayerSide turnSide = TurnManagerAI.Instance.CurrentTurnSide;

		switch (phase)
		{
			case Phase.Start:
				// アンタップ
				players[currentPlayerIndex].manaManager.UntapAll();
				//Debug.Log("Start Phase: UntapAll");

				// ★カードレスト解除
				PlayerFieldAI field =
					(turnSide == PlayerSide.Self)
						? SummonManagerAI.Instance.playerField
						: SummonManagerAI.Instance.enemyField;

				field.UnrestAllCards();

				//Debug.Log("Start Phase: UnrestAllCards");

				if (turnSide == PlayerSide.Enemy)
					StartCoroutine(AutoAdvanceAfterDelay(0.5f));

				break;

			case Phase.Draw:
				// ターン開始時効果
				SummonSide summonSide =
					(turnSide == PlayerSide.Self)
						? SummonSide.Player
						: SummonSide.Enemy;

				EffectManager.Instance.ResolveTurnStartEffects(summonSide);

				// ドロー処理
				players[currentPlayerIndex].deck.DrawCard(turnSide);

				//Debug.Log("Draw Phase: DrawCard");

				if (turnSide == PlayerSide.Enemy)
					StartCoroutine(AutoAdvanceAfterDelay(0.5f));
				break;

			case Phase.Mana:
				// 今ターンが敵かどうか
				if (turnSide == PlayerSide.Enemy)
				{
					//Debug.Log("敵のマナフェーズ：自動でマナ追加");

					StartCoroutine(EnemyAutoManaCharge());
				}
				else
				{
					//Debug.Log("プレイヤーのマナフェーズ：パネル表示");

					if (ManaUIAI.Instance != null)
						ManaUIAI.Instance.OpenPanel();
				}

				break;

			case Phase.Summon:

				if (turnSide == PlayerSide.Enemy)
				{
					//Debug.Log("敵の召喚フェーズ：自動召喚開始");
					StartCoroutine(EnemyAutoSummon());
				}
				else
				{
					//Debug.Log("プレイヤーの召喚フェーズ");
				}



				break;
			case Phase.Attack:
                HandManagerAI.Instance.ArrangeHand();
                if (turnSide == PlayerSide.Enemy)
				{
					Debug.Log(turnSide);
					StartCoroutine(EnemyAutoAttack());
				}
				else
				{

				}
				break;

			case Phase.Block:
				break;

			case Phase.End:
				//Debug.Log("End Phase");
				break;
		}
	}

	//==================================================
	// UI表示（日本語）
	//==================================================
	string GetPhaseName(Phase phase)
	{
		switch (phase)
		{
			case Phase.Start: return "開始";
			case Phase.Draw: return "ドロー";
			case Phase.Mana: return "マナ";
			case Phase.Summon: return "召喚";
			case Phase.Attack: return "攻撃";
			case Phase.Block: return "ブロック";
			case Phase.End: return "終了";
		}
		return "";
	}

	void UpdateUI()
	{
		if (phaseText == null) return;

		string playerName = players[currentPlayerIndex].playerName;

		phaseText.text =
			$"【{playerName}のターン】\n現在のフェーズ：{GetPhaseName(currentPhase)}";
	}

	//==================================================
	// マナ選択完了ボタン用
	//==================================================
	public void OnManaSelected()
	{
		// Manaフェーズ終了 → 次へ
		AdvancePhase();
	}

	//==================================================
	// 初期手札を配る（ゲーム開始時1回だけ）
	//==================================================
	void DrawStartingHands()
	{
		int startHandCount = 4;

		//Debug.Log("=== 初期手札配布開始 ===");

		// 自分に4枚
		for (int i = 0; i < startHandCount; i++)
		{
			players[0].deck.DrawCard(PlayerSide.Self);
		}

		// 相手に4枚
		for (int i = 0; i < startHandCount; i++)
		{
			players[1].deck.DrawCard(PlayerSide.Enemy);
		}

		//Debug.Log("=== 両プレイヤー初期手札4枚配布完了 ===");
	}

	IEnumerator EnemyAutoManaCharge()
	{
		// 少し待つ（演出）
		yield return new WaitForSeconds(1f);

		// ランダムで色を選ぶ
		string[] colors = { "Red" };
		string chosen = colors[Random.Range(0, colors.Length)];

		// 敵プレイヤーのmanaManagerに追加
		players[currentPlayerIndex].manaManager.AddMana(chosen);

		//Debug.Log($"敵がマナチャージした：{chosen}");

		// 少し待ってから次のフェーズへ
		yield return new WaitForSeconds(0.5f);

		// Manaフェーズ完了 → 次へ
		OnManaSelected();
	}

	IEnumerator EnemyAutoSummon()
	{
		yield return new WaitForSeconds(1f);

		Transform handParent = SummonManagerAI.Instance.enemyDeck.handParent;

		CardDisplayAI bestMonster = null;
		int bestMonsterCost = -1;

		CardDisplayAI bestSpell = null;
		int bestSpellCost = -1;

		// 手札を全部チェック
		foreach (Transform child in handParent)
		{
			CardDisplayAI cardDisplay = child.GetComponent<CardDisplayAI>();
			if (cardDisplay == null) continue;

			CardAI card = cardDisplay.cardData;

			// ===== モンスター召喚候補 =====
			if (card.type == CardAI.Type.召喚カード)
			{
				if (SummonManagerAI.Instance.enemyMana.CanPayCost(card))
				{
					if (card.TotalCost > bestMonsterCost)
					{
						bestMonsterCost = card.TotalCost;
						bestMonster = cardDisplay;
					}
				}
			}

			// ===== 呪文使用候補 =====
			if (card.type == CardAI.Type.魔法カード)
			{
				if (SummonManagerAI.Instance.enemyMana.CanPayCost(card))
				{
					if (card.TotalCost > bestSpellCost)
					{
						bestSpellCost = card.TotalCost;
						bestSpell = cardDisplay;
					}
				}
			}
		}

		// ===== ①召喚できるなら召喚 =====
		if (bestMonster != null)
		{
            //Debug.Log($"敵が召喚：{bestMonster.cardData.cardName}");
            bestMonster.cardData.position = 
				new Vector3(
					bestMonster.transform.localPosition.x - 884f, 
					bestMonster.transform.localPosition.y + 150f, 
					bestMonster.transform.localPosition.z);
			Debug.Log("敵のローカル位置 : " + bestMonster.transform.localPosition);
            SummonManagerAI.Instance.TrySummon(
				bestMonster.cardData,
				bestMonster,
				SummonSide.Enemy
			);
		}
		// ===== ②召喚できないなら呪文を探す =====
		else if (bestSpell != null)
		{
			//Debug.Log($"敵が呪文を使用：{bestSpell.cardData.cardName}");

			SummonManagerAI.Instance.TrySpell(
				bestSpell.cardData,
				bestSpell,
				SummonSide.Enemy
			);
		}
		// ===== ③何もできない =====
		else
		{
			//Debug.Log("敵は召喚も呪文もできません");
		}

		yield return new WaitForSeconds(0.5f);

		// Summonフェーズ終了 → 次へ
		AdvancePhase();
	}

	IEnumerator EnemyAutoAttack()
	{
		yield return new WaitForSeconds(1f);

		// -----------------------------
		// ★プレイヤーフィールドにカードがいるなら攻撃しない
		// -----------------------------
		var playerCards = SummonManagerAI.Instance.playerField.GetAllCards();

		if (playerCards.Count > 0)
		{
			//Debug.Log("プレイヤー場にカードがいるので敵は攻撃しません");

			AdvancePhase(); // 次フェーズへ
			yield break;
		}

		// -----------------------------
		// 敵フィールドのカード一覧を取得
		// -----------------------------
		var enemyCards = SummonManagerAI.Instance.enemyField.GetAllCards();

		// 攻撃できるカードだけ探す（レスト除外）
		FieldCardDisplayAI attacker = null;

		foreach (var card in enemyCards)
		{
			if (!card.IsRested)
			{
				attacker = card;
				break;
			}
		}

		// 攻撃できるカードがいないなら終了
		if (attacker == null)
		{
			//Debug.Log("敵は攻撃できるカードがいないので攻撃終了");

			AdvancePhase(); // 次フェーズへ
			yield break;
		}

		// -----------------------------
		// 攻撃開始（場が空なので直接攻撃）
		// -----------------------------
		Debug.Log($"敵が直接攻撃します → {attacker.CardName}");

		AttackManagerAI.Instance.StartAttack(PlayerSide.Enemy, attacker);

		// 攻撃が終わるまで待つ

		Debug.Log("攻撃中");

        yield return new WaitUntil(() =>
			AttackManagerAI.Instance.state == AttackState.None
		);

		// 次のフェーズへ
		AdvancePhase();
	}


	IEnumerator AutoAdvanceAfterDelay(float delay)
	{
		yield return new WaitForSeconds(delay);
		AdvancePhase();
	}


}
