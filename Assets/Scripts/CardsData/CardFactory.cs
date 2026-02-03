using UnityEngine;

public static class CardFactory
{
    /// <summary>
    /// CardData（静的データ）からゲーム内カード(Card)を生成
    /// </summary>
    public static Card CreateFromData(CardData data, int ownerId)
    {
        // --- Card インスタンス作成 ---
        Card card = new Card();
        card.id = data.cardID;
        card.OwnerId = ownerId;
        card.ap = data.ap;
        card.bp = data.bp;
        // --- コスト情報を反映 ---
        card.manaCostList.Clear();

        if (data.manaCosts != null)
        {
            foreach (var entry in data.manaCosts)
            {
                card.manaCostList.Add(new ManaCostEntry
                {
                    color = entry.color,  // ✅ ManaColor 型なのでそのままでOK
                    cost = entry.cost
                });
            }
        }

        card.anyColorCost = data.anyColorCost;

        // --- 初期化処理 ---
        card.Initialize(card.id, ownerId, card.anyColorCost, card.ap, card.bp);

        return card;
    }
}
