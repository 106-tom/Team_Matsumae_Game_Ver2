using UnityEngine;
using UnityEngine.UI;

// --- 固有色の種類を定義する「列挙型」 ---
// どのスクリプトからでも使えるように、classの外に定義するのがオススメ
public enum FkingColorType
{
    None, // 固有色なし
    Red,
    Green,
    Blue,
    Yellow,
    Purple,
    Rainbow // ▼▼▼ 修正点 (追加) ▼▼▼
}

[CreateAssetMenu(fileName = "FkingCard", menuName = "Fking/Fking Card Data")]
public class FkingCardData : ScriptableObject
{
    [Header("カード基本情報")]
    public string cardID;
    public string cardName;
    public Sprite cardImage; // 通常イラスト
    public Sprite keyCardImage; // バトル用イラスト (KeyCardIllust)
    public bool hasKeyCardIllust; // バトル用イラストを持つか
    public string cardType; // "モンスター", "呪文"

    [Header("コスト（グレー）")]
    public int genericCost; // グレーマナ('g')の数

    [Header("コスト（固有色）")]
    public FkingColorType specificColor; // 固有色の種類
    public int specificCost; // 固有色の数

    [Header("テキスト情報")]
    [TextArea(3, 5)]
    public string effectText;
    public string tags;

    [Header("ステータス（モンスターのみ）")]
    public int ap;
    public int bp;

    [Header("召喚後")]
    public bool hasPostSummonEffect; // 召喚後効果があるか
    public bool isWeariness = false;
}