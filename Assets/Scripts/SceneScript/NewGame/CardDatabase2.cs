using System.Collections.Generic;
using UnityEngine;

public class CardDatabase2 : MonoBehaviour
{
	public static CardDatabase2 Instance;

	[Tooltip("Inspectorに登録するカード一覧")]
	public List<CardData2> cardList = new List<CardData2>();

	private static Dictionary<int, CardData2> cardDict = new Dictionary<int, CardData2>();

	private void Awake()
	{
		Instance = this;

		cardDict.Clear();
		foreach (var card in cardList)
		{
			if (!cardDict.ContainsKey(card.cardId))
				cardDict.Add(card.cardId, card);
			else
				Debug.LogWarning($"カードIDが重複：{card.cardId}");
		}
	}

	public static CardData2 GetCardById(int id)
	{
		if (cardDict.TryGetValue(id, out var card))
			return card;

		Debug.LogError($"CardDatabase: ID {id} のカードが見つかりません");
		return null;
	}

	/// <summary>
	/// 全カードのコピーリストを返す（ランダムデッキ用）
	/// </summary>
	public List<CardData2> GetAllCardsCopy()
	{
		var list = new List<CardData2>();
		foreach (var card in cardList)
		{
			var copy = ScriptableObject.CreateInstance<CardData2>();
			copy.cardId = card.cardId;
			copy.cardName = card.cardName;
			copy.attack = card.attack;
			copy.hp = card.hp;
			copy.cost = card.cost;
			list.Add(copy);
		}
		return list;
	}
}
