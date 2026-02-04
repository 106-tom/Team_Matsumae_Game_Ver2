using System.Collections.Generic;
using UnityEngine;

public class PlayerManaManagerAI : MonoBehaviour
{
	public Transform manaZoneParent;

	// 色ごとのPrefab
	public GameObject redManaPrefab;
	public GameObject blueManaPrefab;
	public GameObject greenManaPrefab;
	public GameObject yellowManaPrefab;
	public GameObject purpleManaPrefab;

	public bool isEnemy; // インスペクタで設定


	private const int MAX_MANA = 10;
	private float offsetX = 60f;

	// 実データ
	private List<ManaUnitAI> manaList = new List<ManaUnitAI>();

	// UI（Image or Button）
	private List<ManaIconUIAI> manaUIList = new List<ManaIconUIAI>();


	// -----------------------------
	//  マナ追加
	// -----------------------------
	public void AddMana(string color)
	{
		GameObject prefab = GetManaPrefab(color);
		if (prefab == null)
		{
			Debug.LogError($"[Mana] Prefab not found: {color}");
			return;
		}

		// 10個以上なら古いのから削除
		if (manaList.Count >= MAX_MANA)
		{
			manaList.RemoveAt(0);
			ManaIconUIAI ui = manaUIList[0];
			manaUIList.RemoveAt(0);
			Destroy(ui.gameObject);
		}

		// UI生成
		GameObject icon = Instantiate(prefab, manaZoneParent);
		icon.transform.localPosition = Vector3.zero;
		icon.transform.localRotation = Quaternion.identity;
		icon.transform.localScale = Vector3.one;

		// ManaUnit の追加
		ManaUnitAI unit = new ManaUnitAI(color, false);
		manaList.Add(unit);

		// UI コンポーネント取得
		ManaIconUIAI uiComp = icon.GetComponent<ManaIconUIAI>();
		if (uiComp == null)
		{
			Debug.LogError("Prefab に ManaUIAI が付いていない！");
			return;
		}
		manaUIList.Add(uiComp);

		uiComp.SetTapped(false); // 最初はアンタップ

		SortAndAlign();

		//Debug.Log($"{color} Mana Added");
	}


	// -----------------------------
	//  並び替え
	// -----------------------------
	private void SortAndAlign()
	{
		int count = manaList.Count;
		float center = (count - 1) * 0.5f;

		float yOffset = isEnemy ? -50f : 50f;

		for (int i = 0; i < count; i++)
		{
			RectTransform rt = manaUIList[i].GetComponent<RectTransform>();
			float x = (i - center) * offsetX;

			// ① 位置
			rt.anchoredPosition = new Vector2(x, yOffset);

			// ② 回転（Enemyなら X,Y を 180°）
			if (isEnemy)
			{
				rt.localRotation = Quaternion.Euler(180f, 180f, 0f);
			}
			else
			{
				rt.localRotation = Quaternion.identity;
			}
		}
	}




	// -----------------------------
	//  Prefab取得
	// -----------------------------
	private GameObject GetManaPrefab(string color)
	{
		return color switch
		{
			"Red" => redManaPrefab,
			"Blue" => blueManaPrefab,
			"Green" => greenManaPrefab,
			"Yellow" => yellowManaPrefab,
			"Purple" => purpleManaPrefab,
			_ => null
		};
	}


	// -----------------------------
	//  使用可能チェック
	// -----------------------------
	public bool CanPayCost(CardAI card)
	{
		int needColor = card.costColor;
		int needAny = card.costAny;

		// 色マナ（未タップ）の数を確認
		int activeColor = CountActiveMana(card.colorType);
		if (activeColor < needColor) return false;

		// 全マナの未タップ数
		int activeTotal = CountActiveTotal();
		return activeTotal >= needColor + needAny;
	}


	// -----------------------------
	//  マナ支払い（タップ方式）
	// -----------------------------
	public void PayCost(CardAI card)
	{
		SpendColorMana(card.colorType, card.costColor);
		SpendAnyMana(card.costAny);
	}

	private int CountActiveMana(string color)
	{
		int c = 0;
		foreach (var m in manaList)
			if (!m.isTapped && m.color == color)
				c++;
		return c;
	}

	private int CountActiveTotal()
	{
		int c = 0;
		foreach (var m in manaList)
			if (!m.isTapped)
				c++;
		return c;
	}


	// 色から支払う（タップ）
	private void SpendColorMana(string color, int cost)
	{
		int used = 0;
		for (int i = 0; i < manaList.Count && used < cost; i++)
		{
			if (!manaList[i].isTapped && manaList[i].color == color)
			{
				manaList[i].isTapped = true;
				manaUIList[i].SetTapped(true);
				used++;
			}
		}
	}

	// 無色（未タップから順に）
	private void SpendAnyMana(int cost)
	{
		int used = 0;
		for (int i = 0; i < manaList.Count && used < cost; i++)
		{
			if (!manaList[i].isTapped)
			{
				manaList[i].isTapped = true;
				manaUIList[i].SetTapped(true);
				used++;
			}
		}
	}


	// -----------------------------
	//  Untap（Startフェーズで呼ぶ）
	// -----------------------------
	public void UntapAll()
	{
		for (int i = 0; i < manaList.Count; i++)
		{
			manaList[i].isTapped = false;
			manaUIList[i].SetTapped(false);
		}
	}

	// -----------------------------
	// 指定色の未タップマナが指定数以上あるか
	// -----------------------------
	public bool HasColor(ManaColor color, int required)
	{
		int count = 0;
		string colorStr = color.ToString(); // ManaUnitAI の color は string
		foreach (var m in manaList)
		{
			if (!m.isTapped && m.color == colorStr)
				count++;
		}
		return count >= required;
	}

	// -----------------------------
	// 全色未タップマナが指定数以上揃っているか
	// -----------------------------
	public bool HasAllColors(int requiredPerColor)
	{
		return HasColor(ManaColor.Red, requiredPerColor)
			&& HasColor(ManaColor.Blue, requiredPerColor)
			&& HasColor(ManaColor.Green, requiredPerColor)
			&& HasColor(ManaColor.Yellow, requiredPerColor)
			&& HasColor(ManaColor.Purple, requiredPerColor);
	}



	// -----------------------------
	//  デバッグ用
	// -----------------------------
	public int TotalManaCount() => manaList.Count;
}
