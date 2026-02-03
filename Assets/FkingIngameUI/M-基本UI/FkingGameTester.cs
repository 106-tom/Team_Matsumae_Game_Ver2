using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UIの表示/非表示だけをテストするための外部スクリプト。
/// </summary>
public class FkingGameTester : MonoBehaviour
{
    [Header("制御するUIマネージャー")]
    public FkingUITest uiManager;

    private bool isPlayerTurn = true;
    private bool isInBattle = false;

    // Enumを8フェーズに拡張
    public enum GamePhase
    {
        None,
        PlayerMana,
        PlayerSummon,
        PlayerAttack,
        PlayerBlock,
        EnemyMana,
        EnemySummon,
        EnemyAttack,
        EnemyBlock,
        End // ターン終了処理中
    }
    private GamePhase currentPhase = GamePhase.None;

    void Start()
    {
        if (uiManager == null)
        {
            Debug.LogError("FkingGameTesterに uiManager (FkingUITest) が設定されていません！");
            return;
        }

        uiManager.ShowBattleButton(true);
        uiManager.ShowEndTurnButton(false); // ★ 修正 
        uiManager.SetPlayerTurnActive(true);
        isPlayerTurn = true;
        currentPhase = GamePhase.None;

        uiManager.HideAllPhaseUI();
        isInBattle = false;
    }

    /// <summary>
    /// 【BATTLE START ボタン用】
    /// </summary>
    public void OnBattleStartPressed()
    {
        Debug.Log("--- (TEST) バトル開始 ---");
        isInBattle = true;

        uiManager.ShowBattleButton(false);
        // uiManager.ShowEndTurnButton(true); // SetPhaseUIVisibility が制御するので不要
        uiManager.SetEndTurnButtonText("End Turn");
        isPlayerTurn = true;

        TransitionToPhase(GamePhase.PlayerMana);
    }

    /// <summary>
    /// 【END TURN ボタン用】
    /// </summary>
    public void OnEndTurnPressed()
    {
        if (!isInBattle || !isPlayerTurn) return;

        Debug.Log("--- (TEST) 相手のターンへ ---");
        isPlayerTurn = false;
        currentPhase = GamePhase.End;

        uiManager.SetEnemyTurnActive(true);
        uiManager.HideAllPhaseUI();
        uiManager.ShowEndTurnButton(false); // ★ 修正 (ここで非表示を明示)

        StartCoroutine(EnemyTurnCoroutine());
    }

    // 相手のターンをシミュレートするコルーチン (テスト用)
    private IEnumerator EnemyTurnCoroutine()
    {
        // uiManager.ShowEndTurnButton(false); // OnEndTurnPressedが先に隠すので不要かも

        // 相手のフェーズを順番にシミュレート
        yield return new WaitForSeconds(1.0f);
        TransitionToPhase(GamePhase.EnemyMana); // ここで EndTurn が非表示になる
        yield return new WaitForSeconds(1.5f);
        TransitionToPhase(GamePhase.EnemySummon);
        yield return new WaitForSeconds(1.5f);
        TransitionToPhase(GamePhase.EnemyAttack);
        yield return new WaitForSeconds(1.5f);
        TransitionToPhase(GamePhase.EnemyBlock);
        yield return new WaitForSeconds(1.0f);

        Debug.Log("--- (TEST) プレイヤーのターンへ ---");
        isPlayerTurn = true;
        // uiManager.ShowEndTurnButton(true); // TransitionToPhaseが制御するので不要
        uiManager.SetEndTurnButtonText("End Turn");

        // プレイヤーのターンに戻ったら、自動的にマナチャージフェーズから開始
        TransitionToPhase(GamePhase.PlayerMana); // ここで EndTurn が表示される
    }

    // -------------------------------------------------------------
    // ## UI切り替えのコアロジック ##
    // -------------------------------------------------------------

