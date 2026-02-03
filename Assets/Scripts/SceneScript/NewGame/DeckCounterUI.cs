using UnityEngine;
using TMPro;

public class DeckCounterUI : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI deckCountText;

	public void UpdateCount(int count)
	{
		deckCountText.text = count.ToString();
	}
}
