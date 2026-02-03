using System.Collections.Generic;

public class PlayerDeck
{
	public List<CardData2> deck = new List<CardData2>();
	public List<CardInstance> hand = new List<CardInstance>();

	// ƒJ[ƒh‚ğ1–‡ˆø‚­
	public CardInstance Draw()
	{
		if (deck.Count == 0) return null;

		var data = deck[0];
		deck.RemoveAt(0);

		var instance = new CardInstance(data);
		hand.Add(instance);
		return instance;
	}
}
