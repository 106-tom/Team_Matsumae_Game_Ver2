using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerManager : MonoBehaviourPunCallbacks
{
	public static PlayerManager LocalPlayer;
	public static PlayerManager RemotePlayer;

	[SerializeField] private int maxLife = 20;
	public int Life { get; private set; }

	public Dictionary<ManaColor, int> ManaPool { get; private set; }

	public List<CardData2> Deck { get; private set; } = new List<CardData2>();
	public List<CardInstance> Hand { get; private set; } = new List<CardInstance>();
	public List<CardData2> Graveyard { get; private set; } = new List<CardData2>();
	public List<CardInstance> Battlefield { get; private set; } = new List<CardInstance>();

	public List<CardInstance> BattleDeck { get; private set; } = new List<CardInstance>();

	public bool IsMyTurn { get; private set; }
	public bool IsInitialized;

	private void Awake()
	{
		if (photonView.IsMine) LocalPlayer = this;
		else RemotePlayer = this;

		Life = maxLife;
		ManaPool = new Dictionary<ManaColor, int>();
		foreach (ManaColor color in System.Enum.GetValues(typeof(ManaColor)))
			ManaPool[color] = 0;
	}

	private IEnumerator Start()
	{
		yield return null; // Awake 後待ち

		if (!IsInitialized)
			Init();

		CreateDeck(); // CardInstance デッキ生成
	}

	public void Init()
	{
		if (IsInitialized) return;

		if (PlayerDeckLoader.Instance == null)
			Debug.LogWarning("PlayerDeckLoader.Instance が null です。Inspector確認");

		RegisterTestCards();

		Hand.Clear();
		Battlefield.Clear();
		Graveyard.Clear();
		Life = maxLife;

		foreach (ManaColor color in System.Enum.GetValues(typeof(ManaColor)))
			ManaPool[color] = 0;

		IsInitialized = true;
		Debug.Log($"{(photonView.IsMine ? "Local" : "Remote")} Player initialized.");
	}

	public void RegisterTestCards()
	{
		Deck.Clear();
		for (int i = 0; i < 5; i++)
		{
			CardData2 card = new CardData2
			{
				cardId = i,
				cardName = $"テストカード{i + 1}",
				cost = 1,
				attack = 1,
				hp = 1,
				description = "テスト用カード"
			};
			Deck.Add(card);
		}
		Debug.Log("[PlayerManager] TestCards Deck に登録完了");
	}

	private void CreateDeck()
	{
		BattleDeck.Clear();
		foreach (var data in Deck)
		{
			BattleDeck.Add(new CardInstance(data, this));
		}
		Debug.Log($"[PlayerManager] BattleDeck 作成完了 枚数: {BattleDeck.Count}");
	}

	public void Damage(int amount)
	{
		if (!photonView.IsMine) return;
		Life = Mathf.Max(Life - amount, 0);
		photonView.RPC(nameof(RPC_SyncLife), RpcTarget.Others, Life);
	}

	public void Heal(int amount)
	{
		if (!photonView.IsMine) return;
		Life += amount;
		photonView.RPC(nameof(RPC_SyncLife), RpcTarget.Others, Life);
	}

	[PunRPC]
	private void RPC_SyncLife(int lifeValue) => Life = lifeValue;

	public void SetTurn(bool myTurn)
	{
		if (!photonView.IsMine) return;
		IsMyTurn = myTurn;
		photonView.RPC(nameof(RPC_SyncTurn), RpcTarget.Others, myTurn);
	}

	[PunRPC]
	private void RPC_SyncTurn(bool myTurn) => IsMyTurn = !myTurn;

	public int GetMana(ManaColor color)
	{
		if (ManaPool == null)
		{
			Debug.LogError("[PlayerManager] ManaPool が null です");
			return 0;
		}

		return ManaPool[color];
	}

	public void SetMana(ManaColor color, int value)
	{
		if (ManaPool == null)
		{
			Debug.LogError("[PlayerManager] ManaPool が null です");
			return;
		}

		ManaPool[color] = Mathf.Max(0, value);
	}

}
