using UnityEngine;

public class CardVisualLayoutApplierAI : MonoBehaviour
{
	[SerializeField] private RectTransform cardIllust;
	[SerializeField] private RectTransform textImage;
	[SerializeField] private RectTransform battleFrame;
	[SerializeField] private RectTransform frame;
	[SerializeField] private RectTransform red;
	[SerializeField] private RectTransform nameText;
	[SerializeField] private RectTransform effect;

	public void ApplyHandLayout()
	{
		ApplyRect(cardIllust, CardVisualLayoutPreset.Hand_CardIllust);
		ApplyRect(textImage, CardVisualLayoutPreset.Hand_TextImage);
		ApplyRect(battleFrame, CardVisualLayoutPreset.Hand_BattleFrame);
		ApplyRect(frame, CardVisualLayoutPreset.Hand_Frame);
		ApplyRect(red, CardVisualLayoutPreset.Hand_Red);
		ApplyRect(nameText, CardVisualLayoutPreset.Hand_NameText);
		ApplyRect(effect, CardVisualLayoutPreset.Hand_Effect);
	}

	void ApplyRect(RectTransform rt, RectLayout layout)
	{
		if (!rt) return;

		rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
		rt.pivot = new Vector2(0.5f, 0.5f);
		rt.anchoredPosition = layout.position;
		rt.sizeDelta = layout.size;
		rt.localEulerAngles = layout.rotation;
	}
}
