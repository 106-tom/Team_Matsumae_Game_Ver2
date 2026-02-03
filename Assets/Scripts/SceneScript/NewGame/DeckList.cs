using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDeck", menuName = "CardGame/DeckList")]
public class DeckList : ScriptableObject
{
	//public List<CardData2> cards = new List<CardData2>();
	public List<CardData2> cards = new List<CardData2>(10);
	

}
