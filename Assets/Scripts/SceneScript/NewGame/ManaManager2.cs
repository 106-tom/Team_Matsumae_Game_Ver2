// ===================== ManaManager2.cs =====================
using Photon.Pun;
using UnityEngine;

public class ManaManager2 : MonoBehaviourPun
{
	public static ManaManager2 Instance;

	public ManaUI localPlayerManaUI;
	public ManaUI remotePlayerManaUI;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		AssignManaUI();
	}

	private void AssignManaUI()
	{
		ManaUI[] uis = FindObjectsOfType<ManaUI>();

		foreach (var ui in uis)
		{
			Transform parent = ui.transform.parent;
			if (parent == null) continue;

			if (parent.name == "HostPlayerUI")
			{
				if (PhotonNetwork.IsMasterClient)
					localPlayerManaUI = ui;
				else
					remotePlayerManaUI = ui;
			}
			else if (parent.name == "GuestPlayerUI")
			{
				if (PhotonNetwork.IsMasterClient)
					remotePlayerManaUI = ui;
				else
					localPlayerManaUI = ui;
			}
		}

		Debug.Log($"[ManaManager] LocalUI={localPlayerManaUI?.name}, RemoteUI={remotePlayerManaUI?.name}");
	}

	public void AddMana(ManaColor color, int amount = 1)
	{
		var player = PlayerManager.LocalPlayer;
		if (player == null || !player.IsMyTurn) return;

		int newAmount = player.GetMana(color) + amount;
		player.SetMana(color, newAmount);

		localPlayerManaUI?.AddManaToUI(color);
		photonView.RPC(nameof(RPC_SyncManaColor), RpcTarget.Others, (int)color, newAmount);
	}

	public bool UseMana(ManaColor color, int cost = 1)
	{
		var player = PlayerManager.LocalPlayer;
		if (player == null || !player.IsMyTurn) return false;

		int current = player.GetMana(color);
		if (current < cost) return false;

		int newAmount = current - cost;
		player.SetMana(color, newAmount);

		localPlayerManaUI?.UpdateUI(player.ManaPool);
		photonView.RPC(nameof(RPC_SyncManaColor), RpcTarget.Others, (int)color, newAmount);

		return true;
	}

	[PunRPC]
	private void RPC_SyncManaColor(int colorInt, int amount)
	{
		ManaColor color = (ManaColor)colorInt;
		PlayerManager.RemotePlayer.SetMana(color, amount);
		remotePlayerManaUI?.UpdateUI(PlayerManager.RemotePlayer.ManaPool);
	}
}
