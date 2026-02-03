using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class PlayerDeckLoader : MonoBehaviour
{
	public static PlayerDeckLoader Instance;

	private void Awake()
	{
		Debug.Log("[DeckLoader] Awake 実行");
		Instance = this;
	}

	/// <summary>
	/// デッキを読み込んで PlayerManager に渡す
	/// </summary>
	public List<CardData2> LoadDeck(int deckSize = 10)
	{
		Debug.Log("[DeckLoader] LoadDeck 呼び出し");

		var allCards = CardDatabase2.Instance.GetAllCardsCopy();
		var deck = new List<CardData2>();

		if (allCards.Count == 0)
		{
			Debug.LogError("CardDatabase が空です。Inspectorでカードを登録してください。");
			return deck;
		}

		for (int i = 0; i < deckSize; i++)
		{
			// ランダムにカードを選んで追加
			int index = Random.Range(0, allCards.Count);
			var card = allCards[index];

			deck.Add(card);
		}

		Debug.Log($"[DeckLoader] デッキ{deckSize}枚生成完了");
		return deck;
	}
}
