using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class InGamePlayer
{
	public static InGamePlayer Instance;
	public int PlayerID { get; private set; }      // Photon ActorNumber
	public string Name { get; private set; }       // 繝励Ξ繧､繝､繝ｼ蜷・
	public int DeckID { get; private set; }        // 繝・ャ繧ｭID
	public ManaPool mana;
	public List<Card> deck;
	public List<Card> hand;
	public List<Card> graveyard;
	public List<Card> field;

	public int opponentHandCount = 0;
	public int DefaultLife = 20; // 初期ライフ

	public const int MAX_LIFE = 20; // 菴灘鴨縺ｮ譛螟ｧ蛟､
	public int currentLife = MAX_LIFE; // 迴ｾ蝨ｨ縺ｮ菴灘鴨

   
    // 謖・ｮ壹ョ繝・く繧辰ardData縺ｮ繝ｪ繧ｹ繝医〒險ｭ螳・
    public void SetDeck(List<CardData> cardDataList)
    {
        deck.Clear();
        foreach (var data in cardDataList)
        {
            deck.Add(CardFactory.CreateFromData(data, PlayerID));
        }
    }

	public ManaZoneManager manaZoneManager;

	// --- 繧ｳ繝ｳ繧ｹ繝医Λ繧ｯ繧ｿ ---
	public InGamePlayer(int playerID, string name, int deckID)
	{
		this.PlayerID = playerID;    // ActorNumber繧偵そ繝・ヨ
		this.Name = name;
		this.DeckID = deckID;

		// 蜷・ｦ∫ｴ繧貞・譛溷喧
		mana = new ManaPool();
		deck = new List<Card>();
		hand = new List<Card>();
		graveyard = new List<Card>();
		field = new List<Card>();

		manaZoneManager = new ManaZoneManager();
		currentLife = DefaultLife;
		LoadDeckFromJson("player1_deck");
		ShuffleDeck();
		Debug.Log($"[Init] InGamePlayer {PlayerID} ({Name}) 初期化完了");


		// 菴灘鴨繧呈怙螟ｧ蛟､縺ｧ蛻晄悄蛹・
		currentLife = MAX_LIFE;

	}

	// --- 繝・ャ繧ｭ繧偵す繝｣繝・ヵ繝ｫ ---
	public void ShuffleDeck()
	{
		System.Random rng = new System.Random();
		int n = deck.Count;
		while (n > 1)
		{
			n--;
			int k = rng.Next(n + 1);
			Card tmp = deck[k];
			deck[k] = deck[n];
			deck[n] = tmp;
		}
		Debug.Log($"[Player {PlayerID}] 繝・ャ繧ｭ繧偵す繝｣繝・ヵ繝ｫ縺励∪縺励◆");
	}

	// --- 繝峨Ο繝ｼ ---
	public void DrawCard()
	{
		if (deck.Count == 0)
		{
			return;
		}

		Card drawn = deck[0];
		deck.RemoveAt(0);
		hand.Add(drawn);

		Debug.Log($"[Player {PlayerID}] 繝峨Ο繝ｼ: {drawn.id}");

		if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
		{
			var action = new ActionSyncManager.PlayerActionData
			{
				actionType = ActionSyncManager.ActionType.DrawCard,
				actorId = PlayerID,
				cardId = drawn.id
			};
			ActionSyncManager.Instance.SyncPlayerAction(action);
		}
	}

	// --- 繧ｫ繝ｼ繝峨ｒ蝣ｴ縺ｫ蜃ｺ縺・---
	public bool PlayCard(Card card)
	{
		if (card == null || !hand.Contains(card)) return false;

		if (!mana.CanPay(card))
		{
			Debug.Log($"[Player {PlayerID}] 繝槭リ荳崎ｶｳ: {card.id}");
			return false;
		}

		mana.ConsumeMana(card);
		hand.Remove(card);
		field.Add(card);

		Debug.Log($"[Player {PlayerID}] 繧ｫ繝ｼ繝峨ｒ蝣ｴ縺ｫ蜃ｺ縺励∪縺励◆: {card.id}");
		return true;
	}

	// --- 繝槭リ繝√Ε繝ｼ繧ｸ ---
	public void ChargeMana()
	{
		foreach (ManaColor color in System.Enum.GetValues(typeof(ManaColor)))
		{
			mana.AddMana(color, 1);
		}
	}

	// --- 蠅灘慍縺ｸ騾√ｋ ---
	public void SendToGrave(Card card)
	{
		if (field.Contains(card))
		{
			field.Remove(card);
			graveyard.Add(card);
			Debug.Log($"[Player {PlayerID}] 蠅灘慍縺ｸ騾√ｋ: {card.id}");
		}
	}

	// --- ID縺ｧ繧ｫ繝ｼ繝画､懃ｴ｢ ---
	public Card FindCardById(int cardId)
	{
		Card found =
			hand.FirstOrDefault(c => c.id == cardId)
			?? field.FirstOrDefault(c => c.id == cardId)
			?? manaZoneManager?.cards?.Find(c => c.id == cardId);

		return found;

	}

	// --- 繝槭リUI譖ｴ譁ｰ ---
	public void UpdateManaUI()
	{
		// TODO: UI譖ｴ譁ｰ蜃ｦ逅・ｒ螳溯｣・
		// ManaZoneUI.Instance.Refresh(mana.GetCards());
	}

	// --- デッキをJSONからロード ---
	public void LoadDeckFromJson(string deckName)
	{
		TextAsset jsonFile = Resources.Load<TextAsset>($"Decks/{deckName}");
		if (jsonFile == null)
		{
			Debug.LogError($"デッキファイルが見つかりません: Resources/Decks/{deckName}.json");
			return;
		}

		DeckData deckData = JsonUtility.FromJson<DeckData>(jsonFile.text);
		if (deckData == null || deckData.cardIDs == null)
		{
			Debug.LogError("デッキデータの読み込みに失敗しました。");
			return;
		}

		deck.Clear();
		foreach (int id in deckData.cardIDs)
		{
			Card card = CardDatabase.Instance.CreateCardInstance(id, PlayerID);
			if (card != null)
				deck.Add(card);
		}

		Debug.Log($"プレイヤー{PlayerID}のデッキをロード: {deck.Count}枚");
	}
}
