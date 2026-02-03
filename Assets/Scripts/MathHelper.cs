using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 計算補助クラス
/// </summary>
public class MathHelper : MonoBehaviour
{
    /// <summary>
    /// 移動処理
    /// </summary>
    /// <param name="time">移動にかける時間</param>
    /// <param name="startPosition">移動の初期位置</param>
    /// <param name="endPosition">移動の最終位置</param>
    /// <param name="curve">移動速度変化グラフ</param>
    /// <returns></returns>
    public IEnumerator Move(float time, Vector3 startPosition, Vector3 endPosition, AnimationCurve curve = null)
    {
        float elapsedTime = 0f;
        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / time;
            if (curve == null)
            {
                transform.position = Vector3.Lerp(startPosition, endPosition, t);
            }
            else
            {
                float curveValue = curve.Evaluate(t);
                transform.position = Vector3.Lerp(startPosition, endPosition, curveValue);
            }
            yield return null;
        }
        transform.position = endPosition;
    }

    /// <summary>
    /// 指定したオブジェクトの移動処理
    /// </summary>
    /// <param name="targetTransform">動かしたいオブジェクトのTransform</param>
    /// <param name="startPosition">移動の初期位置</param>
    /// <param name="endPosition">移動の最終位置</param>
    /// <param name="curve">移動速度変化グラフ</param>
    /// <returns></returns>
    public IEnumerator MoveTarget(Transform targetTransform, float time, Vector3 startPosition, Vector3 endPosition, AnimationCurve curve = null)
    {
        float elapsedTime = 0f;
        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / time;
            if (curve == null)
            {
                targetTransform.position = Vector3.Lerp(startPosition, endPosition, t);
            }
            else
            {
                float curveValue = curve.Evaluate(t);
                targetTransform.position = Vector3.Lerp(startPosition, endPosition, curveValue);
            }
            yield return null;
        }
        targetTransform.position = endPosition;
    }

    /// <summary>
    /// 回転処理
    /// </summary>
    /// <param name="time">回転にかける時間</param>
    /// <param name="startRotate"></param>
    /// <param name="endRotate"></param>
    /// <param name="curve"></param>
    /// <returns></returns>
    public IEnumerator Rotation(float time, Quaternion startRotate, Quaternion endRotate, AnimationCurve curve = null)
    {
        float elapsedTime = 0f;
        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / time;
            if (curve == null)
            {
                transform.rotation = Quaternion.Lerp(startRotate, endRotate, t);
            }
            else
            {
                float curveValue = curve.Evaluate(t);
                transform.rotation = Quaternion.Lerp(startRotate, endRotate, curveValue);
            }
           
            yield return null;
        }
        transform.rotation = endRotate;
    }

    /// <summary>
    /// 回転処理
    /// </summary>
    /// <param name="time">回転にかける時間</param>
    /// <param name="startRotate"></param>
    /// <param name="endRotate"></param>
    /// <param name="curve"></param>
    /// <returns></returns>
    public IEnumerator TargetRotation(Transform targetTransform, float time, Quaternion startRotate, Quaternion endRotate, AnimationCurve curve = null)
    {
        float elapsedTime = 0f;
        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / time;
            if (curve == null)
            {
                targetTransform.rotation = Quaternion.Lerp(startRotate, endRotate, t);
            }
            else
            {
                float curveValue = curve.Evaluate(t);
                targetTransform.rotation = Quaternion.Lerp(startRotate, endRotate, curveValue);
            }

            yield return null;
        }
        targetTransform.rotation = endRotate;
    }

    // ターゲット方向へのY軸角度を計算する
    public float GetYAngleToTarget(Vector3 currentPosition, Vector3 targetPosition)
    {
        // 敵への方向ベクトルを計算
        Vector3 direction = targetPosition - currentPosition;

        // 水平方向の回転だけを計算するために、Y成分を0にする
        direction.y = 0;

        // その方向を向くための理想的な回転（クォータニオン）を計算
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // 計算された回転からY軸の角度だけを取得
        return targetRotation.eulerAngles.y;
    }

    // 拡大,縮小
    public IEnumerator Scaling(float time, Vector3 startScale, Vector3 endScale, AnimationCurve curve = null)
    {
        float elapsedTime = 0f;
        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / time;
            if (curve == null)
            {
                transform.localScale = Vector3.Lerp(startScale, endScale, t);
            }
            else
            {
                float curveValue = curve.Evaluate(t);
                transform.localScale = Vector3.Lerp(startScale, endScale, curveValue);
            }
           
            yield return null;
        }
        transform.localScale = endScale; // 最終スケールに設定
    }

    // 特定のオブジェクトを拡大・縮小
    public IEnumerator ScalingTarget(Transform targetTransform, float time, Vector3 startScale, Vector3 endScale, AnimationCurve curve = null)
    {
        float elapsedTime = 0f;
        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / time;
            if (curve == null)
            {
                targetTransform.localScale = Vector3.Lerp(startScale, endScale, t);
            }
            else
            {
                float curveValue = curve.Evaluate(t);
                targetTransform.localScale = Vector3.Lerp(startScale, endScale, curveValue);
            }
            yield return null;
        }
        targetTransform.localScale = endScale; // 最終スケールに設定
    }
}
