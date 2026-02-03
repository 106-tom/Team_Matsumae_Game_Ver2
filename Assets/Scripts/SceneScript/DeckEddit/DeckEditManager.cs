using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;

public class DeckEditManager : MonoBehaviour
{
    // =================================
    // デッキデータ
    // =================================
    [SerializeField] private int deckMax = 40;
    private int[] deckCardIds;
    [SerializeField] private DeckNameInput nameInput;

    // =================================
    // UI関連
    // =================================
    [Header("Deck UI")]
    [SerializeField] private Transform deckContent;
    [SerializeField] private Transform lastTouchedSlot;

    [Header("Stock UI")]
    [SerializeField] private Transform redStockContent;
    [SerializeField] private Transform blueStockContent;
    [SerializeField] private Transform greenStockContent;
    [SerializeField] private Transform yellowStockContent;
    [SerializeField] private Transform purpleStockContent;

    private Dictionary<CardColor, Transform[]> stockSlotsByColor;
    private Transform[] deckSlots;

    private int lastTouchedCardId = -1;
    private Color unselectedColor = new Color(0.6f, 0.6f, 0.6f, 1f);

    // =================================
    // コストグラフ
    // =================================
    [Header("Cost Graph")]
    [SerializeField] private Transform costGraphRoot;
    [SerializeField] private GameObject barPrefab;
    [SerializeField] private float heightPerCard = 20f;
    private const int MAX_COST = 7;

    // =================================
    // Unity Lifecycle
    // =================================
    private void Awake()
    {
        InitializeDeckData();
        InitializeDeckSlots();
        InitializeStockSlots();
    }

    private void Start()
    {
        DeckDataManager.Instance.RegisterEditScene(nameInput, this);
        DeckDataManager.Instance.LoadSelectedDeck();
        RefreshAll();
    }

    // =================================
    // 初期化
    // =================================
    private void InitializeDeckData()
    {
        deckCardIds = new int[deckMax];
        for (int i = 0; i < deckCardIds.Length; i++)
            deckCardIds[i] = -1;
    }

    private void InitializeDeckSlots()
    {
        deckSlots = new Transform[deckContent.childCount];
        for (int i = 0; i < deckContent.childCount; i++)
            deckSlots[i] = deckContent.GetChild(i);
    }

    private void InitializeStockSlots()
    {
        stockSlotsByColor = new Dictionary<CardColor, Transform[]>
        {
            { CardColor.Red,    GetChildren(redStockContent) },
            { CardColor.Blue,   GetChildren(blueStockContent) },
            { CardColor.Green,  GetChildren(greenStockContent) },
            { CardColor.Yellow, GetChildren(yellowStockContent) },
            { CardColor.Purple, GetChildren(purpleStockContent) }
        };
    }

    private Transform[] GetChildren(Transform parent)
    {
        if (parent == null)
        {
            Debug.LogError("StockContent が Inspector で未設定です");
            return new Transform[0];
        }

        Transform[] children = new Transform[parent.childCount];
        for (int i = 0; i < parent.childCount; i++)
            children[i] = parent.GetChild(i);
        return children;
    }

    // =================================
    // デッキの読み込み
    // =================================
    public void LoadFromData(DeckSaveData data)
    {
        for (int i = 0; i < deckCardIds.Length; i++)
            deckCardIds[i] = -1;

        for (int i = 0; i < data.cardIds.Count && i < deckCardIds.Length; i++)
            deckCardIds[i] = data.cardIds[i];

        SortDeckData();
        RefreshAll();
    }

    // =================================
    // 保存ボタン
    // =================================
    public void OnClickSave()
    {
        SaveDeck();
        ReturnToDeckSelect();
    }

    private void SaveDeck()
    {
        DeckDataManager.Instance.SaveDeckSmart();
        Debug.Log("デッキ保存完了");
    }

    private void ReturnToDeckSelect()
    {
        SceneManager.LoadScene("DeckSelect");
    }

    // =================================
    // デッキUI更新
    // =================================
    public void RefreshAll()
    {
        RefreshDeckUI();
        RefreshCostGraph();
    }

