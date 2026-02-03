using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ResultSceneManager : MonoBehaviour
{
	public TextMeshProUGUI resultText;

	void Start()
	{
		if (ResultData.isWin)
		{
			resultText.text = "YOU WIN!!";
		}
		else
		{
			resultText.text = "YOU LOSE...";
		}
	}

	public void GoTitle()
	{
		SceneManager.LoadScene("TitleScene");
	}
}
