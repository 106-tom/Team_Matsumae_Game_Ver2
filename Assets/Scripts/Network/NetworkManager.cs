using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class NetworkManager : MonoBehaviourPunCallbacks
{
	public static NetworkManager Instance;

	public string CreatedRoomName { get; private set; }
	private bool isConnectedToLobby = false;

	private void Awake()
	{
		if (Instance == null) Instance = this;
		else { Destroy(gameObject); return; }

		DontDestroyOnLoad(gameObject);
		PhotonNetwork.AutomaticallySyncScene = true;
	}

	private void Start()
	{
		if (!PhotonNetwork.IsConnected)
			PhotonNetwork.ConnectUsingSettings();
	}

	public override void OnConnectedToMaster()
	{
		Debug.Log("✅ Masterサーバー接続完了 → ロビー入室");
		PhotonNetwork.JoinLobby();
	}

	public override void OnJoinedLobby()
	{
		Debug.Log("✅ ロビー入室完了");
		isConnectedToLobby = true;
	}

	public void CreateRoom()
	{
		if (!isConnectedToLobby)
		{
			Debug.LogWarning("まだロビーに接続できていません。少し待ってから再試行してください。");
			return;
		}

		string roomName = Random.Range(10000, 99999).ToString();
		CreatedRoomName = roomName;

		RoomOptions options = new RoomOptions { MaxPlayers = 2 };
		PhotonNetwork.CreateRoom(roomName, options);

		Debug.Log("部屋作成リクエスト：" + roomName);
	}

	public void JoinRoom(string roomName)
	{
		if (!string.IsNullOrEmpty(roomName))
		{
			PhotonNetwork.JoinRoom(roomName);
			Debug.Log("入室リクエスト：" + roomName);
		}
	}

	public override void OnCreatedRoom()
	{
		Debug.Log("✅ 部屋作成成功：" + PhotonNetwork.CurrentRoom.Name);
		HomeUIManager.Instance.SetRoomID(PhotonNetwork.CurrentRoom.Name);
	}

	public override void OnJoinedRoom()
	{
		Debug.Log("✅ 部屋入室成功：" + PhotonNetwork.CurrentRoom.Name);

		// マスタークライアントだけゲームシーン遷移可能
		if (PhotonNetwork.IsMasterClient)
		{
			Debug.Log("マスタークライアントがゲーム開始シーンに遷移");
			PhotonNetwork.LoadLevel("Room2");
		}
	}

	public override void OnJoinRoomFailed(short returnCode, string message)
	{
		Debug.LogError("❌ 入室失敗：" + message);
		HomeUIManager.Instance.ShowWarning("ルームが見つかりません");
	}
}
