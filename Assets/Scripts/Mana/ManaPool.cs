using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//プレイヤーが現在持っているマナの量を管理。


public class ManaPool
{
    public Dictionary<ManaColor, int> coloredMana = new Dictionary<ManaColor, int>();

    public ManaPool()
    {
        foreach (ManaColor color in System.Enum.GetValues(typeof(ManaColor)))
        {
            coloredMana[color] = 0;
        }
    }

    public void AddMana(ManaColor color, int amount)
    {
        coloredMana[color] += amount;
    }

	//カードコストを支払えるか判定
	public bool CanPay(Card card)
    {
        foreach (var cost in card.ManaCost)
        {
            if (coloredMana[cost.Key] < cost.Value)
                return false;
        }

        // 全マナ総量から色指定分を引いた残りで、任意コストをまかなえるか
        int totalMana = 0;
        foreach (var kv in coloredMana)
            totalMana += kv.Value;

        int usedForSpecific = 0;
        foreach (var kv in card.ManaCost)
            usedForSpecific += kv.Value;

        int remaining = totalMana - usedForSpecific;
        return remaining >= card.anyColorCost;
    }

    /// 実際に支払い処理を行う
    public bool ConsumeMana(Card card)
    {
        if (!CanPay(card))
            return false; // 支払い不可能なら終了

        // まず指定色のマナを支払う
        foreach (var kv in card.ManaCost)
        {
            coloredMana[kv.Key] -= kv.Value;
        }

        // 任意マナを残りから支払う
        int remainingCost = card.anyColorCost;

        foreach (var color in coloredMana.Keys)
        {
            if (remainingCost <= 0) break;

            int available = coloredMana[color];
            int use = Mathf.Min(available, remainingCost);
            coloredMana[color] -= use;
            remainingCost -= use;
        }

		return true;
    }
}
