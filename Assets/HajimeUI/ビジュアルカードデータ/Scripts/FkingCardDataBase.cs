using UnityEngine;
using System.Collections.Generic; // Listを使うために必要
using System.Linq; // GetCardByID のために必要

[CreateAssetMenu(fileName = "FkingCardDatabase", menuName = "Fking/Fking Card Database")]
public class FkingCardDatabase : ScriptableObject
{
    [Tooltip("データベースに登録するカードの全リスト")]
    public List<FkingCardData> AllCardsList; // あなたの既存のリスト

    // ▼▼▼ この関数を追加してください ▼▼▼
    /// <summary>
    /// 文字列のIDから、リスト内のFkingCardDataを検索して返す
    /// </summary>
    public FkingCardData GetCardByID(string cardID)
    {
        // AllCardsList の中から、
        // cardData の cardID が 引数の cardID と一致する(==)
        // 最初の要素(FirstOrDefault)を探して、それを返す
        return AllCardsList.FirstOrDefault(cardData => cardData.cardID == cardID);
    }
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
}