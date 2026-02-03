using UnityEngine;
using UnityEngine.SceneManagement;

// ボタンで指定したシーンに移動するためのシンプルなクラス
public class FkingSceneLoader: MonoBehaviour
{
    // Inspectorに表示され、シーン名を文字列で指定できる
    [Tooltip("ロードしたいシーンの名前をここに入力してください")]
    public string targetSceneName;

    // ボタンの OnClick() イベントから呼び出すための公開関数
    public void LoadTargetScene()
    {
        // targetSceneName が空でないかチェック
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("FkingSceneLoader: targetSceneName が設定されていません！");
            return;
        }

        // 指定された名前のシーンをロードする
        SceneManager.LoadScene(targetSceneName);
    }
}