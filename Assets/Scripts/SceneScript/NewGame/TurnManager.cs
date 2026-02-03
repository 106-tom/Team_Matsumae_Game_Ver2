using UnityEngine;
using Photon.Pun;
using System.Collections;

public class TurnManager : MonoBehaviourPun
{
	public static TurnManager Instance;

	private void Awake()
	{
		Instance = this;
	}

	public void StartTurn()
	{
		StartCoroutine(StartTurnRoutine());
	}

	private IEnumerator StartTurnRoutine()
	{
		// LocalPlayer ‚ª‘¶Ý‚·‚é‚Ü‚Å‘Ò‚Â
		yield return new WaitUntil(() => PlayerManager.LocalPlayer != null);

		// GameStartInitializer ‚ªŠ®‘S‚É€”õŠ®—¹‚·‚é‚Ü‚Å‘Ò‚Â
		yield return new WaitUntil(() => GameStartInitializer.IsReady == true);

		if (!PhotonNetwork.IsMasterClient)
			yield break;

		Debug.Log("Turn Start: " + PhotonNetwork.LocalPlayer.NickName);

		PlayerManager.LocalPlayer.SetTurn(true);

		photonView.RPC(nameof(RPC_SetTurn), RpcTarget.Others, false);

		PhaseManager.Instance.StartPhase();
	}

	public void EndTurn()
	{
		if (!PlayerManager.LocalPlayer.IsMyTurn) return;

		Debug.Log("Turn End: " + PhotonNetwork.LocalPlayer.NickName);
		PlayerManager.LocalPlayer.SetTurn(false);
	}

	[PunRPC]
	private void RPC_SetTurn(bool myTurn)
	{
		PlayerManager.RemotePlayer.SetTurn(myTurn);
	}
}
