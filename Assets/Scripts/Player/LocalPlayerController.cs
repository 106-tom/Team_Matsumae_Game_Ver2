using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class LocalPlayerController : MonoBehaviour
{
    public static LocalPlayerController Instance;

    [Header("プレイヤー情報")]
    public InGamePlayer localPlayer;// このコントローラが操作するプレイヤーのID

    public Transform handPanel;    // 謇区惆陦ｨ遉ｺ逕ｨ繝代ロ繝ｫ
    public Transform fieldPanel;   // 繝輔ぅ繝ｼ繝ｫ繝芽｡ｨ遉ｺ逕ｨ繝代ロ繝ｫ
    public GameObject cardPrefab;  // 繧ｫ繝ｼ繝芽｡ｨ遉ｺ逕ｨPrefab

    private Camera mainCamera;
    Card selectedCard = null;

    // 繝槭ロ繝ｼ繧ｸ繝｣繝ｼ蜿門ｾ励・繝倥Ν繝代・繝｡繧ｽ繝・ラ・・urnPhaseManager繧貞━蜈茨ｼ・
    private object GetGameManager()
    {
        if (TurnPhaseManager.Instance != null)
            return TurnPhaseManager.Instance;
        return GameManager.Instance;
    }

    private bool IsPlayerTurn(int playerId)
    {
        if (TurnPhaseManager.Instance != null)
            return TurnPhaseManager.Instance.IsPlayerTurn(playerId);
        if (GameManager.Instance != null)
            return GameManager.Instance.IsPlayerTurn(playerId);
        return false;
    }

    private TurnPhase GetCurrentPhase()
    {
        if (TurnPhaseManager.Instance != null)
            return TurnPhaseManager.Instance.CurrentPhaseEnum;
        if (GameManager.Instance != null)
            return GameManager.Instance.CurrentPhase;
        return TurnPhase.Summon;
    }

	void Start()
	{
		mainCamera = Camera.main;

		// --- Photon 縺ｮ諠・ｱ縺九ｉ InGamePlayer 繧剃ｽ懈・ ---
		Photon.Realtime.Player localPhotonPlayer = PhotonNetwork.LocalPlayer;
		string playerName = localPhotonPlayer.CustomProperties.ContainsKey("PlayerName")
			? (string)localPhotonPlayer.CustomProperties["PlayerName"]
			: "Player" + localPhotonPlayer.ActorNumber;

		int deckID = localPhotonPlayer.CustomProperties.ContainsKey("DeckID")
			? (int)localPhotonPlayer.CustomProperties["DeckID"]
			: 0;

		localPlayer = new InGamePlayer(localPhotonPlayer.ActorNumber, playerName, deckID);

		// 蛻晄悄繝槭リ・医ユ繧ｹ繝育畑・・
		localPlayer.mana.AddMana(ManaColor.Red, 3);
		localPlayer.mana.AddMana(ManaColor.Blue, 2);
		localPlayer.mana.AddMana(ManaColor.Green, 1);

		// 蛻晄悄謇区惆・医ユ繧ｹ繝育畑・・
		Card dragon = new Card();
		dragon.Initialize(1, localPlayer.PlayerID, 2, 4, 5);
		dragon.AddManaCost(ManaColor.Red, 2);
		localPlayer.hand.Add(dragon);

		// UI譖ｴ譁ｰ
		UpdateHandUI();
	}

	void Update()
    {
		if (!IsPlayerTurn(localPlayer.PlayerID))
			return; // 閾ｪ蛻・・繧ｿ繝ｼ繝ｳ莉･螟悶・謫堺ｽ應ｸ榊庄

		if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
			return;

		if (Input.GetMouseButtonDown(0))
		{
			GameObject clickedObj = GetClickedObject();
			if (clickedObj != null)
				OnObjectClicked(clickedObj);
		}
	}

    /// <summary>
    // Raycast縺ｧ繧ｯ繝ｪ繝・け蟇ｾ雎｡繧貞叙蠕・
    /// </summary>
    private GameObject GetClickedObject()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.collider.gameObject;
        }
        return null;
    }

    /// <summary>
    /// 繧ｯ繝ｪ繝・け縺輔ｌ縺溘が繝悶ず繧ｧ繧ｯ繝医↓蠢懊§縺溷・逅・
    /// </summary>
    private void OnObjectClicked(GameObject obj)
    {
        // --- 繧ｿ繝ｼ繝ｳ邨ゆｺ・・繝ｼ繧ｫ繝ｼ縺ｪ繧・---
        TurnEndMarker marker = obj.GetComponent<TurnEndMarker>();
        if (marker != null)
        {
            // TurnPhaseManager繧貞━蜈育噪縺ｫ菴ｿ逕ｨ
            if (TurnPhaseManager.Instance != null)
            {
                TurnPhaseManager.Instance.SendNextPhaseEvent();
            }
            else if (GameManager.Instance != null)
            {
                GameManager.Instance.EndPhase();
            }
            return;
        }

		// --- 繧ｫ繝ｼ繝峨↑繧・---
		CardView cardView = obj.GetComponent<CardView>();
        if (cardView == null) return;

        Card card = cardView.CardData;
        var phase = GetCurrentPhase();

		// --- 閾ｪ蛻・・繧ｫ繝ｼ繝峨・蝣ｴ蜷・---
		if (card.OwnerId == localPlayer.PlayerID)
		{
            if (phase == TurnPhase.Summon)
            {
                // 繝｡繧､繝ｳ繝輔ぉ繝ｼ繧ｺ縺ｧ縺ｯ繝｢繝ｳ繧ｹ繧ｿ繝ｼ蜿ｬ蝟壹→蜻ｪ譁・ｽｿ逕ｨ縺悟庄閭ｽ
                if (TurnPhaseManager.Instance != null)
                {
                    // TurnPhaseManager縺ｮUseCard繝｡繧ｽ繝・ラ繧剃ｽｿ逕ｨ
                    if (localPlayer.hand.Contains(card))
                    {
                        bool success = TurnPhaseManager.Instance.UseCard(localPlayer.PlayerID, card);
                        if (success)
                        {
                            UpdateHandUI();
                            UpdateFieldUI();
                        }
                    }
                }
                else if (GameManager.Instance != null)
                {
                    // 蠕梧婿莠呈鋤諤ｧ・哦ameManager繧剃ｽｿ逕ｨ
                    if (localPlayer.hand.Contains(card))
                    {
                        TrySummonCard(card);
                    }
                }
            }
            else if (phase == TurnPhase.Attack)
            {
                // 繧｢繧ｿ繝・け繝輔ぉ繝ｼ繧ｺ・夊・蛻・・繝｢繝ｳ繧ｹ繧ｿ繝ｼ縺ｧ謾ｻ謦・
                if (localPlayer.field.Contains(card) && card.bp > 0)
                {
                    // TurnPhaseManager繧貞━蜈育噪縺ｫ菴ｿ逕ｨ
                    if (TurnPhaseManager.Instance != null)
                    {
                        TurnPhaseManager.Instance.SelectAttacker(localPlayer.PlayerID, card);
                    }
                    else if (GameManager.Instance != null)
                    {
                        // 蠕梧婿莠呈鋤諤ｧ・哦ameManager繧剃ｽｿ逕ｨ
                        SelectCard(card);
                    }
                }
            }
        }
        //--- 謨ｵ縺ｮ繧ｫ繝ｼ繝峨・蝣ｴ蜷・---
        else
        {
            SelectTarget(card);
            CardEffectManager.ActivateEffects(selectedCard.Data, "onAttack");
            // 繧｢繧ｿ繝・け繝輔ぉ繝ｼ繧ｺ・壽判謦・ｯｾ雎｡繧帝∈謚・
            if (phase == TurnPhase.Attack)
            {
                if (TurnPhaseManager.Instance != null)
                {
                    // 繝｢繝ｳ繧ｹ繧ｿ繝ｼ縺ｸ縺ｮ謾ｻ謦・
                    if (card.bp > 0)
                    {
                        TurnPhaseManager.Instance.SelectAttackTarget(localPlayer.PlayerID, targetMonster: card);
                    }
                }
                else if (GameManager.Instance != null)
                {
                    // 蠕梧婿莠呈鋤諤ｧ・哦ameManager繧剃ｽｿ逕ｨ
                    SelectTarget(card);
                }
            }
            else
            {
                // 縺昴・莉悶・繝輔ぉ繝ｼ繧ｺ縺ｧ縺ｮ蜃ｦ逅・
                SelectTarget(card);
            }
        }
    }

    /// <summary>
    /// カード選択
    /// </summary>

    private void SelectCard(Card card)
    {
        selectedCard = card;
        Debug.Log($"繧ｫ繝ｼ繝・{card.id} 繧帝∈謚槭＠縺ｾ縺励◆");

        // TurnPhaseManager繧貞━蜈育噪縺ｫ菴ｿ逕ｨ
        if (TurnPhaseManager.Instance != null)
        {
            TurnPhaseManager.Instance.SelectCard(localPlayer.PlayerID, card);
        }
        else if (GameManager.Instance != null)
        {
            GameManager.Instance.SelectCard(localPlayer.PlayerID, card);
        }
    }


    private void SelectTarget(Card target)
    {
		if (selectedCard == null)
		{
			Debug.Log("蜈医↓繧ｫ繝ｼ繝峨ｒ驕ｸ謚槭＠縺ｦ縺上□縺輔＞");
			return;
		}

		GameManager.Instance.SelectTarget(localPlayer.PlayerID, target); // ← PlayerID に変更
		Debug.Log($"ターゲット {target.id} を選択");
    
		if (TurnPhaseManager.Instance != null)
		{
			TurnPhaseManager.Instance.SelectTarget(localPlayer.PlayerID, target);
		}
		else if (GameManager.Instance != null)
		{
			GameManager.Instance.SelectTarget(localPlayer.PlayerID, target);
		}
	}

    /// <summary>
    /// 謇区惆縺九ｉ繧ｫ繝ｼ繝峨ｒ蜿ｬ蝟・
    /// </summary>
    private void TrySummonCard(Card card)
    {
		if (localPlayer.PlayCard(card))
		{
			Debug.Log($"プレイヤー{localPlayer.PlayerID}が {card.id} を召喚しました"); // ← PlayerID に変更
			GameManager.Instance.OnCardSummoned(localPlayer, card);
            UpdateHandUI();

			Debug.Log($"繝励Ξ繧､繝､繝ｼ{localPlayer.PlayerID}縺・{card.id} 繧貞小蝟壹＠縺ｾ縺励◆");
			// TurnPhaseManager繧貞━蜈育噪縺ｫ菴ｿ逕ｨ
			if (TurnPhaseManager.Instance != null)
			{
				TurnPhaseManager.Instance.OnCardSummoned(localPlayer, card);
			}
			else if (GameManager.Instance != null)
			{
				GameManager.Instance.OnCardSummoned(localPlayer, card);
			}
			UpdateHandUI();
			UpdateFieldUI();
		}
		else
		{
		}
	}

    /// <summary>
    // 謇区惆UI繧呈峩譁ｰ
    /// </summary>
    public void UpdateHandUI()
    {
        // 譌｢蟄倥・謇区惆UI繧貞炎髯､
        foreach (Transform child in handPanel)
        {
            Destroy(child.gameObject);
        }

        // 迴ｾ蝨ｨ縺ｮ謇区惆縺九ｉCardView繧剃ｽ懈・
        foreach (Card card in localPlayer.hand)
        {
            GameObject cardObj = Instantiate(cardPrefab, handPanel);
            cardObj.name = $"Card_{card.id}";
            CardView view = cardObj.GetComponent<CardView>();
            view.Setup(card);
        }
    }

    // --- 繝輔ぅ繝ｼ繝ｫ繝蔚I譖ｴ譁ｰ ---
    public void UpdateFieldUI()
    {
        foreach (Transform child in fieldPanel)
            Destroy(child.gameObject);

        foreach (Card card in localPlayer.field)
        {
            GameObject cardObj = Instantiate(cardPrefab, fieldPanel);
            CardView view = cardObj.GetComponent<CardView>();
            view.Setup(card);
        }
    }
}
