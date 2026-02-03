using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomClickManager : MonoBehaviour
{
	public RectTransform titleImage;

	[Header("クリック判定エリア（正規化 0〜1）")]
	[SerializeField] private Rect createRoomArea = new Rect(0.1f, 0.2f, 0.2f, 0.15f);
	[SerializeField] private Rect joinRoomArea = new Rect(0.7f, 0.2f, 0.2f, 0.15f);

	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			Vector2 localPoint;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				titleImage, Input.mousePosition, null, out localPoint);

			Vector2 normalized = RectPointToNormalized(titleImage, localPoint);

			// ルーム作成エリア
			if (createRoomArea.Contains(normalized))
			{
				Debug.Log("ルーム作成クリック！");
				HomeUIManager.Instance.OnCreateRoomClicked();
			}

			// ルーム参加エリア
			if (joinRoomArea.Contains(normalized))
			{
				Debug.Log("ルーム参加クリック → ID入力表示");
				RoomInputManager rim = FindObjectOfType<RoomInputManager>();
				if (rim != null)
					rim.isInputActive = true;
			}
		}

		// 戻る
		if (Input.GetKeyDown(KeyCode.Backspace))
		{
			if (!string.IsNullOrEmpty(SceneHistory.beforeScene))
			{
				SceneManager.LoadScene(SceneHistory.beforeScene);
			}
		}

		// ゲーム終了
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

		DrawAreaGizmo(createRoomArea, Color.red);
		DrawAreaGizmo(joinRoomArea, Color.blue);
	}

	void DrawAreaGizmo(Rect area, Color color)
	{
		Rect rect = titleImage.rect;
		Vector3[] corners = new Vector3[4];
		titleImage.GetWorldCorners(corners);

		float width = rect.width * area.width * titleImage.lossyScale.x;
		float height = rect.height * area.height * titleImage.lossyScale.y;

		Vector3 pos = titleImage.position + new Vector3(
			(area.x - 0.5f + area.width / 2) * rect.width * titleImage.lossyScale.x,
			(area.y - 0.5f + area.height / 2) * rect.height * titleImage.lossyScale.y,
			0
		);

		Gizmos.color = new Color(color.r, color.g, color.b, 0.4f);
		Gizmos.DrawCube(pos, new Vector3(width, height, 0));
	}
}
