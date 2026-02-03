using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DeckManager : MonoBehaviour
{
    [SerializeField] CardController cardPrefab; // カードプレハブ 
    [SerializeField] CardController AnycardPrefab; //　デッキの外用プレハブ 
    [SerializeField] Color selectedColor = Color.white;
    [SerializeField] Color unselectedColor = new Color(0.6f, 0.6f, 0.6f, 1f);
    // シングルトン化
    public static DeckManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    // カードを生成するメソッド（Public化した）
    public GameObject CreateCard(int cardId, Transform trans)
    {
        CardController card = Instantiate(cardPrefab, trans);
        card.Init(cardId);
        return card.gameObject;
    }
    //スロットの外用
    public GameObject AnyCreateCard(int cardId, Transform trans)
    {
        CardController card = Instantiate(AnycardPrefab, trans);
        card.Init(cardId);
        return card.gameObject;
    }
}