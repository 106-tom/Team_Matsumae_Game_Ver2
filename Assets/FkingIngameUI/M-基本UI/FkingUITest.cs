using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshProUGUI を使う場合

/// <summary>
/// FkingGameTester によって制御されるUIマネージャー (兼用バージョン)
/// </summary>
public class FkingUITest : MonoBehaviour
{
    [Header("ボタン類")]
    public GameObject battleButton;
    public GameObject endTurnButton;
    public TextMeshProUGUI endTurnButtonText;

    [Header("ターン表示")]
    public GameObject playerTurnIndicator; // (m2-AllyTurn を設定)
    public GameObject enemyTurnIndicator; // (m3-EnemyTurn を設定)

    [Header("ライフ表示")]
    public TextMeshProUGUI playerLifeText;
    private int currentPlayerLife = 20; // 例

    [Header("フェーズごとのUIパネル (兼用)")]
    public GameObject playerManaPhasePanel;
    public GameObject playerSummonPhasePanel;
    public GameObject playerAttackPhasePanel;
    public GameObject playerBlockPhasePanel;
    public GameObject enemyManaPhasePanel;
    public GameObject enemySummonPhasePanel;
    public GameObject enemyAttackPhasePanel;
    public GameObject enemyBlockPhasePanel;

    [Header("フェーズ専用ボタン")]
    public GameObject blockAcceptButton; // ブロックフェーズの「受け入れ」ボタン

    // --- 既存の関数 ---

    public void ShowBattleButton(bool show)
    {
        if (battleButton != null) battleButton.SetActive(show);
    }

    public void ShowEndTurnButton(bool show)
    {
        if (endTurnButton != null) endTurnButton.SetActive(show);
    }

    public void SetEndTurnButtonText(string text)
    {
        if (endTurnButtonText != null) endTurnButtonText.text = text;
    }

    public void SetPlayerTurnActive(bool isActive)
    {
        if (playerTurnIndicator != null) playerTurnIndicator.SetActive(isActive);
        if (enemyTurnIndicator != null) enemyTurnIndicator.SetActive(!isActive);
    }

    public void SetEnemyTurnActive(bool isActive)
    {
        SetPlayerTurnActive(!isActive);
    }

    public void ChangePlayerLife(int amount)
    {
        currentPlayerLife += amount;
        if (playerLifeText != null)
        {
            playerLifeText.text = currentPlayerLife.ToString();
        }
    }


    // ▼▼▼ 8フェーズのUI制御関数 ▼▼▼

    /// <summary>
    /// すべてのフェーズUIパネルと専用ボタンを一度に非表示にする
    /// </summary>
    public void HideAllPhaseUI()
    {
        if (playerManaPhasePanel != null) playerManaPhasePanel.SetActive(false);
        if (playerSummonPhasePanel != null) playerSummonPhasePanel.SetActive(false);
        if (playerAttackPhasePanel != null) playerAttackPhasePanel.SetActive(false);
        if (playerBlockPhasePanel != null) playerBlockPhasePanel.SetActive(false);
        if (enemyManaPhasePanel != null) enemyManaPhasePanel.SetActive(false);
        if (enemySummonPhasePanel != null) enemySummonPhasePanel.SetActive(false);
        if (enemyAttackPhasePanel != null) enemyAttackPhasePanel.SetActive(false);
        if (enemyBlockPhasePanel != null) enemyBlockPhasePanel.SetActive(false);

        if (blockAcceptButton != null) blockAcceptButton.SetActive(false);
    }

    public void ShowBlockAcceptButton(bool show)
    {
        if (blockAcceptButton != null) blockAcceptButton.SetActive(show);
    }

    public void ShowPlayerManaUI(bool show)
    {
        if (playerManaPhasePanel != null) playerManaPhasePanel.SetActive(show);
    }

    public void ShowPlayerSummonUI(bool show)
    {
        if (playerSummonPhasePanel != null) playerSummonPhasePanel.SetActive(show);
    }

    public void ShowPlayerAttackUI(bool show)
    {
        if (playerAttackPhasePanel != null) playerAttackPhasePanel.SetActive(show);
    }

    public void ShowPlayerBlockUI(bool show)
    {
        if (playerBlockPhasePanel != null) playerBlockPhasePanel.SetActive(show);
    }

    public void ShowEnemyManaUI(bool show)
    {
        if (enemyManaPhasePanel != null) enemyManaPhasePanel.SetActive(show);
    }

    public void ShowEnemySummonUI(bool show)
    {
        if (enemySummonPhasePanel != null) enemySummonPhasePanel.SetActive(show);
    }

    public void ShowEnemyAttackUI(bool show)
    {
        if (enemyAttackPhasePanel != null) enemyAttackPhasePanel.SetActive(show);
    }

    public void ShowEnemyBlockUI(bool show)
    {
        if (enemyBlockPhasePanel != null) enemyBlockPhasePanel.SetActive(show);
    }

    // --- 古い関数名も残しておく ---
    public void ShowSummonPhaseUI(bool show) { ShowPlayerSummonUI(show); }
    public void ShowBattlePhaseUI(bool show) { ShowPlayerAttackUI(show); }
    public void ShowBlockPhaseUI(bool show) { ShowPlayerBlockUI(show); }
}