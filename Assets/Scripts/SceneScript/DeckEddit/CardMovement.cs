using UnityEngine;
using UnityEngine.EventSystems;

public class CardMovement : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IPointerClickHandler
{
    DropDeckCardPlace[] dropDeckPanels;
    DropCollectCardPlace[] dropCollectionPanels;
    public bool droppedSuccessfully = false;

    void Awake()
    {
        dropDeckPanels = FindObjectsOfType<DropDeckCardPlace>();
        dropCollectionPanels = FindObjectsOfType<DropCollectCardPlace>();
    }
    // ===== クリック =====
    public void OnPointerClick(PointerEventData eventData)
    {
        CardController card = GetComponent<CardController>();
        if (card == null) return;

        DeckEditManager manager =
            FindObjectOfType<DeckEditManager>();

        manager.SetLastTouchedCard(card.model.cardId);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        foreach (var panel in dropDeckPanels)
        {
            panel.EnableDrop(true);
        }
        
        foreach (var panel in dropCollectionPanels)
        {
            panel.EnableDrop(true);
        }


        Transform canvas = GameObject.Find("Canvas").GetComponent<Transform>();
        transform.SetParent(canvas, false);
        GetComponent<CanvasGroup>().blocksRaycasts = false;
        DeckEditManager manager = FindObjectOfType<DeckEditManager>();
        CardController card = GetComponent<CardController>();

        manager.SetLastTouchedCard(card.model.cardId);

        DeckEditManager deckEditManager = GameObject.Find("DeckEditManager").GetComponent<DeckEditManager>();
        deckEditManager.CreateOutCard(card.model.color, card.model.cardId,false);

    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
        Transform target = transform.Find("costBG");
        if (target != null)
        {
            target.gameObject.SetActive(false);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        foreach (var panel in dropDeckPanels)
        {
            panel.EnableDrop(false);
        }
        foreach (var panel in dropCollectionPanels)
        {
            panel.EnableDrop(false);
        }

        GetComponent<CanvasGroup>().blocksRaycasts = true;

        CardController card = GetComponent<CardController>();
        DeckEditManager manager = FindObjectOfType<DeckEditManager>();

        if (!droppedSuccessfully)
        {
            // ★ どこにもドロップされなかった
            Debug.Log("無効な場所にドロップ");

            // UIを正しい状態に戻す
            manager.RefreshAll();
        }
        Destroy(gameObject);
    }
    public void OnDrop(PointerEventData eventData)
    {
        CardController card =
            eventData.pointerDrag.GetComponent<CardController>();

        if (card == null) return;

        card.movement.droppedSuccessfully = true;

        DeckEditManager manager = FindObjectOfType<DeckEditManager>();
        manager.SetDeckCards(card.model.cardId);

        Destroy(card.gameObject);
    }

}
