using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// カード位置調整クラス(シングルトンクラス)
public class AdjustCardPosition : MonoBehaviour
{
    [Header("プレイヤーフィールドの中央位置")]
    public Vector3 fieldCenter = new Vector3(0.05f, 0.1f, -0.6f);
    [Space(10)]

    [Header("プレイヤー手札の中央位置")]
    public Vector3 handCenter = new Vector3(0.1f, 1.1f, -1.4f);
    [Space(10)]

    [Header("カード間の距離")]
    [Tooltip("手札のカード間距離")]
    public float handCardSpaceLength = 0.6f;
    [Tooltip("フィールドのカード間距離")]
    public float fieldCardSpaceLength = 1.0f;
    [Space(10)]

    [Header("カード移動時間")]
    [SerializeField] private float moveTime = 0.2f;
    public static AdjustCardPosition Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 手札の位置調整処理
    /// </summary>
    /// <param name="handCard">手札から出すカード</param>
    public void AdjustHandCardsPosition(List<CardAI> handCardList)
    {
        // 手札の枚数取得
        int handCount = handCardList.Count;
        if (handCount == 0) return;

        // 手札全体の幅 (Spacingはカードの中心間の距離)
        float totalWidth = (handCount - 1) * handCardSpaceLength;

        // 一番左のカードの開始位置を計算 (中心からのオフセット)
        float startX = handCenter.x - (totalWidth / 2f);
        for (int i = 0; i < handCount; ++i)
        {
            float targetX = startX + (i * handCardSpaceLength);
            //Vector3 startPosition = handCardList[i].gameObject.transform.position;
            Vector3 endPosition = handCenter;
            endPosition.x = targetX;

            //StartCoroutine(SystemManager.Instance.cardMotionHelper.MoveTarget(handCardList[i].gameObject.transform, moveTime, startPosition, endPosition));
        }
    }

    /// <summary>
    /// フィールドカードの位置調整処理
    /// </summary>
    /// <param name="fieldCard">フィールドカードから除外するカード</param>
    public void AdjustFieldCardsPosition(List<CardAI> fieldCardList)
    {
        // フィールドカードの枚数取得
        int fieldCount = fieldCardList.Count;
        if (fieldCount == 0) return;

        // 手札全体の幅 (Spacingはカードの中心間の距離)
        float totalWidth = (fieldCount - 1) * fieldCardSpaceLength;

        // 一番左のカードの開始位置を計算 (中心からのオフセット)
        float startX = fieldCenter.x - (totalWidth / 2f);
        for (int i = 0; i < fieldCount; ++i)
        {
            float targetX = startX + (i * fieldCardSpaceLength);
            //Vector3 startPosition = fieldCardList[i].gameObject.transform.position;
            Vector3 endPosition = fieldCenter;
            endPosition.x = targetX;

            //StartCoroutine(SystemManager.Instance.cardMotionHelper.MoveTarget(fieldCardList[i].gameObject.transform, moveTime, startPosition, endPosition));
        }
    }
}
