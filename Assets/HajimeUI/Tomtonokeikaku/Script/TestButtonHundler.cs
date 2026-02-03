using UnityEngine;

/// <summary>
/// テストボタンからの呼び出しを、FkingCardSpawnerに仲介するスクリプト
/// </summary>
public class TestButtonHandler : MonoBehaviour
{
    [Header("呼び出し先のSpawner")]
    [Tooltip("シーン内にいる FkingCardSpawner を設定してください")]
    public FkingCardSpawner cardSpawner;

    [Header("テスト用のカード設定")]
    public string testCardID = "01";
    public int testAmount = 1;
    public Vector3 testPosition = new Vector3(0f, 0f, 0f);

    /// <summary>
    /// 【重要】Unityのボタンからこの関数を呼び出します
    /// </summary>
    public void SpawnTestCard()
    {
        if (cardSpawner == null)
        {
            Debug.LogError("Card Spawnerが設定されていません！", this);
            return;
        }

        // FkingCardSpawner の関数を、Inspectorで設定した引数で呼び出す
        Debug.Log($"テスト召喚: ID={testCardID}, 枚数={testAmount}");
        cardSpawner.SpawnCards(testCardID, testAmount, testPosition);
    }
}