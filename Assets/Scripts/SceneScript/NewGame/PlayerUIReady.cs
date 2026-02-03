using Photon.Pun;
using UnityEngine;

public class PlayerUIReady : MonoBehaviourPun
{
	private void Start()
	{
		// Ž©•ª‚Ì PlayerUI ‚¾‚¯‚ª’Ê’m‚ð‘—‚é
		if (photonView.IsMine)
		{
			PhotonView roomPV = GameObject.Find("GameStartInitializer").GetComponent<PhotonView>();
			roomPV.RPC(nameof(GameStartInitializer.RegisterUIReady), RpcTarget.AllBuffered);
		}
	}
}