    private void RefreshDeckUI()
    {
        ClearSlots(deckSlots);

        // デッキスロットの更新
        var deckCounts = GetDeckCardCounts();
        int deckSlotIndex = 0;
        foreach (var pair in deckCounts)
        {
            if (deckSlotIndex >= deckSlots.Length) break;
            CreateDeckCard(deckSlots[deckSlotIndex], pair.Key, pair.Value);
            deckSlotIndex++;
        }

        // ストックスロットの更新
        foreach (var pair in stockSlotsByColor)
        {
            CardColor color = pair.Key;
            Transform[] slots = pair.Value;

            ClearSlots(slots);

            for (int i = 0; i < slots.Length; i++)
            {
                int cardId = GetCardIdByColorAndIndex(color, i);
                int deckCount = deckCounts.ContainsKey(cardId) ? deckCounts[cardId] : 0;
                int stockCount = 3 - deckCount;

                if (stockCount <= 0)
                {
                    CreateOutCard(color, cardId, true);
                    continue;
                }

                CreateDeckCard(slots[i], cardId, stockCount);
            }
        }
    }

    private void CreateDeckCard(Transform parent, int cardId, int count)
    {
        GameObject cardObj = DeckManager.instance.CreateCard(cardId, parent);
        cardObj.GetComponent<CardView>().SetCount(count);
    }

    public void CreateOutCard(CardColor color, int cardId, bool limitTrg)
    {
        int slotId = GetCardIdByColorAndcardId(color, cardId);
        Transform slot = stockSlotsByColor[color][slotId];
        Transform deckSlot = deckSlots[GetDeckClotIdBycardI(cardId)];

        var existingCard = slot.GetComponentInChildren<CardController>();
        var existingDeckCard = deckSlot.GetComponentInChildren<CardController>();
        GameObject cardObj = null;

        if (!limitTrg)
        {
            if (existingCard == null)
                cardObj = DeckManager.instance.AnyCreateCard(cardId, slot);
            else if (existingDeckCard == null)
                cardObj = DeckManager.instance.AnyCreateCard(cardId, deckSlot);
            else
                return;
        }
        else
        {
            cardObj = DeckManager.instance.AnyCreateCard(cardId, slot);
        }

        var overlay = cardObj.transform.Find("DarkOverlayImage");
        overlay.gameObject.SetActive(true);
        Debug.Log("1枚生成");
    }

    public void SetDeckCards(int cardId)
    {
        if (GetCardCountInDeck(cardId) >= 3)
        {
            Debug.Log("このカードはデータ上3枚までです");
            return;
        }

        for (int i = 0; i < deckCardIds.Length; i++)
        {
            if (deckCardIds[i] == -1)
            {
                deckCardIds[i] = cardId;
                break;
            }
        }

        SortDeckData();
        RefreshAll();
    }

    public void RemoveDeckCard(int cardId)
    {
        for (int i = 0; i < deckCardIds.Length; i++)
        {
            if (deckCardIds[i] == cardId)
            {
                deckCardIds[i] = -1;
                break;
            }
        }

        RefreshAll();
    }

    // =================================
    // ストックページ
    // =================================
    public void ShowPage(CardColor color)
    {
        HideAllStockPages();

        switch (color)
        {
            case CardColor.Red: redStockContent.gameObject.SetActive(true); break;
            case CardColor.Blue: blueStockContent.gameObject.SetActive(true); break;
            case CardColor.Green: greenStockContent.gameObject.SetActive(true); break;
            case CardColor.Yellow: yellowStockContent.gameObject.SetActive(true); break;
            case CardColor.Purple: purpleStockContent.gameObject.SetActive(true); break;
        }
    }

    private void HideAllStockPages()
    {
        redStockContent.gameObject.SetActive(false);
        blueStockContent.gameObject.SetActive(false);
        greenStockContent.gameObject.SetActive(false);
        yellowStockContent.gameObject.SetActive(false);
        purpleStockContent.gameObject.SetActive(false);
    }

