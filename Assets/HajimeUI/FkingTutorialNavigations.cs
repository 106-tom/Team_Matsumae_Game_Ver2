using UnityEngine;

// 複数のUIパネル（画面）の表示を管理するクラス
public class FkingTutorialNavigations : MonoBehaviour
{
    // メインメニューのパネル（親GameObject）
    public GameObject mainMenuPanel;

    // 「遊び方」画面のパネル（親GameObject）
    public GameObject howToPlayPanel;

    // ゲーム開始時に呼ばれる
    void Start()
    {
        // 起動時はメインメニューを
        ShowMainMenuScreen();
    }

    // 「遊び方」画面を表示する関数
    public void ShowHowToPlayScreen()
    {
        mainMenuPanel.SetActive(false); // メインメニューを非表示
        howToPlayPanel.SetActive(true);  // 遊び方画面を表示
    }

    // 「メインメニュー」画面を表示する（戻る）関数
    public void ShowMainMenuScreen()
    {
        mainMenuPanel.SetActive(true);   // メインメニューを表示
        howToPlayPanel.SetActive(false); // 遊び方画面を非表示
    }
}