using UnityEngine;
using TMPro;

public class HomeUIManager : MonoBehaviour
{
	public static HomeUIManager Instance { get; private set; }

	[Header("UI")]
	public TMP_Text roomCodeText; // ルームID表示用

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	public void OnCreateRoomClicked()
	{
		Debug.Log("✅ ルーム作成クリック");
		NetworkManager.Instance.CreateRoom();
	}

	public void OnJoinRoomClicked(string roomID)
	{
		if (!string.IsNullOrEmpty(roomID))
		{
			NetworkManager.Instance.JoinRoom(roomID);
		}
		else
		{
			ShowWarning("ルームIDを入力してください");
		}
	}

	public void SetRoomID(string id)
	{
		if (roomCodeText != null)
			roomCodeText.text = "Room ID: " + id;
	}

	public void ShowWarning(string message)
	{
		if (roomCodeText != null)
			roomCodeText.text = message;
	}
}
