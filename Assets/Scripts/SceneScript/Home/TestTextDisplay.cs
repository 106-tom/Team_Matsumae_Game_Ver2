using TMPro;
using UnityEngine;

public class TestTextDisplay : MonoBehaviour
{
	[SerializeField] private TMP_Text testText;

	private void Start()
	{
		if (testText == null)
		{
			Debug.LogError("TMP_Textがアタッチされていません！");
			return;
		}

		testText.text = "Hello! これが表示されればOK";
		Debug.Log("テキスト表示更新完了");
	}
}
