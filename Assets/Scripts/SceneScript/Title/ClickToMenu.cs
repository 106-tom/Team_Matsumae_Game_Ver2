using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClickToMenu : MonoBehaviour
{
	[SerializeField] private string nextSceneName = "menu";

	// Update is called once per frame
	void Update()
	{
		// 画面のどこかをクリックしたら
		if (Input.GetMouseButtonDown(0))
		{
			SceneManager.LoadScene(nextSceneName);
		}

		// Escキーで終了
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false; // エディタで停止
#endif
		}
	}
}