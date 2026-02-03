using UnityEngine;

[CreateAssetMenu(fileName = "CardData2", menuName = "Card Game/Card")]
public class CardData2 : ScriptableObject
{
	public int	cardId;
	public string cardName;
	public Sprite artwork;

	public int cost;
	public int attack;
	public int hp;

	public string description;
}
