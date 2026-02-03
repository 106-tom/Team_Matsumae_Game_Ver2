using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropCollectCardPlace : MonoBehaviour, IDropHandler
{
	Image image;

	void Awake()
	{
		image = GetComponent<Image>();
		image.raycastTarget = false; // 初期は無効
	}
	public void EnableDrop(bool enable)
	{
		image.raycastTarget = enable;
	}

	public void OnDrop(PointerEventData eventData) // ドロップされた時に行う処理
	{
		CardController card = eventData.pointerDrag.GetComponent<CardController>(); // ドラッグしてきた情報からCardMovementを取得

		if (card.movement == null) return;

		DeckEditManager deckEditManager =
			FindObjectOfType<DeckEditManager>();

		Debug.Log("一枚減らしたい");
		// デッキから1枚減らす
		card.movement.droppedSuccessfully = true;
		deckEditManager.RemoveDeckCard(card.model.cardId);
	}
}