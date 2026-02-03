using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using Photon.Pun;

public class RoomInputManager : MonoBehaviour
{
	[Header("入力表示用テキスト")]
	public TMP_Text roomCodeText;

	[Header("入力制御")]
	public string roomCode = ""; // 入力内容
	public bool isInputActive = false;
	public int maxDigits = 5;

	public RectTransform titleImage;
	public Rect inputArea = new Rect(0.3f, 0.2f, 0.4f, 0.1f); // 画像上の入力部分

	// Start is called before the first frame update
	private void Update()
	{
		// ① 入力エリアをクリックしたら入力モードON
		if (Input.GetMouseButtonDown(0))
		{
			Vector2 norm = GetNormalizedPosition();
			isInputActive = inputArea.Contains(norm);
		}

		// ② キーボード入力（数字のみ受付・5桁まで）
		if (isInputActive)
		{
			foreach (char c in Input.inputString)
			{
				if (char.IsDigit(c) && roomCode.Length < 5)
				{
					roomCode += c;
				}
				if (c == '\b' && roomCode.Length > 0) // Backspace
				{
					roomCode = roomCode.Substring(0, roomCode.Length - 1);
				}
			}

			if (Input.GetKeyDown(KeyCode.Return) && !string.IsNullOrEmpty(roomCode))
			{
				NetworkManager.Instance.JoinRoom(roomCode);
			}
		}

		if (roomCodeText != null)
		{
			roomCodeText.text = roomCode == "" ? "-----" : roomCode;
		}
	}

	public void TryJoinRoom()
	{
		if (string.IsNullOrEmpty(roomCode) || roomCode.Length != 5)
		{
			Debug.LogWarning("5桁のルームIDを入力してください");
			return;
		}

		Debug.Log("ルーム参加リクエスト：" + roomCode);

		// NetworkManager 経由で入室
		NetworkManager.Instance.JoinRoom(roomCode);
	}

	private Vector2 GetNormalizedPosition()
	{
		Vector2 local;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			titleImage, Input.mousePosition, null, out local);

		return new Vector2(
			(local.x / titleImage.rect.width) + 0.5f,
			(local.y / titleImage.rect.height) + 0.5f
		);
	}

	void OnDrawGizmos()
	{
		if (titleImage == null) return;

		// 画像の四隅をワールド座標で取得
		Vector3[] corners = new Vector3[4];
		titleImage.GetWorldCorners(corners);

		// 正規化した inputArea をワールド座標上に変換
		Vector3 posX1 = Vector3.Lerp(corners[0], corners[3], inputArea.x);
		Vector3 posX2 = Vector3.Lerp(corners[0], corners[3], inputArea.x + inputArea.width);
		Vector3 posY1 = Vector3.Lerp(corners[1], corners[2], inputArea.x);
		Vector3 posY2 = Vector3.Lerp(corners[1], corners[2], inputArea.x + inputArea.width);

		Vector3 bottomLeft = Vector3.Lerp(posX1, posY1, inputArea.y);
		Vector3 bottomRight = Vector3.Lerp(posX2, posY2, inputArea.y);
		Vector3 topLeft = Vector3.Lerp(posX1, posY1, inputArea.y + inputArea.height);
		Vector3 topRight = Vector3.Lerp(posX2, posY2, inputArea.y + inputArea.height);

		// 中心とサイズを計算
		Vector3 center = (bottomLeft + topRight) / 2f;
		Vector3 size = new Vector3(
			Vector3.Distance(bottomLeft, bottomRight),
			Vector3.Distance(bottomLeft, topLeft),
			0
		);

		// 色つける（ここ変えれば色変わる）
		Gizmos.color = new Color(0, 1, 1, 0.5f); // 🩵 シアン（透明50%）

		// 描画
		Gizmos.DrawCube(center, size);
	}


}
