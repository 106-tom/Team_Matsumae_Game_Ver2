using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI nameText;
	[SerializeField] private Image artworkImage;
	[SerializeField] private Image backImage;
	[SerializeField] private TextMeshProUGUI costText;
	[SerializeField] private TextMeshProUGUI attackText;
	[SerializeField] private TextMeshProUGUI hpText;

	private string instanceId;

	public void Setup(CardData2 data, bool faceUp)
	{
		if (faceUp)
		{
			nameText.text = data.cardName;
			artworkImage.sprite = data.artwork;
			artworkImage.gameObject.SetActive(true);
			backImage.gameObject.SetActive(false);

			costText.text = data.cost.ToString();
			attackText.text = data.attack.ToString();
			hpText.text = data.hp.ToString();
		}
		else
		{
			// — Œü‚«•\Ž¦
			nameText.text = "";
			artworkImage.gameObject.SetActive(false);
			backImage.gameObject.SetActive(true);

			costText.text = "";
			attackText.text = "";
			hpText.text = "";
		}
	}

	public void SetInstance(CardInstance instance)
	{
		instanceId = instance.instanceId;
	}
}
