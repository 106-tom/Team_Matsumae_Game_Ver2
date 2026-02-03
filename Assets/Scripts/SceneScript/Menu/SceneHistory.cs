using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneHistory
{
	public static string beforeScene;

	public static void LoadScene(string nextScene)
	{
		beforeScene = SceneManager.GetActiveScene().name;
		SceneManager.LoadScene(nextScene);
	}
}
