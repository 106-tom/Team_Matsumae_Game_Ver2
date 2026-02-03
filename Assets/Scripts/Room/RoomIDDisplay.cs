using Photon.Pun;
using TMPro;
using UnityEngine;

public class RoomIDDisplay : MonoBehaviourPunCallbacks
{
	[SerializeField] private TMP_Text roomIdText;

	private void Start()
	{
		Debug.Log("RoomIDDisplay Start ✅");
		UpdateRoomID();
	}

	public override void OnJoinedRoom()
	{
		Debug.Log("OnJoinedRoom 発火！✅ ルームIDを更新");
		UpdateRoomID();
	}

	void UpdateRoomID()
	{
		if (PhotonNetwork.CurrentRoom != null)
		{
			roomIdText.text = "Room ID : " + PhotonNetwork.CurrentRoom.Name;
			Debug.Log("Room ID 表示完了: " + PhotonNetwork.CurrentRoom.Name);
		}
		else
		{
			roomIdText.text = "Room ID : ----";
			Debug.LogWarning("まだルーム情報が取れない...");
		}
	}
}
