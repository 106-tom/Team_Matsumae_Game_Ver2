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
	[SerializeField] Image BGiconImage;
	[SerializeField] Image frameImage;
	[SerializeField] Image textFrameImage;

	[SerializeField] Image manaImage;
	[SerializeField] TextMeshProUGUI anyCost;
	[SerializeField] TextMeshProUGUI Over50CardCost;
	[SerializeField] TextMeshProUGUI AP;
	[SerializeField] TextMeshProUGUI BP;

	public string category;

	[SerializeField] Color imageAlpha;
	public Card CardData { get; private set; }   // Å© public & get; private set;

	public void Setup(Card card)
	{
		CardData = card;
		nameText.text = $"Card {card.id}";
	}
	public void Show(CardModel cardModel) // cardModelÇÃÉfÅ[É^éÊìæÇ∆îΩâf
	{
		if (cardModel.cardImage != null)
			iconImage.sprite = cardModel.cardImage;
			BGiconImage.sprite = cardModel.cardImage;
		if (cardModel.frameImage != null)
			frameImage.sprite = cardModel.frameImage;
		if (cardModel.TextframeImage != null)
			textFrameImage.sprite = cardModel.TextframeImage;
		if (cardModel.TextframeImage != null)
			manaImage.sprite = cardModel.manaImage;
		costText.text = cardModel.cost.ToString();
		anyCost.text = cardModel.anyCost.ToString();
		Over50CardCost.text = cardModel.anyCost.ToString();
		AP.text = cardModel.ap.ToString();
		BP.text = cardModel.bp.ToString();
		nameText.text = cardModel.name.ToString();
		effectText.text = cardModel.effectText.ToString();
		category = cardModel.category;
	}
	public void Apply(CardEntity entity)
	{
		nameText.text = entity.Name;
		effectText.text = entity.CardText;
		var CardSprite = Resources.Load<Sprite>(entity.ImagePath);
		if (CardSprite != null)
			iconImage.sprite = CardSprite;
			BGiconImage.sprite = CardSprite;

		var FrameSprite = Resources.Load<Sprite>(entity.FrameImagePath);
		if (FrameSprite != null)
			frameImage.sprite = FrameSprite;
		var TextFrameSprite = Resources.Load<Sprite>(entity.TextFrameImagePath);
		if (TextFrameSprite != null)
			textFrameImage.sprite = TextFrameSprite;
		var ManaSprite = Resources.Load<Sprite>(entity.ManaImagePath);
		if (ManaSprite != null)
			textFrameImage.sprite = ManaSprite;
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
			stack.text = "Å~" + count;
		}
	}
}