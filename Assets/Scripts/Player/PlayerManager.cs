//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class PlayerManager : MonoBehaviour
//{
//    public static PlayerManager Instance;

//    public List<Player> players = new List<Player>();

//    void Awake()
//    {
//        Instance = this;
//    }

//    public Player this[int index] => players[index];
//}

//[System.Serializable]
//public class Player
//{
//    public int hp = 10;
//    public ManaPool mana = new ManaPool();
//    public List<Card> deck = new List<Card>();     // 山札
//    public List<Card> hand = new List<Card>();     // 手札
//    public List<Card> graveyard = new List<Card>(); // 墓地
//    public List<Card> field = new List<Card>();    // 場に出したカード


//    public void ChargeMana()
//    {
//        // テスト用：各色に1ずつ補充
//        foreach (ManaColor color in System.Enum.GetValues(typeof(ManaColor)))
//        {
//            mana.AddMana(color, 1);
//        }
//        mana.AddGeneric(1);
        
//        Debug.Log("マナ補充完了！");
//    }

//    public Player(List<Card> deck)
//    {
//        this.deck = new List<Card>(deck); // デッキをコピーして使用
//        ShuffleDeck();
//    }

//    // 山札をシャッフル
//    public void ShuffleDeck()
//    {
//        System.Random rng = new System.Random();
//        int n = deck.Count;
//        while (n > 1)
//        {
//            n--;
//            int k = rng.Next(n + 1);
//            Card tmp = deck[k];
//            deck[k] = deck[n];
//            deck[n] = tmp;
//        }
//    }

//    // ドロー
//    public void DrawCard()
//    {
//        if (deck.Count == 0)
//        {
//            Debug.Log("山札切れ！");
//            return;
//        }
//        Card drawn = deck[0];
//        deck.RemoveAt(0);
//        hand.Add(drawn);
//        Debug.Log("ドロー: " + drawn.id);
//    }

//    // 手札から場に出す
//    public void PlayCard(int index)
//    {
//        if (index < 0 || index >= hand.Count) return;
//        Card card = hand[index];

//        if (!mana.CanPay(card))
//        {
//            Debug.Log($"マナ不足: {card.id}");
//            return;
//        }

//        mana.ConsumeMana(card);
//        hand.RemoveAt(index);
//        field.Add(card);

//        Debug.Log($"カードを出した: {card.id}");
//    }

//    // カードを墓地に送る
//    public void SendToGrave(Card card)
//    {
//        if (field.Contains(card))
//        {
//            field.Remove(card);
//            graveyard.Add(card);
//            Debug.Log("墓地送り: " + card.id);
//        }
//    }
//}