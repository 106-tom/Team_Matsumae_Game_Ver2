using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ManaCostEntry
{
    public ManaColor color;
    public int cost;
}


[System.Serializable]
public class Card
{
    [Header("基本情報")]
    public int id;
    public int ap;
    public int bp;

    [Header("コスト情報")]
    public List<ManaCostEntry> manaCostList = new List<ManaCostEntry>();// 各色ごとのマナコスト
    public int anyColorCost = 0;    // 無色マナ（どの色でもOK）

    [Header("所有者情報")]
    public int OwnerId;    // どのプレイヤーがこのカードを所有しているか

    [Header("状態管理")]
    public bool IsSelected = false;
	public bool isTapped; // タップ状態を保持

	public bool IsDead => bp <= 0;

    public CardData Data; // ← 静的データへの参照

    // --- 内部処理用の辞書 ---
    private Dictionary<ManaColor, int> manaCost = new Dictionary<ManaColor, int>();
    public IReadOnlyDictionary<ManaColor, int> ManaCost => manaCost;


    public void Initialize(int id, int ownerId, int anyColorCost, int ap, int bp)
    {
        this.id = id;
        this.OwnerId = ownerId;
        this.anyColorCost = anyColorCost;
        this.ap = ap;
        this.bp = bp;
        Data = CardDatabase.Instance.GetCardData(id);

        manaCost.Clear();
        foreach (var entry in manaCostList)
        {
            if (!manaCost.ContainsKey(entry.color))
                manaCost.Add(entry.color, entry.cost);
        }

        // ネット同期用に登録
        CardRegistry.RegisterCard(this);
    }


    // 色マナ追加
    public void AddManaCost(ManaColor color, int amount)
    {
        if (manaCost.ContainsKey(color))
            manaCost[color] = amount;
        else
            manaCost.Add(color, amount);

        // リストにも反映（エディタ上で確認しやすく）
        var index = manaCostList.FindIndex(m => m.color == color);
        if (index >= 0)
            manaCostList[index] = new ManaCostEntry { color = color, cost = amount };
        else
            manaCostList.Add(new ManaCostEntry { color = color, cost = amount });
    }

    // 任意のマナで支払えるコスト
    public void SetAnyColorCost(int amount)
    {
        anyColorCost = amount;
    }

    // 合計コスト
    public int TotalCost()
    {
        int sum = anyColorCost;
        foreach (var kv in manaCost) sum += kv.Value;
        return sum;
    }

    // ------------------------------
    // ダメージ処理
    // ------------------------------
    public void TakeDamage(int amount)
    {
        bp -= amount;
        if (bp <= 0)
        {
            bp = 0;
            OnDeath();
        }
    }

    // ------------------------------
    // 死亡時
    // ------------------------------
    protected virtual void OnDeath()
    {
        Debug.Log($"カード {id} が破壊された");
        // ゲームマネージャー経由で墓地送り処理などを行う
        //InGamePlayer.ReferenceEquals.Instance?.se(this);
        //GameManager.Instance.
    }
}