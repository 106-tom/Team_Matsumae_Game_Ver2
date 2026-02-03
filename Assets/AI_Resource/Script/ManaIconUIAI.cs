using UnityEngine;
using UnityEngine.UI;

public class ManaIconUIAI : MonoBehaviour
{
	public Image iconImage;

	private void Awake()
	{
		if (iconImage == null)
			iconImage = GetComponent<Image>();

		// ★ 起動時に強制リセット
		transform.localRotation = Quaternion.identity;
	}

	public void SetTapped(bool tapped)
	{
		// 半透明 or 通常
		iconImage.color = tapped
			? new Color(1, 1, 1, 0.4f)
			: Color.white;

		// ★ 必ず localRotation を使う
		transform.localRotation = tapped
			? Quaternion.Euler(0f, 0f, 90f)
			: Quaternion.identity;
	}
}
