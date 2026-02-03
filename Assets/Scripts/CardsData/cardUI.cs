//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;

//public class CardUI : MonoBehaviour
//{
//    // カードの「居場所」を定義するためのカテゴリ
//    public enum CardLocation
//    {
//        Collection, // 所持カード一覧
//        Deck        // デッキ内容
//    }

//    private CardData assignedCardData;
//    private DeckEditorManager editorManager;
//    private CardLocation location; // このカードの居場所を覚えておく変数

//    // --- UIパーツへの参照 ---
//    public Image cardImage;
//    public TextMeshProUGUI cardNameText;
//    public TextMeshProUGUI costText;
//    public TextMeshProUGUI effectText;
//    public TextMeshProUGUI apText;
//    public TextMeshProUGUI bpText;
//    [Header("枚数カウンター")]
//    public GameObject counterObject;
//    public TextMeshProUGUI countText;

//    // Initializeメソッドに「自分の居場所」を教える引数を追加
//    public void Initialize(CardData cardData, DeckEditorManager manager, CardLocation loc)
//    {
//        assignedCardData = cardData;
//        editorManager = manager;
//        location = loc; // 教えてもらった居場所を覚えておく

//        // UIの見た目を更新
//        cardNameText.text = cardData.cardName;
//        costText.text = cardData.cost.ToString();
//        effectText.text = cardData.text;
//        apText.text = "AP: " + cardData.ap.ToString();
//        bpText.text = "BP: " + cardData.bp.ToString();

//        GetComponent<Button>().onClick.AddListener(OnCardClicked);
//    }

//    // 枚数表示を更新する
//    public void UpdateCountDisplay(int count)
//    {
//        if (count > 1)
//        {
//            counterObject.SetActive(true);
//            countText.text = count.ToString();
//        }
//        else
//        {
//            counterObject.SetActive(false);
//        }
//    }

//    // カードがクリックされた時の処理
//    private void OnCardClicked()
//    {
//        // ★★★ ここが重要 ★★★
//        // もし自分の居場所が「コレクション」なら、カードを追加
//        if (location == CardLocation.Collection)
//        {
//            editorManager.AddCardToDeck(assignedCardData);
//        }
//        // もし自分の居場所が「デッキ」なら、カードを削除
//        else
//        {
//            editorManager.RemoveCardFromDeck(assignedCardData);
//        }
//    }
//}