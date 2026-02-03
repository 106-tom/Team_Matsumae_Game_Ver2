using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class RoomManager : MonoBehaviour
{
	[Header("タイトル画像")]
	public RectTransform titleImage;

	[Header("クリック判定エリア（正規化 0〜1）")]
	[SerializeField] private Rect createRoomArea = new Rect(0.1f, 0.2f, 0.2f, 0.15f);
	[SerializeField] private Rect joinButtonArea = new Rect(0.7f, 0.35f, 0.2f, 0.1f);
	[SerializeField] private Rect inputArea = new Rect(0.7f, 0.2f, 0.2f, 0.1f);

	[Header("ID入力")]
	public TMP_Text roomCodeText;

	[SerializeField] private Vector2 roomIDNormalizedPos = new Vector2(0.5f, 0.1f); // 画像の中央下
	[SerializeField] private Vector2 roomIDSize = new Vector2(0.2f, 0.05f); // 表示する長方形の比率


	public string roomCode = "";
	public bool isInputActive = false;
	public int maxDigits = 5;

	void Start()
	{
		UpdateRoomIDPosition();
	}


	void Update()
	{
		HandleMouseClick();
		HandleKeyboardInput();
		UpdateRoomCodeText();
		HandleSceneNavigation();
	}

	private void HandleMouseClick()
	{
		if (!Input.GetMouseButtonDown(0)) return;

		Vector2 localPoint;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(titleImage, Input.mousePosition, null, out localPoint);
		Vector2 normalized = RectPointToNormalized(titleImage, localPoint);

		// ルーム作成
		if (createRoomArea.Contains(normalized))
		{
			NetworkManager.Instance.CreateRoom();
		}

		// 入力エリアクリックで入力モードON
		if (inputArea.Contains(normalized))
		{
			isInputActive = true;
		}

		// 入室ボタンクリック
		if (joinButtonArea.Contains(normalized))
		{
			TryJoinRoom();
		}
	}

	private void HandleKeyboardInput()
	{
		if (!isInputActive) return;

		foreach (char c in Input.inputString)
		{
			if (char.IsDigit(c) && roomCode.Length < maxDigits)
				roomCode += c;

			if (c == '\b' && roomCode.Length > 0)
				roomCode = roomCode.Substring(0, roomCode.Length - 1);
		}

		// Enterキーで入室
		if (Input.GetKeyDown(KeyCode.Return))
		{
			TryJoinRoom();
		}
	}

	private void UpdateRoomCodeText()
	{
		if (roomCodeText != null)
			roomCodeText.text = string.IsNullOrEmpty(roomCode) ? "-----" : roomCode;
	}

	private void HandleSceneNavigation()
	{
		if (Input.GetKeyDown(KeyCode.Backspace))
		{
			if (!string.IsNullOrEmpty(SceneHistory.beforeScene))
				SceneManager.LoadScene(SceneHistory.beforeScene);
		}

		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Debug.Log("ゲーム終了");
			Application.Quit();
		}
	}

	public void TryJoinRoom()
	{
		if (string.IsNullOrEmpty(roomCode) || roomCode.Length != maxDigits)
		{
			Debug.LogWarning("5桁のルームIDを入力してください");
			return;
		}

		Debug.Log("ルーム参加リクエスト：" + roomCode);
		NetworkManager.Instance.JoinRoom(roomCode);
	}

	private Vector2 RectPointToNormalized(RectTransform rect, Vector2 localPoint)
	{
		float x = (localPoint.x / rect.rect.width) + 0.5f;
		float y = (localPoint.y / rect.rect.height) + 0.5f;
		return new Vector2(x, y);
	}

	private void UpdateRoomIDPosition()
	{
		if (roomCodeText == null || titleImage == null) return;

		float x = (roomIDNormalizedPos.x - 0.5f) * titleImage.rect.width;
		float y = (roomIDNormalizedPos.y - 0.5f) * titleImage.rect.height;

		roomCodeText.rectTransform.localPosition = new Vector3(x, y, 0f);
	}

	void OnDrawGizmos()
	{
		if (titleImage == null) return;
		DrawAreaGizmo(createRoomArea, Color.red);
		DrawAreaGizmo(joinButtonArea, Color.green);
		DrawAreaGizmo(inputArea, Color.cyan);

		// ルームID表示位置のGizmo（長方形）
		Rect rect = titleImage.rect;
		float width = rect.width * roomIDSize.x * titleImage.lossyScale.x;
		float height = rect.height * roomIDSize.y * titleImage.lossyScale.y;

		Vector3 pos = titleImage.position + new Vector3(
			(roomIDNormalizedPos.x - 0.5f) * rect.width * titleImage.lossyScale.x,
			(roomIDNormalizedPos.y - 0.5f) * rect.height * titleImage.lossyScale.y,
			0
		);

		Gizmos.color = new Color(1f, 1f, 0f, 0.4f); // 半透明黄色
		Gizmos.DrawCube(pos, new Vector3(width, height, 0));
	}

	void DrawAreaGizmo(Rect area, Color color)
	{
		Rect rect = titleImage.rect;
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
