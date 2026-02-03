using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UILayoutReleaseAI : MonoBehaviour
{
	void Start()
	{
		StartCoroutine(ReleaseLayout());
	}

	IEnumerator ReleaseLayout()
	{
		// 1フレーム待つ（Layout計算を終わらせる）
		yield return null;

		// 自分と親に付いている Layout 系を無効化
		foreach (var lg in GetComponentsInParent<LayoutGroup>())
			lg.enabled = false;

		foreach (var csf in GetComponentsInParent<ContentSizeFitter>())
			csf.enabled = false;
	}
}
