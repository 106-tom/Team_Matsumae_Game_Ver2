using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class RoomReadyManager : MonoBehaviourPunCallbacks
{
	[Header("背景画像（ルーム画面の一枚絵）")]
	public RectTransform roomBackground;

	[Header("準備完了チェックマーク画像")]
	public GameObject readyCheckMarkPrefab;
	private GameObject myCheckMark;
	private GameObject otherCheckMark;

	[Header("クリック判定エリア（正規化 0〜1）")]
	public Rect readyButtonArea = new Rect(0.8f, 0.1f, 0.15f, 0.1f);

	[Header("チェックマーク配置（正規化 0〜1）")]
	public Rect hostCheckMarkArea = new Rect(0.1f, 0.8f, 0.05f, 0.05f);
	public Rect guestCheckMarkArea = new Rect(0.85f, 0.8f, 0.05f, 0.05f);

	[Header("UI")]
	[SerializeField] private TMP_Text roomIDText;

	private bool isReady = false;

	void Start()
	{
		// 自分のチェックマーク生成
		myCheckMark = Instantiate(readyCheckMarkPrefab, roomBackground);
		myCheckMark.SetActive(false);
		myCheckMark.transform.localPosition = GetCheckMarkPosition(PhotonNetwork.LocalPlayer.IsMasterClient);

		// 相手のチェックマーク生成（自分と逆位置）
		otherCheckMark = Instantiate(readyCheckMarkPrefab, roomBackground);
		otherCheckMark.SetActive(false);
		otherCheckMark.transform.localPosition = GetCheckMarkPosition(!PhotonNetwork.LocalPlayer.IsMasterClient);

		// 初期プロパティにReady=falseをセット
		Hashtable props = new Hashtable { ["Ready"] = false };
		PhotonNetwork.LocalPlayer.SetCustomProperties(props);

		UpdateRoomID();
	}

	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			Vector2 localPoint;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				roomBackground, Input.mousePosition, null, out localPoint);

			Vector2 normalized = RectPointToNormalized(roomBackground, localPoint);

			if (readyButtonArea.Contains(normalized))
			{
				ToggleReady();
			}
		}
	}

	private void ToggleReady()
	{
		if (!PhotonNetwork.IsConnected)
		{
			Debug.LogError("Photon未接続のため Ready を送信できません！");
			return;
		}

		isReady = !isReady;
		myCheckMark.SetActive(isReady);

		// プロパティ更新
		Hashtable props = new Hashtable { ["Ready"] = isReady };
		PhotonNetwork.LocalPlayer.SetCustomProperties(props);

		// 全員の準備チェック
		CheckAllReady();
	}

	// ✅ 他プレイヤーのプロパティ更新を受け取る
	public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
	{
		if (changedProps.ContainsKey("Ready"))
		{
			bool ready = (bool)changedProps["Ready"];

			if (targetPlayer.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
			{
				otherCheckMark.SetActive(ready);
			}

			CheckAllReady();
		}
	}

	private Vector3 GetCheckMarkPosition(bool isHost)
	{
		Rect area = isHost ? hostCheckMarkArea : guestCheckMarkArea;
		float x = (area.x + area.width / 2 - 0.5f) * roomBackground.rect.width;
		float y = (area.y + area.height / 2 - 0.5f) * roomBackground.rect.height;
		return new Vector3(x, y, 0f);
	}

	Vector2 RectPointToNormalized(RectTransform rect, Vector2 localPoint)
	{
		float x = (localPoint.x / rect.rect.width) + 0.5f;
		float y = (localPoint.y / rect.rect.height) + 0.5f;
		return new Vector2(x, y);
	}

	private void CheckAllReady()
	{
		foreach (Player p in PhotonNetwork.PlayerList)
		{
			if (!p.CustomProperties.ContainsKey("Ready") || !(bool)p.CustomProperties["Ready"])
				return;
		}

		Debug.Log("✅ 全員準備完了！ゲーム開始！");

		if (PhotonNetwork.IsMasterClient)
		{
			PhotonNetwork.LoadLevel("Game");
		}
	}

	public override void OnJoinedRoom()
	{
		Debug.Log("OnJoinedRoom called");
		// ルームに入ったタイミングでも更新
		UpdateRoomID();
	}

	private void UpdateRoomID()
	{
		if (PhotonNetwork.CurrentRoom != null)
		{
			roomIDText.text = "Room ID: " + PhotonNetwork.CurrentRoom.Name;
		}
		else
		{
			roomIDText.text = "Room ID: ----";
		}
	}



	// ───────────── Gizmos ─────────────

	void OnDrawGizmos()
	{
		if (roomBackground == null) return;

		DrawAreaGizmo(hostCheckMarkArea, Color.green);
		DrawAreaGizmo(guestCheckMarkArea, Color.yellow);
		DrawAreaGizmo(readyButtonArea, Color.cyan);
	}

	void DrawAreaGizmo(Rect area, Color color)
	{
		Rect rect = roomBackground.rect;
		float width = rect.width * area.width * roomBackground.lossyScale.x;
		float height = rect.height * area.height * roomBackground.lossyScale.y;

		Vector3 pos = roomBackground.position + new Vector3(
			(area.x - 0.5f + area.width / 2) * rect.width * roomBackground.lossyScale.x,
			(area.y - 0.5f + area.height / 2) * rect.height * roomBackground.lossyScale.y,
			0
		);

		Gizmos.color = new Color(color.r, color.g, color.b, 0.4f);
		Gizmos.DrawCube(pos, new Vector3(width, height, 0));
	}
}
