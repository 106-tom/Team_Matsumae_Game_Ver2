using Photon.Pun;
using System.Collections;
using UnityEngine;

public class GameStartInitializer : MonoBehaviourPunCallbacks
{
	[SerializeField] private Transform parentM1;
	public static bool IsReady { get; private set; } = false;

	public static int uiReadyCount = 0;

	private IEnumerator Start()
	{
		yield return null;
		Debug.Log("① Loaded");

		// PlayerUI 生成
		string prefabName = PhotonNetwork.IsMasterClient ?
			"PlayerUI/HostPlayerUI" :
			"PlayerUI/GuestPlayerUI";

		var uiObj = PhotonNetwork.Instantiate(prefabName, Vector3.zero, Quaternion.identity);
		//Debug.Break();

		// SetParent を RPC で同期（全クライアントで同じ親にセットされる）
		photonView.RPC(nameof(RPC_SetUIParent), RpcTarget.AllBuffered, uiObj.GetComponent<PhotonView>().ViewID);

		// 2〜3 フレーム待つ（Photon の transform 同期が落ち着く）
		Debug.Log("② 同期前");
		//yield return new WaitUntil(() =>
		//{
		//	Debug.Log($"同期させようとしてるぞ:{photonView.Synchronization}");

		//	//return photonView.Synchronization == ViewSynchronization.ReliableDeltaCompressed;
		//	yield return new WaitForSeconds(3.0f);
		//});
		yield return new WaitForSeconds(3.0f);

		Debug.Log("同期後");
		//yield return null;
		//yield return null;
		//yield return null;

		photonView.RPC(nameof(RegisterUIReady), RpcTarget.AllBuffered);
		Debug.Log("② UIReady 送信");

		// DrawManager 待機
		//yield return new WaitUntil(() => DrawManager.Instance != null);

		//// Host / Guest UI が揃うのを待つ
		//yield return new WaitUntil(() =>
		//	FindChildByPrefix(parentM1, "HostPlayerUI") != null &&
		//	FindChildByPrefix(parentM1, "GuestPlayerUI") != null
		//);
		//
		//
		//Transform hostUI = FindChildByPrefix(parentM1, "HostPlayerUI");
		//Transform guestUI = FindChildByPrefix(parentM1, "GuestPlayerUI");
		//
		//Transform localUI = PhotonNetwork.IsMasterClient ? hostUI : guestUI;
		//Transform remoteUI = PhotonNetwork.IsMasterClient ? guestUI : hostUI;
		//
		//// ここが最重要：DrawManager に UI 親を登録
		//DrawManager.Instance.SetupUIParents(localUI, remoteUI);

		// DrawManager 待ち
		yield return new WaitUntil(() => DrawManager.Instance != null);
		Debug.Log("② DrawManager Ready");

		// Host / Guest UI の Ready 通知を待つ
		//yield return new WaitUntil(() => uiReadyCount >= 2);
		//Debug.Log("③ Host & Guest UI Ready");

		// UI の Transform を取得
		Transform hostUI = FindChildContains("HostPlayerUI");
		Transform guestUI = FindChildContains("GuestPlayerUI");

		Transform localUI = PhotonNetwork.IsMasterClient ? hostUI : guestUI;
		Transform remoteUI = PhotonNetwork.IsMasterClient ? guestUI : hostUI;

		DrawManager.Instance.SetupUIParents(localUI, remoteUI);


		// HandUI 取得を待つ
		yield return new WaitUntil(() =>
			DrawManager.Instance.localHandUI != null &&
			DrawManager.Instance.remoteHandUI != null
		);

		Debug.Log("② HandUI Ready");

		// PlayerManager が揃うまで待つ
		yield return new WaitUntil(() =>
			PlayerManager.LocalPlayer != null &&
			PlayerManager.RemotePlayer != null
		);

		Debug.Log("③ PlayerManager Ready");

		// PlayerManager.Init() はここで呼ぶのが正解
		PlayerManager.LocalPlayer.Init();
		PlayerManager.RemotePlayer.Init();

		yield return new WaitUntil(() =>
			PlayerManager.LocalPlayer.IsInitialized &&
			PlayerManager.RemotePlayer.IsInitialized
		);

		Debug.Log("④ Player Init 完了");

		// HandUI を安全に初期化
		DrawManager.Instance.localHandUI.Initialize(PlayerManager.LocalPlayer, DrawManager.Instance.localHandParent);
		DrawManager.Instance.remoteHandUI.Initialize(PlayerManager.RemotePlayer, DrawManager.Instance.remoteHandParent);

		// PhaseManager / TurnManager を待つ
		yield return new WaitUntil(() =>
			PhaseManager.Instance != null &&
			TurnManager.Instance != null
		);

		Debug.Log("⑤ PhaseManager & TurnManager Ready");

		// ManaUI 初期化
		if (ManaUI.Instance != null)
			ManaUI.Instance.Init();

		// 全部準備完了
		IsReady = true;
		Debug.Log("⑥ IsReady = true");

		// マスターだけターン開始
		if (PhotonNetwork.IsMasterClient)
		{
			Debug.Log("🔥 StartTurn()");
			TurnManager.Instance.StartTurn();
		}
	}

	private Transform FindChildByPrefix(Transform parent, string namePrefix)
	{
		for (int i = 0; i < parent.childCount; i++)
		{
			var c = parent.GetChild(i);
			if (c.name.StartsWith(namePrefix)) return c;
		}
		return null;
	}

	private Transform FindChildContains(string namePart)
	{
		for (int i = 0; i < parentM1.childCount; i++)
		{
			//GameObject.Find(namePart+"")
			var c = parentM1.GetChild(i);
			if (c.name.Contains(namePart))
				return c;
		}
		return null;
	}

	[PunRPC]
	public void RegisterUIReady()
	{
		uiReadyCount++;
		Debug.Log($"UI Ready 通知受信: {uiReadyCount}/2");
	}



	[PunRPC]
	private void RPC_SetUIParent(int viewID)
	{
		PhotonView pv = PhotonView.Find(viewID);
		if (pv == null)
		{
			Debug.LogError($"❌ viewID {viewID} の PhotonView が見つかりません");
			return;
		}

		pv.transform.SetParent(parentM1, false);
		Debug.Log($"📌 SetParent 完了: {pv.name}");
	}

}
