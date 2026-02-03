using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClickManager : MonoBehaviour
{
	public RectTransform titleImage;
	[SerializeField] private Rect battleArea = new Rect(0.7f, 0.1f, 0.2f, 0.15f);
	[SerializeField] private string nextSceneName = "menu";

	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			Vector2 localPoint;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				titleImage, Input.mousePosition, null, out localPoint);

			Vector2 normalized = RectPointToNormalized(titleImage, localPoint);

			if (battleArea.Contains(normalized))
			{
				Debug.Log("対戦準備画面へ遷移！");
				SceneManager.LoadScene(nextSceneName);
			}
		}
		if (Input.GetKeyDown(KeyCode.Backspace))
		{
			if (!string.IsNullOrEmpty(SceneHistory.beforeScene))
			{
				SceneManager.LoadScene(SceneHistory.beforeScene);
			}
		}
		// ESCキーでゲーム終了
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Debug.Log("ゲーム終了");
			Application.Quit();
		}
	}

	Vector2 RectPointToNormalized(RectTransform rect, Vector2 localPoint)
	{
		float x = (localPoint.x / rect.rect.width) + 0.5f;
		float y = (localPoint.y / rect.rect.height) + 0.5f;
		return new Vector2(x, y);
	}

	void OnDrawGizmos()
	{
		if (titleImage == null) return;

		// RectTransformの矩形情報を取得
		Rect rect = titleImage.rect;

		// battleAreaをワールド座標に変換
		Vector3[] corners = new Vector3[4];
		titleImage.GetWorldCorners(corners);

		// 正規化したbattleAreaをワールド上に投影
		Vector3 bottomLeft = Vector3.Lerp(corners[0], corners[3], battleArea.x);
		bottomLeft = Vector3.Lerp(bottomLeft, Vector3.Lerp(corners[1], corners[2], battleArea.x + battleArea.width), battleArea.y);

		float width = rect.width * battleArea.width * titleImage.lossyScale.x;
		float height = rect.height * battleArea.height * titleImage.lossyScale.y;

		Gizmos.color = new Color(1, 0, 0, 0.5f);
		Gizmos.DrawCube(titleImage.position + new Vector3((battleArea.x - 0.5f + battleArea.width / 2) * rect.width * titleImage.lossyScale.x,
														  (battleArea.y - 0.5f + battleArea.height / 2) * rect.height * titleImage.lossyScale.y, 0),
						new Vector3(width, height, 0));
	}
}
