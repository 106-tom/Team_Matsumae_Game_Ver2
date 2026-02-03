using UnityEngine;

/// <summary>
/// 指定されたID、枚数、位置にカードPrefabを「召喚（生成）」するスクリプト
/// </summary>
public class FkingCardSpawner : MonoBehaviour
{
    [Header("参照するPrefab")]
    [Tooltip("生成するカードのPrefab（FkingCardDisplayやFXManagerがアタッチされたもの）")]
    public GameObject cardPrefab;

    [Header("参照するデータベース")]
    [Tooltip("カードデータをIDで検索するためのCardDatabase")]
    public FkingCardDatabase cardDatabase; // ※FkingCardDatabase.cs が必要です

    /// <summary>
    /// 外部（例: ゲームマネージャー）から呼び出すための召喚関数
    /// </summary>
    /// <param name="cardID">FkingCardDataのID</param>
    /// <param name="amount">生成する枚数</param>
    /// <param name="position">生成する中心位置</param>
    public void SpawnCards(string cardID, int amount, Vector3 position)
    {
        if (cardPrefab == null)
        {
            Debug.LogError("Card Prefabが設定されていません！", this);
            return;
        }
        if (cardDatabase == null)
        {
            Debug.LogError("Card Databaseが設定されていません！", this);
            return;
        }

        FkingCardData dataToSpawn = cardDatabase.GetCardByID(cardID);

        if (dataToSpawn == null)
        {
            Debug.LogError($"ID '{cardID}' のカードデータが見つかりません。", this);
            return;
        }

        for (int i = 0; i < amount; i++)
        {
            Vector3 spawnPos = position + (Vector3.right * i * 0.1f);

            GameObject newCardInstance = Instantiate(cardPrefab, spawnPos, Quaternion.identity);
            newCardInstance.name = $"Card_{cardID}_{i}";

            // ▼▼▼ ここを修正 ▼▼▼
            // GetComponent -> GetComponentInChildren に変更
            // これで、子オブジェクトの「CardVisualHajime」まで探しに行きます
            FkingCardDisplay display = newCardInstance.GetComponentInChildren<FkingCardDisplay>();
            // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲

            if (display != null)
            {
                // FkingUpdateDisplay を呼び出して、カード情報を更新
                display.FkingUpdateDisplay(dataToSpawn);
            }
            else
            {
                // エラーメッセージを修正
                Debug.LogError("生成したPrefab（またはその子）に FkingCardDisplay がありません！", newCardInstance);
            }
        }
    }
}