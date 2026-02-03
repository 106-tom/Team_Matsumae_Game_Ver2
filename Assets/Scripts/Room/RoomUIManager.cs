using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomUIManager : MonoBehaviourPunCallbacks
{
	[Header("UI参照")]
	[SerializeField] private TMP_Text roomIDText;

	[Header("背景画像（チェックマーク用）")]
	[SerializeField] private RectTransform roomBackground;

	[Header("準備完了チェックマーク画像")]
	[SerializeField] private GameObject readyCheckMarkPrefab;

	[Header("クリック判定エリア（正規化 0〜1）")]
	[SerializeField] private Rect readyButtonArea = new Rect(0.8f, 0.1f, 0.15f, 0.1f);

	[Header("チェックマーク配置（正規化 0〜1）")]
	[SerializeField] private Rect hostCheckMarkArea = new Rect(0.1f, 0.8f, 0.05f, 0.05f);
	[SerializeField] private Rect guestCheckMarkArea = new Rect(0.85f, 0.8f, 0.05f, 0.05f);

	[Header("ルームID表示位置（正規化 0〜1）")]
	[SerializeField] private Rect roomIDDisplayArea = new Rect(0.5f, 0.95f, 0.1f, 0.05f);


	private bool isReady = false;
	private GameObject myCheckMark;
	private GameObject otherCheckMark;

	private void Start()
	{
		Debug.Log("PhotonNetwork.InRoom = " + PhotonNetwork.InRoom);
		Debug.Log("CurrentRoom = " + (PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.Name : "NULL"));

		// RoomID表示
		UpdateRoomID();

		// チェックマーク生成
		myCheckMark = Instantiate(readyCheckMarkPrefab, roomBackground);
		myCheckMark.SetActive(false);
		myCheckMark.transform.localPosition = GetCheckMarkPosition(PhotonNetwork.LocalPlayer.IsMasterClient);

		otherCheckMark = Instantiate(readyCheckMarkPrefab, roomBackground);
		otherCheckMark.SetActive(false);
		otherCheckMark.transform.localPosition = GetCheckMarkPosition(!PhotonNetwork.LocalPlayer.IsMasterClient);

		// 初期プロパティ
		Hashtable props = new Hashtable { ["Ready"] = false };
		PhotonNetwork.LocalPlayer.SetCustomProperties(props);

		
	}

	private void Update()
	{
		// 画像クリックでReady
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

	private void OnReadyClicked()
	{
		ToggleReady();
	}

	private void ToggleReady()
	{
		isReady = !isReady;
		myCheckMark.SetActive(isReady);

		Hashtable props = new Hashtable { ["Ready"] = isReady };
		PhotonNetwork.LocalPlayer.SetCustomProperties(props);

		CheckAllReady();
	}

	private void OnStartClicked()
	{
		if (!PhotonNetwork.IsMasterClient) return;

		if (AllPlayersReady())
		{
			Debug.Log("全員準備完了 → ゲーム開始！");
			PhotonNetwork.LoadLevel("Game2");
		}
	}

	private void UpdateRoomID()
	{
		if (roomIDText == null) return;

		roomIDText.text = PhotonNetwork.CurrentRoom != null ?
			"" + PhotonNetwork.CurrentRoom.Name : "----";

		UpdateRoomIDPosition();
	}

	private void UpdateRoomIDPosition()
	{
		if (roomIDText == null || roomBackground == null) return;

		Rect rect = roomBackground.rect;

		// Gizmoと同じ位置に合わせる
		float x = (roomIDDisplayArea.x - 0.5f) * rect.width;
		float y = (roomIDDisplayArea.y - 0.5f) * rect.height;

		roomIDText.rectTransform.localPosition = new Vector3(x, y, 0f);
	}



	private bool AllPlayersReady()
	{
		if (PhotonNetwork.PlayerList.Length < 2) return false;
		foreach (var p in PhotonNetwork.PlayerList)
		{
			if (!p.CustomProperties.ContainsKey("Ready") || !(bool)p.CustomProperties["Ready"])
				return false;
		}
		return true;
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
			PhotonNetwork.LoadLevel("Game2");
		}
	}

	

	private bool GetReadyState(Player player)
	{
		if (player.CustomProperties.TryGetValue("Ready", out object value))
			return (bool)value;
		return false;
	}

	private Vector3 GetCheckMarkPosition(bool isHost)
	{
		Rect area = isHost ? hostCheckMarkArea : guestCheckMarkArea;
		Rect rect = roomBackground.rect;

		Vector2 pivotOffset = new Vector2(
			(0.5f - roomBackground.pivot.x) * rect.width,
			(0.5f - roomBackground.pivot.y) * rect.height
		);

		float x = (area.x + area.width / 2 - 0.5f) * rect.width + pivotOffset.x;
		float y = (area.y + area.height / 2 - 0.5f) * rect.height + pivotOffset.y;

		return new Vector3(x, y, 0f);
	}



	private Vector2 RectPointToNormalized(RectTransform rect, Vector2 localPoint)
	{
		float x = (localPoint.x / rect.rect.width) + 0.5f;
		float y = (localPoint.y / rect.rect.height) + 0.5f;
		return new Vector2(x, y);
	}

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

	private void OnDrawGizmos()
	{
		if (roomBackground == null) return;

		DrawRectGizmo(readyButtonArea, Color.green);
		DrawRectGizmo(hostCheckMarkArea, Color.blue);
		DrawRectGizmo(guestCheckMarkArea, Color.red);

		Rect rect = roomBackground.rect;
		float width = rect.width * roomIDDisplayArea.width * roomBackground.lossyScale.x;
		float height = rect.height * roomIDDisplayArea.height * roomBackground.lossyScale.y;
		Vector3 pos = roomBackground.position + new Vector3(
			(roomIDDisplayArea.x - 0.5f) * rect.width * roomBackground.lossyScale.x,
			(roomIDDisplayArea.y - 0.5f) * rect.height * roomBackground.lossyScale.y,
			0
		);

		Gizmos.color = new Color(1f, 1f, 0f, 0.4f); // 黄色半透明
		Gizmos.DrawCube(pos, new Vector3(width, height, 0));
	}

	private void DrawRectGizmo(Rect area, Color color)
	{
		Rect rect = roomBackground.rect;

		// pivot を考慮した offset
		Vector2 pivotOffset = new Vector2(
			(0.5f - roomBackground.pivot.x) * rect.width,
			(0.5f - roomBackground.pivot.y) * rect.height
		);

		Vector3 pos = roomBackground.position + new Vector3(
			(area.x + area.width / 2 - 0.5f) * rect.width + pivotOffset.x,
			(area.y + area.height / 2 - 0.5f) * rect.height + pivotOffset.y,
			0
		);

		Vector3 size = new Vector3(area.width * rect.width, area.height * rect.height, 0);

		Gizmos.color = new Color(color.r, color.g, color.b, 0.4f);
		Gizmos.DrawCube(pos, size);
	}

}
