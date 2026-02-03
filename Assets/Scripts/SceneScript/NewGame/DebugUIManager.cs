using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class DebugUIManager : MonoBehaviour
{
	// ‚±‚ÌŠÖ”‚ğ UI ƒ{ƒ^ƒ“‚Ì OnClick ‚É“o˜^
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.T))
		{
			if (PhotonNetwork.InRoom)
				PhotonNetwork.LeaveRoom();

			SceneManager.LoadScene("Title");
		}
	}

}
