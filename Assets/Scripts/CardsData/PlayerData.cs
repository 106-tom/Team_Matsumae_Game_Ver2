using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "CardGame/PlayerData")]
public class PlayerData : ScriptableObject
{
    [Header("基本情報")]
    public int playerID;

    [Header("初期マナプール")]
    public ManaPool baseMana = new ManaPool();

    [Header("山札")]
    public List<Card> deck = new List<Card>();

    [Header("初期手札")]
    public List<Card> initialHand = new List<Card>();

    [Header("初期フィールド")]
    public List<Card> initialField = new List<Card>();
}
