using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ManaUseManager : MonoBehaviourPun
{
	//[System.Serializable]

	[SerializeField] private ManaUseManager manaUseManager;


	public class Mana
	{
		//public ManaColor color;
		public string color;
		public bool isRest; // false=アクティブ, true=レスト(使用済み)
	}



	public enum ManaColor
	{
		Red, Blue, Green, Yellow, Purple
	}

	public List<Mana> manaList = new List<Mana>();
	public const int MAX_MANA = 10;

	public void AddMana(ManaColor color)
	{
		if (manaList.Count < MAX_MANA)
		{
			manaList.Add(new Mana { color = color.ToString(), isRest = false });
		}
		else
		{
			manaList.RemoveAt(0);
			manaList.Add(new Mana { color = color.ToString(), isRest = false });
		}

		SyncMana();
	}

	public bool UseMana(ManaColor color)
	{
		foreach (var mana in manaList)
		{
			if (mana.color == color.ToString() && !mana.isRest)
			{
				mana.isRest = true;
				SyncMana();
				return true;
			}
		}
		return false;
	}

	public void RefreshMana()
	{
		foreach (var mana in manaList)
		{
			mana.isRest = false;
		}
		SyncMana();
	}

	void SyncMana()
	{
		int[] colors;
		bool[] rests;
		ConvertToSyncData(out colors, out rests);
		photonView.RPC(nameof(RPC_SyncMana), RpcTarget.All, colors, rests);
	}



	[PunRPC]
	void RPC_SyncMana(int[] colors, bool[] rests)
	{
		manaList.Clear();
		for (int i = 0; i < colors.Length; i++)
		{
			manaList.Add(new Mana
			{
				color = ((ManaColor)colors[i]).ToString(),
				isRest = rests[i]
			});
		}

		// UI更新をここで呼べる
		// ManaUI.Instance.UpdateManaUI(manaList);
	}

	void ConvertToSyncData(out int[] colors, out bool[] rests)
	{
		colors = new int[manaList.Count];
		rests = new bool[manaList.Count];

		for (int i = 0; i < manaList.Count; i++)
		{
			// string → enum → int に変換
			colors[i] = (int)System.Enum.Parse(typeof(ManaColor), manaList[i].color);
			rests[i] = manaList[i].isRest;
		}
	}

	//  指定されたマナコストを全て払えるか？
	public bool CanPayCost(int anyColorCost, List<(ManaColor color, int cost)> colorCosts)
	{
		foreach (var cost in colorCosts)
		{
			int available = manaList.Count(m => m.color == cost.color.ToString() && !m.isRest);
			if (available < cost.cost)
				return false;
		}

		int unusedMana = manaList.Count(m => !m.isRest);
		int required = anyColorCost + colorCosts.Sum(c => c.cost);
		return unusedMana >= required;
	}

	//  実際にマナを支払う（消費）
	public bool PayCost(int anyColorCost, List<(ManaColor color, int cost)> colorCosts)
	{
		if (!CanPayCost(anyColorCost, colorCosts))
			return false;

		foreach (var cost in colorCosts)
		{
			for (int i = 0; i < cost.cost; i++)
			{
				var mana = manaList.First(m => m.color == cost.color.ToString() && !m.isRest);
				mana.isRest = true;
			}
		}

		int remain = anyColorCost;
		foreach (var mana in manaList.Where(m => !m.isRest))
		{
			if (remain <= 0) break;
			mana.isRest = true;
			remain--;
		}

		SyncMana();
		return true;
	}

	public bool PayCost(int anyColorCost, List<(string color, int cost)> colorCosts)
	{
		// 色付きマナのチェック
		foreach (var cost in colorCosts)
		{
			int available = manaList.Count(m => m.color == cost.color && !m.isRest);
			if (available < cost.cost)
				return false;
		}

		// 無色マナのチェック
		int unusedMana = manaList.Count(m => !m.isRest);
		int required = anyColorCost + colorCosts.Sum(c => c.cost);
		if (unusedMana < required)
			return false;

		// 色マナ消費
		foreach (var cost in colorCosts)
		{
			for (int i = 0; i < cost.cost; i++)
			{
				var mana = manaList.First(m => m.color == cost.color && !m.isRest);
				mana.isRest = true;
			}
		}

		// 無色マナ消費
		int remain = anyColorCost;
		foreach (var mana in manaList.Where(m => !m.isRest))
		{
			if (remain <= 0) break;
			mana.isRest = true;
			remain--;
		}

		SyncMana(); // 変化を同期
		return true;
	}

}
