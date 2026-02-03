using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardView : MonoBehaviour
{

	[SerializeField] TextMeshProUGUI costText;
	[SerializeField] TextMeshProUGUI nameText;
	[SerializeField] TextMeshProUGUI effectText;
	[SerializeField] TextMeshProUGUI stack;

	[SerializeField] Image iconImage;
	[SerializeField] Image frameImage;
	[SerializeField] Image textFrameImage;

	[SerializeField] Color imageAlpha;
	public Card CardData { get; private set; }   // ← public & get; private set;

	public void Setup(Card card)
	{
		CardData = card;
		nameText.text = $"Card {card.id}";
		costText.text = $"Cost: {card.TotalCost()}";
	}
	public void Show(CardModel cardModel) // cardModelのデータ取得と反映
	{
		if (cardModel.cardImage != null)
			iconImage.sprite = cardModel.cardImage;
		if (cardModel.frameImage != null)
			frameImage.sprite = cardModel.frameImage;
		if (cardModel.TextframeImage == null) Debug.Log("エラー");
		textFrameImage.sprite = cardModel.TextframeImage;
		costText.text = cardModel.cost.ToString();
		nameText.text = cardModel.name.ToString();
	}
	public void Apply(CardEntity entity)
	{
		nameText.text = entity.Name;
		effectText.text = entity.CardText;
		var CardSprite = Resources.Load<Sprite>(entity.ImagePath);
		if (CardSprite != null)
			iconImage.sprite = CardSprite;

		var FrameSprite = Resources.Load<Sprite>(entity.FrameImagePath);
		if (FrameSprite != null)
			frameImage.sprite = FrameSprite;
		var TextFrameSprite = Resources.Load<Sprite>(entity.TextFrameImagePath);
		if (TextFrameSprite != null)
			textFrameImage.sprite = TextFrameSprite;
	}
	public void SetCount(int count)
	{
		if (count < 1)
		{
			stack.gameObject.SetActive(false);
		}
		else
		{
			stack.gameObject.SetActive(true);
			stack.text = "×" + count;
		}
	}
}