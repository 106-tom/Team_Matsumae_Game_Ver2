using System.Collections.Generic;
using UnityEngine;

public class ManaZoneManager
{
	public List<Card> cards = new List<Card>();

	// --- カードをマナゾーンに追加 ---
	public void AddCard(Card card)
	{
		if (!cards.Contains(card))
		{
			cards.Add(card);
			Debug.Log($"[ManaZone] Added card {card.id}");
		}
	}

	public void AddCard(Card card, Vector3? position = null)
	{
		if (!cards.Contains(card))
		{
			cards.Add(card);

			// 位置情報が渡されていたらUIマネージャーやView側に反映
			if (position.HasValue)
			{
				// 例：カードを物理的に配置するためのUIイベント発行
				Debug.Log($"[ManaZone] Added card {card.id} at position {position.Value}");
				//InGameUIManager.Instance?.MoveCardToManaZone(card, position.Value);
			}
			else
			{
				Debug.Log($"[ManaZone] Added card {card.id}");
			}
		}
	}

	// --- マナゾーンからカードを削除（必要に応じて） ---
	public void RemoveCard(Card card)
	{
		if (cards.Contains(card))
		{
			cards.Remove(card);
			Debug.Log($"[ManaZone] Removed card {card.id}");
		}
	}
}
