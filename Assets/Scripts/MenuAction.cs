using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuActions : MonoBehaviour
{
    // 文字列ミス防止
    public static class Scenes
    {
        public const string Boot = "Boot";
        public const string Title = "Title";
        public const string Home = "Home";
        public const string DeckSelect = "DeckSelect";
        public const string DeckEdit = "DeckEdit";
        public const string VSMenu = "VSMenu";
        public const string MatchReady = "MatchReady";
        public const string MatchMenu = "MatchMenu";
        public const string MatchGame = "MatchGame";
        public const string Battle = "Battle";
        public const string Result = "Result";
        public const string CPUMenu = "CPUMenu";
        public const string MatchJoin = "MatchJoin";
        public const string CPUGame = "CPUGame";
        public const string CPUResult = "CPUResult";


    }

    // 汎用
    public void Go(string sceneName) => SceneTransition.Load(sceneName);
    public void ReloadCurrent() => SceneTransition.Load(SceneManager.GetActiveScene().name);
    public void Quit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // 個別（Button OnClick 用）
    public void GoBoot() => Go(Scenes.Boot);
    public void GoTitle() => Go(Scenes.Title);
    public void GoHome() => Go(Scenes.Home);
    public void GoDeckSelect() => Go(Scenes.DeckSelect);
    public void GoDeckEdit() => Go(Scenes.DeckEdit);
    public void GoMatchMenu() => Go(Scenes.MatchMenu);
    public void GoMatchReady() => Go(Scenes.MatchReady);
    public void GoMatchGame() => Go(Scenes.MatchGame);
    public void GoBattle() => Go(Scenes.Battle);
    public void GoResult() => Go(Scenes.Result);
    public void GoCPUMenu() => Go(Scenes.CPUMenu);
    public void GoMatchJoin() => Go(Scenes.MatchJoin);
    public void GoCPUGame() => Go(Scenes.CPUGame);
    public void GoCPUResult() => Go(Scenes.CPUResult);

    // 状態セット系（必要なときだけ使う）
    public void SelectDeck(string deckId)
    {
        if (AppState.I != null) AppState.I.selectedDeckId = deckId;
        else Debug.LogWarning("AppState not initialized.");
    }

    // よくある導線
    public void StartPvPFromHome() => Go(Scenes.VSMenu);        // Home → VSMenu
    public void StartCPUFromHome() => Go(Scenes.CPUMenu);       // Home → CPUMenu
    public void ProceedFromVSMenu() => Go(Scenes.MatchReady);    // VSMenu → MatchReady
    public void BeginBattle() => Go(Scenes.Battle);        // MatchReady/CPUMenu → Battle
    public void BackToHome() => Go(Scenes.Home);          // どこからでも Home
    public void ToResult() => Go(Scenes.Result);        // Battle 終了 → Result
    public void Rematch() => Go(Scenes.Battle);        // Result → Battle
    public void ReturnToMenuFromResult() => Go(Scenes.Home);          // Result → Home
}
