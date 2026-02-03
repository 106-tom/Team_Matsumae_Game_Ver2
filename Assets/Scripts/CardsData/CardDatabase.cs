using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class CardDatabase : MonoBehaviour
{
    public static CardDatabase Instance { get; private set; }

    private Dictionary<int, CardData> cardDataDict = new Dictionary<int, CardData>();

    private void Awake()
    {
        // シングルトン化
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadAllCardData();
    }

    /// <summary>
    /// JSONファイルから全カードデータを読み込む
    /// </summary>
    private void LoadAllCardData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Cards/card_data");
        if (jsonFile == null)
        {
            Debug.LogError("カードデータのJSONファイルが見つかりません: Resources/Cards/card_data.json");
            return;
        }

        // JSON → CardData配列に変換
        CardData[] cardArray = JsonHelper.FromJson<CardData>(jsonFile.text);

        cardDataDict.Clear();
        foreach (var data in cardArray)
        {
            if (!cardDataDict.ContainsKey(data.cardID))
                cardDataDict.Add(data.cardID, data);
        }

        Debug.Log($"カードデータを {cardDataDict.Count} 枚ロードしました。");
    }

    /// <summary>
    /// カードIDからCardDataを取得
    /// </summary>
    public CardData GetCardData(int id)
    {
        if (cardDataDict.TryGetValue(id, out var data))
            return data;
        Debug.LogWarning($"CardID {id} のデータが見つかりません");
        return null;
    }

    /// <summary>
    /// CardDataからゲーム内カードを生成
    /// </summary>
    public Card CreateCardInstance(int id, int ownerId)
    {
        var data = GetCardData(id);
        if (data == null)
            return null;

        return CardFactory.CreateFromData(data, ownerId);
    }
}
