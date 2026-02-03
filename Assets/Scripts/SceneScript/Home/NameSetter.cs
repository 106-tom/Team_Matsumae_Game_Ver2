using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class NameSetter : MonoBehaviourPunCallbacks
{
	public override void OnJoinedRoom()
	{
		SetTemporaryPlayerName();
	}

	void SetTemporaryPlayerName()
	{
		// ルーム内の自分の番号（1, 2, 3...）
		int index = PhotonNetwork.LocalPlayer.ActorNumber;

		// 番号1 → Player1（ホスト）、2 → Player2 になる
		string tempName = "Player" + index;

		PhotonNetwork.NickName = tempName;
		PhotonNetwork.LocalPlayer.NickName = tempName;

		Debug.Log($"プレイヤー名設定: {tempName}");
	}
}