    // =================================
    // ラストタッチカード
    // =================================
    public void SetLastTouchedCard(int cardId)
    {
        lastTouchedCardId = cardId;

        if (lastTouchedSlot.childCount > 0)
            Destroy(lastTouchedSlot.GetChild(0).gameObject);

        GameObject cardObj = DeckManager.instance.AnyCreateCard(cardId, lastTouchedSlot);
        cardObj.GetComponent<CardController>().view.SetCount(1);
    }

    // =================================
    // デッキ計算 / ヘルパー
    // =================================
    private int GetCardCountInDeck(int cardId)
    {
        int count = 0;
        foreach (int id in deckCardIds)
            if (id == cardId) count++;
        return count;
    }

    private void ClearSlots(Transform[] slots)
    {
        foreach (Transform slot in slots)
            if (slot.childCount > 0)
                Destroy(slot.GetChild(0).gameObject);
    }

    private Dictionary<int, int> GetDeckCardCounts()
    {
        var counts = new Dictionary<int, int>();
        foreach (int id in deckCardIds)
        {
            if (id < 0) continue;
            counts[id] = counts.ContainsKey(id) ? counts[id] + 1 : 1;
        }
        return counts;
    }

    private Dictionary<int, int> GetDeckCostCounts()
    {
        var counts = new Dictionary<int, int>();
        for (int i = 0; i <= MAX_COST; i++) counts[i] = 0;

        foreach (int cardId in deckCardIds)
        {
            if (cardId < 0) continue;

            var data = CardDatabase.Instance.GetCardData(cardId);
            if (data == null) continue;

            int cost = Mathf.Min(GetCollerCost(data) + data.anyColorCost, MAX_COST);
            counts[cost]++;
        }

        return counts;
    }

    private int GetCollerCost(CardData data)
    {
        int total = 0;
        foreach (var mana in data.manaCosts)
            total += mana.cost;
        return total;
    }

    private void RefreshCostGraph()
    {
        foreach (Transform child in costGraphRoot)
            Destroy(child.gameObject);

        var costCounts = GetDeckCostCounts();

        for (int cost = 1; cost <= MAX_COST; cost++)
        {
            GameObject barObj = Instantiate(barPrefab, costGraphRoot);

            RectTransform bar = barObj.transform.Find("Bar").GetComponent<RectTransform>();
            int count = Mathf.Min(costCounts[cost], 10);
            bar.sizeDelta = new Vector2(bar.sizeDelta.x, count * heightPerCard);

            barObj.transform.Find("CostText").GetComponent<TMP_Text>().text = cost == MAX_COST ? "7+" : cost.ToString();
            Transform countText = barObj.transform.Find("CountText");
            if (countText != null) countText.GetComponent<TMP_Text>().text = count.ToString();
        }
    }

    private void SortDeckData()
    {
        System.Array.Sort(deckCardIds, (a, b) =>
        {
            if (a == -1 && b == -1) return 0;
            if (a == -1) return 1;
            if (b == -1) return -1;
            return a.CompareTo(b);
        });
    }

    // =================================
    // カードID関連
    // =================================
    private int GetCardIdByColorAndIndex(CardColor color, int index)
    {
        int baseId = color switch
        {
            CardColor.Red => 1,
            CardColor.Blue => 11,
            CardColor.Green => 21,
            CardColor.Yellow => 31,
            CardColor.Purple => 41,
            _ => 0
        };
        return baseId + index;
    }

    private int GetCardIdByColorAndcardId(CardColor color, int cardId)
    {
        int baseId = color switch
        {
            CardColor.Red => 1,
            CardColor.Blue => 11,
            CardColor.Green => 21,
            CardColor.Yellow => 31,
            CardColor.Purple => 41,
            _ => 0
        };
        return cardId - baseId;
    }

    private int GetDeckClotIdBycardI(int cardId)
    {
        int deckSlotIndex = 0;
        foreach (var pair in GetDeckCardCounts())
        {
            if (deckSlotIndex >= deckSlots.Length) break;
            if (pair.Key == cardId) break;
            deckSlotIndex++;
        }
        return deckSlotIndex;
    }

    public List<int> GetDeckCardIds()
    {
        var ids = new List<int>();
        foreach (int card in deckCardIds)
            if (card >= 0) ids.Add(card);
        return ids;
    }
}