    /// <summary>
    /// 【UI切り替えの本体】
    /// 指定されたゲームフェーズのUIだけを表示し、他を非表示にする
    /// </summary>
    public void SetPhaseUIVisibility(GamePhase phase)
    {
        currentPhase = phase;
        if (uiManager == null) return;

        // 1. 最初にすべてのフェーズUIと専用ボタンを非表示にする
        uiManager.HideAllPhaseUI();

        // 2. 指定されたフェーズのUIだけを表示する
        switch (phase)
        {
            case GamePhase.PlayerMana:
                uiManager.ShowPlayerManaUI(true);
                break;
            case GamePhase.PlayerSummon:
                uiManager.ShowPlayerSummonUI(true);
                break;
            case GamePhase.PlayerAttack:
                uiManager.ShowPlayerAttackUI(true);
                break;
            case GamePhase.PlayerBlock:
                uiManager.ShowPlayerBlockUI(true);
                uiManager.ShowBlockAcceptButton(true);
                break;

            case GamePhase.EnemyMana:
                uiManager.ShowEnemyManaUI(true);
                break;
            case GamePhase.EnemySummon:
                uiManager.ShowEnemySummonUI(true);
                break;
            case GamePhase.EnemyAttack:
                uiManager.ShowEnemyAttackUI(true);
                break;
            case GamePhase.EnemyBlock:
                uiManager.ShowEnemyBlockUI(true);
                break;

            case GamePhase.None:
            case GamePhase.End:
                // 何も表示しない
                break;
        }

        // ▼▼▼ 【修正】 ターンインジケーターとEndTurnボタンの制御 ▼▼▼
        if (phase == GamePhase.PlayerMana || phase == GamePhase.PlayerSummon ||
            phase == GamePhase.PlayerAttack || phase == GamePhase.PlayerBlock)
        {
            uiManager.SetPlayerTurnActive(true);
            uiManager.ShowEndTurnButton(true); // プレイヤーのターンは表示
            isPlayerTurn = true;
        }
        else if (phase == GamePhase.EnemyMana || phase == GamePhase.EnemySummon ||
                 phase == GamePhase.EnemyAttack || phase == GamePhase.EnemyBlock)
        {
            uiManager.SetEnemyTurnActive(true);
            uiManager.ShowEndTurnButton(false); // 相手のターンは非表示
            isPlayerTurn = false;
        }
        else if (phase == GamePhase.End)
        {
            uiManager.SetEnemyTurnActive(true);
            uiManager.ShowEndTurnButton(false); // 相手のターンは非表示
            isPlayerTurn = false;
        }
        else if (phase == GamePhase.None)
        {
            uiManager.SetPlayerTurnActive(isPlayerTurn); // 
            uiManager.ShowEndTurnButton(false); // バトル開始前は非表示
        }

        Debug.Log($"[UI TEST] フェーズUI切り替え: {phase.ToString()} のUIを表示");
    }

    /// <summary>
    /// 【UI切り替えの窓口】
    /// 任意のフェーズに直接移行する
    /// </summary>
    public void TransitionToPhase(GamePhase targetPhase)
    {
        // UIパネルを変更 (ターンインジケーターとEndTurnボタンもここで切り替わる)
        SetPhaseUIVisibility(targetPhase);
    }

    // -------------------------------------------------------------
    // ## 【Button割り当て用】
    // (ButtonのOnClick()に、以下の8つの関数を割り当ててください)
    // -------------------------------------------------------------

    public void TestShowPlayerMana()
    {
        TransitionToPhase(GamePhase.PlayerMana);
    }

    public void TestShowPlayerSummon()
    {
        TransitionToPhase(GamePhase.PlayerSummon);
    }

    public void TestShowPlayerAttack()
    {
        TransitionToPhase(GamePhase.PlayerAttack);
    }

    public void TestShowPlayerBlock()
    {
        TransitionToPhase(GamePhase.PlayerBlock);
    }

    public void TestShowEnemyMana()
    {
        TransitionToPhase(GamePhase.EnemyMana);
    }

    public void TestShowEnemySummon()
    {
        TransitionToPhase(GamePhase.EnemySummon);
    }

    public void TestShowEnemyAttack()
    {
        TransitionToPhase(GamePhase.EnemyAttack);
    }

    public void TestShowEnemyBlock()
    {
        TransitionToPhase(GamePhase.EnemyBlock);
    }
}