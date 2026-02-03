//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;
//using System.Collections.Generic;
//using UnityEngine.SceneManagement;

//public class DeckSelectManager : MonoBehaviour
//{
//    private int selectedSlot = -1;

//    [Header("シーン名")]
//    public string editSceneName = "DeckEditScene";

//    [Header("UI参照")]
//    public GameObject previewPanel;
//    public Transform previewContentPanel;
//    public TextMeshProUGUI[] deckNameTexts;
//    public GameObject cardPrefab;

//    private List<CardData> allCardsDatabase = new List<CardData>();

//    void Start()
//    {
//        previewPanel.SetActive(false);
//        InitializeCardDatabase();
//        UpdateAllDeckDisplays();
//    }

//    public void UpdateAllDeckDisplays()
//    {
//        for (int i = 0; i < deckNameTexts.Length; i++)
//        {
//            int slotNumber = i + 1;
//            string saveKey = "DeckJsonData_" + slotNumber;
//            string jsonString = PlayerPrefs.GetString(saveKey, "");

//            if (!string.IsNullOrEmpty(jsonString))
//            {
//                SavedDeckData loadedData = JsonUtility.FromJson<SavedDeckData>(jsonString);
//                deckNameTexts[i].text = loadedData.deckName;
//            }
//            else
//            {
//                deckNameTexts[i].text = "空のスロット";
//            }
//        }
//    }

//    public void SelectDeck(int slotIndex)
//    {
//        selectedSlot = slotIndex;
//    }

//    public void OnEditButtonClicked()
//    {
//        if (selectedSlot == -1) return;
//        DeckGameData.currentDeckSlot = selectedSlot;
//        SceneManager.LoadScene(editSceneName);
//    }

//    public void OnPreviewButtonClicked()
//    {
//        if (selectedSlot == -1) return;

//        foreach (Transform child in previewContentPanel) Destroy(child.gameObject);

//        string saveKey = "DeckJsonData_" + selectedSlot;
//        string jsonString = PlayerPrefs.GetString(saveKey, "");

//        if (!string.IsNullOrEmpty(jsonString))
//        {
//            SavedDeckData loadedData = JsonUtility.FromJson<SavedDeckData>(jsonString);
//            for (int i = 0; i < loadedData.cardIDs.Count; i++)
//            {
//                int cardID = loadedData.cardIDs[i];
//                int count = loadedData.cardCounts[i];
//                CardData cardData = allCardsDatabase.Find(c => c.cardID == cardID);
//                if (cardData != null)
//                {
//                    GameObject cardObject = Instantiate(cardPrefab, previewContentPanel);
//                    cardObject.GetComponent<Button>().enabled = false;
//                    CardUI cardUI = cardObject.GetComponent<CardUI>();
//                    cardUI.Initialize(cardData, null, CardUI.CardLocation.Collection);
//                    cardUI.UpdateCountDisplay(count);
//                }
//            }
//        }

//        previewPanel.SetActive(true);
//    }

//    public void ClosePreview()
//    {
//        previewPanel.SetActive(false);
//    }

//    private void InitializeCardDatabase()
//    {
//        allCardsDatabase = new List<CardData>() {
//            new CardData { cardID = 1, cardName = "足軽", cost = 1, color = CardColor.Red, ap = 1, bp = 100, text = "" },
//            new CardData { cardID = 2, cardName = "侍", cost = 2, color = CardColor.Red, ap = 2, bp = 200, text = "" },
//            new CardData { cardID = 3, cardName = "ビックリ・バトル・ボール ドラゴン改", cost = 3, color = CardColor.Red, ap = 2, bp = 200, text = "BP500以下のモンスター1体を破壊する" },
//            new CardData { cardID = 4, cardName = "ナヨ竹しき「昔を扱えし少女」", cost = 3, color = CardColor.Red, ap = 3, bp = 100, text = "1枚ドローする" },
//            new CardData { cardID = 5, cardName = "龍のご隠居", cost = 4, color = CardColor.Red, ap = 5, bp = 500, text = "" },
//            new CardData { cardID = 6, cardName = "捕らわれた巫女", cost = 5, color = CardColor.Red, ap = 3, bp = 500, text = "相手のモンスター1体を破壊する" },
//            new CardData { cardID = 7, cardName = "女王の建築（男）＝降臨", cost = 5, color = CardColor.Red, ap = 4, bp = 200, text = "BP300以下のモンスターすべてを破壊する" },
//            new CardData { cardID = 8, cardName = "ドラゴン族の咆哮", cost = 6, color = CardColor.Red, ap = 4, bp = 500, text = "このモンスターがいる間、自分の赤モンスターのAPを+1する" },
//            new CardData { cardID = 9, cardName = "ご隠居の使ってる様子", cost = 2, color = CardColor.Red, ap = 0, bp = 0, text = "BP300以下のモンスター2体を破壊する" },
//            new CardData { cardID = 10, cardName = "OBがブレス", cost = 5, color = CardColor.Red, ap = 0, bp = 0, text = "BP500以下のモンスターすべてを破壊する" }
//        };
//    }
//}