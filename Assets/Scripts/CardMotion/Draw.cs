using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// ドロー演出処理
/// </summary>
public class Draw : MonoBehaviour
{
    [Header("カード回転")]
    [Tooltip("回転時間")]
    [SerializeField] private float rotateTime = 2.0f;
    [Tooltip("回転速度変化グラフ")]
    [SerializeField] private AnimationCurve rotateCurve;

    private MeshRenderer[] meshRenderers; // ドローした影をいじるためのMeshRenderer配列

    /// <summary>
    /// カードドロー演出
    /// </summary>
    /// <returns></returns>
    public IEnumerator StartDraw(GameObject newCard, GameObject hand)
    {
        Debug.Log("ドロー開始");

        CardMotionHelper mathHelper = SystemManager.Instance.cardMotionHelper;
        AdjustCardPosition adjustCardPosition = AdjustCardPosition.Instance;

        // 新しくドローしたカードの影を落とさない設定にする
        meshRenderers = newCard.GetComponentsInChildren<MeshRenderer>(true);
        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
          
        float elaspedTime = 0f;

        // 回転の初期位置と終了位置の設定
        Quaternion startRotation = Quaternion.Euler(0f, 0f, 0f);
        Quaternion endRotation = Quaternion.Euler(360f, 0f, 360f); // 360度足して1回転させる

        // 手札間の距離を設定
        // ドロー後の位置
        float cardSpace = adjustCardPosition.handCardSpaceLength;
        int index = hand.transform.childCount - 1;
        Vector3 endPosition = adjustCardPosition.handCenter + new Vector3(cardSpace * index, 0, 0);
       
        // ドローしたカードの初期位置
        Vector3 startPosition = newCard.transform.localPosition;

        // 回転しながら手札のいちばん右側まで移動する
        while (elaspedTime < rotateTime)
        {
            // 時間経過
            elaspedTime += Time.deltaTime;          

            // 回転処理
            float t = elaspedTime / rotateTime;
            float curveT = rotateCurve.Evaluate(t);
            //newCard.transform.localRotation = Quaternion.Lerp(startRotation, endRotation, curveT);
            newCard.transform.localRotation = Quaternion.Euler(0f, 360f * t, 0f);

            // ドローしたカードと手札全体の移動処理
            newCard.transform.localPosition = Vector3.Lerp(startPosition, endPosition, curveT);
            yield return null;
        }
        newCard.transform.localPosition = endPosition;     
    }
}
