//IDでカードを一意に管理する仕組み
using System.Collections.Generic;
using UnityEngine;

public static class CardRegistry
{
    private static Dictionary<int, Card> cardDict = new Dictionary<int, Card>();

    public static void RegisterCard(Card card)
    {
        if (!cardDict.ContainsKey(card.id))
            cardDict.Add(card.id, card);
    }

    public static Card GetCardById(int id)
    {
        cardDict.TryGetValue(id, out Card card);
        return card;
    }
}
