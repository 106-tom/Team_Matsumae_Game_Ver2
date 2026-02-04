using UnityEngine;
using System.Collections.Generic;


public enum CardColor
{
    Red, Blue, Yellow, Purple, Green, White,Colorless
}
[System.Serializable]
public class CardTriggerEffects
{
    [Header("カードを出した瞬間に発動")]
    public List<EffectEntry> onSummon = new List<EffectEntry>();

    [Header("フィールド上で常時発動")]
    public List<EffectEntry> onField = new List<EffectEntry>();

    [Header("攻撃した瞬間に発動")]
    public List<EffectEntry> onAttack = new List<EffectEntry>();

    [Header("破壊されたときに発動")]
    public List<EffectEntry> onDestroy = new List<EffectEntry>();
}

/// <summary>
/// 発動する効果の種類
/// </summary>
public enum CardEffectType//Jsonに書く際はここからコピペ
{
    None,
    //カード効果
    DrawCard,       //カードをドロー
    DestoryAnyMonster,        //相手モンスターを選択破壊
    DestoryAllMonster,        //相手モンスターを全破壊

    //カード特性
    Dragon,         //自分ターンの間BPが1.2倍
    Hide,           //攻撃対象にならない
    Instant,        //防御中に割り込むスペル用
    Resurrection,   //破壊されたときもう一度同じカードをResurrectionを消して召喚
    AddAP,          //APが加算される

    //特定のカード専用
    Princess,       //このカードが存在する限り場のカードに「AddAP」のタグが付与される
}
[System.Serializable]
public class EffectEntry
{
    public CardEffectType effectType;
    public int timeValue; //（回数）
    public int mathValue; //（数値）

}

[System.Serializable]
public class CardData
{
    public int cardID;
    public string cardName;
    public Sprite cardImage;

    public string imagePath;
    public string color;
    [TextArea(3, 5)]
    public string CardText;

    [Header("能力値")]
    public int ap;
    public int bp;
    [Header("コスト情報")]
    public List<ManaCostEntry> manaCosts = new List<ManaCostEntry>(); // 色ごとのコスト配分
    public int anyColorCost = 0; // 無色コスト（どの色でもOK）
    [Header("属性やタグ")]

    public CardEffectType effectType;

    [Header("発動タグ分類")]
    public CardTriggerEffects triggerEffects = new CardTriggerEffects();

    [Header("所持者")]
    public int owner;

    // --- 便利メソッド ---
    public List<EffectEntry> GetEffectsOnSummon() => triggerEffects.onSummon;
    public List<EffectEntry> GetEffectsOnDestroy() => triggerEffects.onDestroy;
	[Header("カテゴリー")]
	public string category;
}