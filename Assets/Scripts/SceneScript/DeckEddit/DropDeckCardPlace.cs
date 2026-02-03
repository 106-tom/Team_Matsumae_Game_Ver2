using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class DropDeckCardPlace : MonoBehaviour, IDropHandler
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

        if (card.movement != null) // もしカードがあれば、
        {
            DeckEditManager deckEditManager = GameObject.Find("DeckEditManager").GetComponent<DeckEditManager>();
            card.movement.droppedSuccessfully = true;
            deckEditManager.SetDeckCards(card.model.cardId);
        }
    }
}