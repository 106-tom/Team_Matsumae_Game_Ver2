// ===================== DrawManager.cs (修正版) =====================
using Photon.Pun;
using UnityEngine;
using System.Collections;

public class DrawManager : MonoBehaviourPun
{
	public static DrawManager Instance;

	[Header("カードUIプレハブ")]
	public GameObject cardUIPrefab;

	[Header("ローカル")]
	public Transform localHandParent;
	public Transform localDeckParent;
	public DeckCounterUI localDeckCounter;
	public HandUI localHandUI;

	[Header("リモート")]
	public Transform remoteHandParent;
	public Transform remoteDeckParent;
	public DeckCounterUI remoteDeckCounter;
	public HandUI remoteHandUI;

	private void Awake()
	{
		Instance = this;
	}

	/// <summary>
	/// UI の親 Transform をセットする。
	/// localUI, remoteUI の両方を渡してください。remoteUI が null の場合はシーン内から探す。
	/// </summary>
	public void SetupUIParents(Transform localUI, Transform remoteUI)
	{
		// --- local ---
		localHandParent = localUI.Find("PlayerHandPanel");
		localDeckParent = localUI.Find("PlayerDeckPanel");

		if (localHandParent == null)
			Debug.LogError("[DrawManager] localHandParent が見つかりません：PlayerHandPanel");
		if (localDeckParent == null)
			Debug.LogError("[DrawManager] localDeckParent が見つかりません：PlayerDeckPanel");

		localHandUI = localHandParent != null ? localHandParent.GetComponent<HandUI>() : null;
		localDeckCounter = localDeckParent != null ? localDeckParent.GetComponentInChildren<DeckCounterUI>() : null;

		// --- remote ---
		remoteHandParent = remoteUI.Find("PlayerHandPanel");
		remoteDeckParent = remoteUI.Find("PlayerDeckPanel");

		if (remoteHandParent == null)
			Debug.LogError("[DrawManager] remoteHandParent が見つかりません：PlayerHandPanel");
		if (remoteDeckParent == null)
			Debug.LogError("[DrawManager] remoteDeckParent が見つかりません：PlayerDeckPanel");

		remoteHandUI = remoteHandParent != null ? remoteHandParent.GetComponent<HandUI>() : null;
		remoteDeckCounter = remoteDeckParent != null ? remoteDeckParent.GetComponentInChildren<DeckCounterUI>() : null;

		Debug.Log("[DrawManager] SetupUIParents 完了（Initialize はここでは呼ばない）");
	}


	public IEnumerator DrawCardRoutine(int count)
	{
		yield return StartCoroutine(DrawCardProcess(count));
	}

	public IEnumerator DrawCardProcess(int count = 1)
	{
		// PlayerManager.LocalPlayer と Deck が揃うまで待つ
		yield return new WaitUntil(() => PlayerManager.LocalPlayer != null && PlayerManager.LocalPlayer.Deck != null);

		// localHandUI が初期化されるまで待つ
		yield return new WaitUntil(() => localHandUI != null && localHandUI.IsInitialized);

		for (int i = 0; i < count; i++)
		{
			if (PlayerManager.LocalPlayer.Deck.Count == 0)
			{
				Debug.Log("[DrawManager] Deck が空です。仮カードで補充します。");
				var fakeCard = new CardData2 { cardId = 999, cardName = "FakeCard", cost = 1, attack = 1, hp = 1 };
				PlayerManager.LocalPlayer.Deck.Add(fakeCard);
			}

			var cardData = PlayerManager.LocalPlayer.Deck[0];
			PlayerManager.LocalPlayer.Deck.RemoveAt(0);

			var instance = new CardInstance(cardData, PlayerManager.LocalPlayer);
			PlayerManager.LocalPlayer.Hand.Add(instance);

			localHandUI.CreateCardUI(instance);
			RefreshDeckCount();

			photonView.RPC(nameof(RPC_SyncDrawCard), RpcTarget.Others, cardData.cardId, PlayerManager.LocalPlayer.photonView.ViewID);

			yield return null;
		}
	}

	private IEnumerator RotateCardAnimation(GameObject card)
	{
		float duration = 0.5f;
		float elapsed = 0f;
		while (elapsed < duration)
		{
			card.transform.Rotate(Vector3.up, 360 * Time.deltaTime / duration);
			elapsed += Time.deltaTime;
			yield return null;
		}
		card.transform.rotation = Quaternion.identity;
	}

	[PunRPC]
	private void RPC_SyncDrawCard(int cardId, int playerViewID)
	{
		var player = PhotonView.Find(playerViewID)?.GetComponent<PlayerManager>();
		if (player == null) return;

		var cardData = CardDatabase2.GetCardById(cardId);
		if (cardData == null)
		{
			Debug.LogWarning($"[DrawManager] ID {cardId} のカードが見つかりません。仮カードを生成します。");
			cardData = new CardData2 { cardId = cardId, cardName = "FakeCard" + cardId, cost = 1, attack = 1, hp = 1 };
		}

		var instance = new CardInstance(cardData, player);
		player.Hand.Add(instance);

		HandUI targetUI = (player.photonView.IsMine) ? localHandUI : remoteHandUI;
		if (targetUI != null)
			targetUI.CreateCardUI(instance);
		else
			Debug.LogWarning("[DrawManager] RPC_SyncDrawCard: targetUI が null です");

		RefreshDeckCount();
	}

	private void RefreshDeckCount()
	{
		int localCount = 0;
		int remoteCount = 0;

		if (PlayerManager.LocalPlayer != null && PlayerManager.LocalPlayer.Deck != null)
			localCount = PlayerManager.LocalPlayer.Deck.Count;

		if (PlayerManager.RemotePlayer != null && PlayerManager.RemotePlayer.Deck != null)
			remoteCount = PlayerManager.RemotePlayer.Deck.Count;

		if (localDeckCounter != null)
			localDeckCounter.UpdateCount(localCount);

		if (remoteDeckCounter != null)
			remoteDeckCounter.UpdateCount(remoteCount);
	}
}
