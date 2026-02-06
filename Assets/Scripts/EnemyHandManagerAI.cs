using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHandManagerAI : MonoBehaviour
{
    static public EnemyHandManagerAI Instance { get; private set; }

    public Transform handZoneParent;
    public Transform fieldZoneParent;
    public float offsetX = 120f;   // カード間の距離（調整可）
    public float fieldOffsetX = 200f;   // カード間の距離（調整可）

    private void Awake()
    {
        Instance = this;
    }

    public void ArrangeHand()
    {
        Debug.Log("手札調整");
        int count = handZoneParent.childCount;
        for (int i = 0; i < count; i++)
        {
            Transform card = handZoneParent.GetChild(i);
            float posX = i * offsetX;
            card.localPosition = new Vector3(posX, 0, 0);
        }
    }

    public void ArrangeField()
    {
        int count = fieldZoneParent.childCount;
        if (count == 0) return;

        // 間隔を200に固定
        float spacing = 200f;
        Debug.Log("ArrangField : " + count);
        // 全体の幅の半分を計算して開始位置を決める
        float totalWidth = spacing * (count - 1);
        float startX = -totalWidth / 2f;

        for (int i = 0; i < count; i++)
        {
            Transform card = fieldZoneParent.GetChild(i);
            // 開始地点から 200 ずつ右にずらす
            float posX = startX + (i * spacing);
            card.localPosition = new Vector3(posX, 0, 0);
        }
    }
}
