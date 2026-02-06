using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class PlayerDeckAI : MonoBehaviour
{
	[Header("カードデータ")]
	public List<CardAI> allCards = new List<CardAI>(); // Inspectorで10種類のカードをセット

	[Header("デッキ/手札")]
	public List<CardAI> deck = new List<CardAI>();     // 40枚のデッキ（10種類×4枚）
	public List<CardAI> hand = new List<CardAI>();     // プレイヤーの手札

	[Header("手札UI")]
	public Transform handParent;                   // 手札表示用の親オブジェクト
	public GameObject cardPrefab;                  // カード表示用Prefab
	public GameObject playerAi;

	public GameObject playerAI;

	bool isGameEnd = false;

	//public HandManagerAI handManager;

	[System.Serializable]
	public class DeckEntry
	{
		public string cardID;
		public int count;
	}
	public List<DeckEntry> deckRecipe = new List<DeckEntry>();



	// 現在の手札枚数
	public int HandCount => hand.Count;

	public List<CardAI> GetHandCards()
	{
		return hand;
	}


	void Start()
	{
		InitializeDeckFromSavedDeck();
	}

    void InitializeDeckFromSavedDeck()
    {
        deck.Clear();
        string path = ""; // 初期化

        // ① 選択中のデッキ名を取得
        string deckName = DeckDataManager.Instance.SelectedDeckName;
        if (string.IsNullOrEmpty(deckName))
        {
            Debug.LogError("Battle開始時にデッキが選択されていません！");
            return;
        }

        // ---------------------------
        // tsukasa_2 優先ロジック: ユーザーデッキ or サンプルデッキの判定
        // ---------------------------
        string userPath = Path.Combine(
            DeckDataManager.Instance.DeckDirectory,
            deckName + ".json"
        );

        string jsonContent = "";

        if (File.Exists(userPath))
        {
            // ユーザーが作成したデッキが存在する場合
            jsonContent = File.ReadAllText(userPath);
            Debug.Log($"ユーザーデッキを読み込みます: {userPath}");
        }
        else
        {
            // 存在しない場合はResources（サンプルデッキ）から読み込み
            TextAsset sample = Resources.Load<TextAsset>("Decks/" + deckName);
            if (sample != null)
            {
                jsonContent = sample.text;
                Debug.Log($"サンプルデッキをResourcesから読み込みます: {deckName}");
            }
            else
            {
                Debug.LogError($"デッキファイルが見つかりません (User/Sample共): {deckName}");
                return;
            }
        }

        // ③ JSON読み込み
        DeckSaveData data = JsonUtility.FromJson<DeckSaveData>(jsonContent);

        Debug.Log($"Battle用デッキデータ展開成功: {data.deckName}");

        // ④ cardIds を元に40枚デッキ生成
        foreach (int id in data.cardIds)
        {
            // IDを2桁の文字列に変換 (例: 1 -> "01")
            string idString = id.ToString().PadLeft(2, '0');

            // allCardsから一致するカードを探す
            CardAI cardData = allCards.Find(card => card.cardID == idString);

            if (cardData == null)
            {
                Debug.LogWarning($"カードIDがallCardsに存在しません: {idString}");
                continue;
            }

            deck.Add(cardData);
        }

        // ⑤ シャッフル
        ShuffleDeck();

        Debug.Log($"デッキ初期化完了：{deck.Count}枚");
    }

    // デッキを初期化
    void InitializeDeck()
	{
		//deck.Clear();
		//
		//// 各カードを4枚ずつ追加
		//foreach (var card in allCards)
		//{
		//	for (int i = 0; i < 4; i++)
		//	{
		//		deck.Add(card);
		//	}
		//}
		//
		//ShuffleDeck();

		deck.Clear();

		foreach (var entry in deckRecipe)
		{
			// allCardsからID一致カードを探す
			CardAI cardData = allCards.Find(card => card.cardID == entry.cardID);

			if (cardData == null)
			{
				continue;
			}

			// 指定枚数追加
			for (int i = 0; i < entry.count; i++)
			{
				deck.Add(cardData);
			}
		}

		ShuffleDeck();

	}

	// デッキをシャッフル
	void ShuffleDeck()
	{
		for (int i = 0; i < deck.Count; i++)
		{
			CardAI temp = deck[i];
			int randomIndex = Random.Range(i, deck.Count);
			deck[i] = deck[randomIndex];
			deck[randomIndex] = temp;
		}
	}

	// デッキから1枚ドロー
	public void DrawCard(PlayerSide drawingPlayer)
	{
		if (isGameEnd) return;

		// デッキ切れ = 負け
		if (deck.Count == 0)
		{
			isGameEnd = true;

			bool isWin =
				(drawingPlayer == PlayerSide.Enemy); // 敵が引けない = 勝ち

			ResultUI.Instance.ShowResult(isWin);
			return;
		}


		if (hand.Count >= 8)
		{
			CardAI burnedCard = deck[0];
			deck.RemoveAt(0);

			return;
		}

		CardAI drawnCard = deck[0];
		deck.RemoveAt(0);
		hand.Add(drawnCard);

		if (cardPrefab != null && handParent != null)
		{
			Vector3 pos = new Vector3(-0.35f, 0f, 0f);
			Quaternion rot = Quaternion.Euler(90f, 0f, 180f); // この角度にしないとカードが正面向かない
			GameObject cardGO = Instantiate(cardPrefab, pos, rot, handParent);
			cardGO.transform.localPosition = new Vector3(0f, 0f, 0f);
			cardGO.transform.localScale = Vector3.one * 2.0f;
			// ① データをセット
			CardDisplayAI display = cardGO.GetComponent<CardDisplayAI>();
			display.Setup(drawnCard);
			//Debug.Log($"[Draw] cardName={drawnCard.cardName}, cardID={drawnCard.cardID}");

			// ★ ここで誰のカードかと場所をセット
			display.OwnerSide = drawingPlayer;
			display.Location = CardLocation.Hand;

			FkingCardDisplayAttacher visual = cardGO.GetComponentInChildren<FkingCardDisplayAttacher>();


			if (visual != null)
			{
				// 敵の手札なら裏面表示
				if (drawingPlayer == PlayerSide.Enemy)
				{
					visual.SetAIFaceDownMode();
				}
				else
				{
					visual.ChangeCard(drawnCard.cardID);
				}
			}
			else
			{
				Debug.LogError("FkingCardDisplayAttacher が子オブジェクトに見つかりません");
			}


			// ドロー演出
			StartCoroutine(MotionManager.Instance.draw.StartDraw(cardGO, handParent.gameObject));
		}

	}


	public void RemoveFromHand(CardDisplayAI display)
	{
		// 手札からデータ削除
		if (hand.Contains(display.cardData))
			hand.Remove(display.cardData);

		// UIカード削除
		Destroy(display.gameObject);

		//if (HandManagerAI.Instance != null)
		//	HandManagerAI.Instance.ArrangeHand();
	}

	public CardDisplayAI DrawCardAndGetDisplay(PlayerSide drawingPlayer)
	{
		if (deck.Count == 0) return null;
		if (hand.Count >= 8) return null; // 上限があるなら

		CardAI drawnCard = deck[0];
		deck.RemoveAt(0);
		hand.Add(drawnCard);

		GameObject cardGO = Instantiate(cardPrefab, handParent);

		CardDisplayAI display = cardGO.GetComponent<CardDisplayAI>();
		display.Setup(drawnCard);
		display.OwnerSide = drawingPlayer;
		display.Location = CardLocation.Hand;

		var visual = cardGO.GetComponentInChildren<FkingCardDisplayAttacher>();
		if (visual != null)
			visual.ChangeCard(drawnCard.cardID);

		// 並び替え（これが絶対必要）
		//if (HandManagerAI.Instance != null)
		HandManagerAI.Instance.ArrangeHand();

		return display; // ★ これが超重要
	}



}

