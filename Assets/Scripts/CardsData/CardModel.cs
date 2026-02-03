using UnityEngine;

public class CardModel
{
	public int cardId;
	public string name;
	public string effectText;
	public int cost;
	public Sprite cardImage;
	public Sprite frameImage;
	public Sprite TextframeImage;
	public CardColor color;

	public CardModel(int cardID) // データを受け取り、その処理
	{
		CardEntity cardEntity = Resources.Load<CardEntity>("CardEntityList/CardEntity_" + cardID);

		if (cardEntity == null)
		{
			Debug.LogError("CardEntity が見つかりません: CardEntityList/Card" + cardID);
			return; // ここで止めないと次で必ず null 参照
		}
		cardId = cardEntity.Id;
		name = cardEntity.Name;
		effectText = cardEntity.CardText;
		cardImage = Resources.Load<Sprite>(cardEntity.ImagePath);
		frameImage = Resources.Load<Sprite>(cardEntity.FrameImagePath);
		TextframeImage = Resources.Load<Sprite>(cardEntity.TextFrameImagePath);
		cost = cardEntity.ManaCosts;
		color = cardEntity.Color;
	}
}