using UnityEngine;

public class CardVisualLayoutTunerAI : MonoBehaviour
{
	[Header("Targets")]
	[SerializeField] private RectTransform cardIllust;
	[SerializeField] private RectTransform textImage;
	[SerializeField] private RectTransform battleFrame;
	[SerializeField] private RectTransform frame;
	[SerializeField] private RectTransform red;
	[SerializeField] private RectTransform nameText;
	[SerializeField] private RectTransform effect;

	[Header("Layout Values (Manual Tuning)")]
	public RectLayout cardIllustLayout;
	public RectLayout textImageLayout;
	public RectLayout battleFrameLayout;
	public RectLayout frameLayout;
	public RectLayout redLayout;
	public RectLayout nameTextLayout;
	public RectLayout effectLayout;

	[Header("Apply")]
	public bool applyOnUpdate = true;

	void Reset()
	{
		CacheFromCurrent();
	}

	void Update()
	{
		if (applyOnUpdate)
			Apply();
	}

	// ==============================
	// 手動調整 → 反映
	// ==============================
	public void Apply()
	{
		ApplyRect(cardIllust, cardIllustLayout);
		ApplyRect(textImage, textImageLayout);
		ApplyRect(battleFrame, battleFrameLayout);
		ApplyRect(frame, frameLayout);
		ApplyRect(red, redLayout);
		ApplyRect(nameText, nameTextLayout);
		ApplyRect(effect, effectLayout);
	}

	// ==============================
	// 現在の値を保存（最重要）
	// ==============================
	[ContextMenu("Cache From Current")]
	public void CacheFromCurrent()
	{
		CopyRect(cardIllust, ref cardIllustLayout);
		CopyRect(textImage, ref textImageLayout);
		CopyRect(battleFrame, ref battleFrameLayout);
		CopyRect(frame, ref frameLayout);
		CopyRect(red, ref redLayout);
		CopyRect(nameText, ref nameTextLayout);
		CopyRect(effect, ref effectLayout);
	}

	// ==============================
	// util
	// ==============================
	void ApplyRect(RectTransform rt, RectLayout layout)
	{
		if (!rt) return;

		rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
		rt.pivot = new Vector2(0.5f, 0.5f);

		rt.anchoredPosition = layout.position;
		rt.sizeDelta = layout.size;
		rt.localEulerAngles = layout.rotation;
	}

	void CopyRect(RectTransform rt, ref RectLayout layout)
	{
		if (!rt) return;

		layout.position = rt.anchoredPosition;
		layout.size = rt.sizeDelta;
		layout.rotation = rt.localEulerAngles;
	}
}
