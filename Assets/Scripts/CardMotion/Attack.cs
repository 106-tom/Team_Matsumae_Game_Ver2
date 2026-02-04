using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 攻撃演出クラス
/// </summary>
public class Attack : MonoBehaviour
{
	[Header("カード前進")]
	[Tooltip("前進時間")]
	[SerializeField] private float forwardTime;
	[Tooltip("前進時間")]
	[SerializeField] private AnimationCurve forwardCurve;
	[Space(10)]

	[Header("カード浮上")]
	[Tooltip("浮上時間")]
	[SerializeField] private float upTime;
	[Tooltip("どの程度浮上させるか")]
	[SerializeField] private float upOffset;
	[Tooltip("浮上速度変化グラフ")]
	[SerializeField] private AnimationCurve upCurve;
	[Space(10)]

	[Header("カード下降")]
	[Tooltip("下降時間")]
	[SerializeField] private float downTime;
	[Tooltip("下降速度変化グラフ")]
	[SerializeField] private AnimationCurve downCurve;
	[Space(10)]

	[Header("カード回転")]
	[Tooltip("回転時間")]
	[SerializeField] private float rotateTime;
	[Tooltip("回転速度変化グラフ")]
	[SerializeField] private AnimationCurve rotateCurve;

<<<<<<< HEAD
	[Header("攻撃対象のアイコン位置")]
	[SerializeField] private RectTransform playerRectTransform;
	[SerializeField] private RectTransform enemyRectTransform;

	private CardMotionHelper cardMotionHelper;

	private Vector3 blockPosition = new Vector3();
=======
    [Header("攻撃対象のアイコン位置")]
    [SerializeField] private RectTransform playerRectTransform;
    [SerializeField] private RectTransform enemyRectTransform;

    private CardMotionHelper cardMotionHelper;

    private Vector3 blockPosition = new Vector3();

    private void Start()
    {
        cardMotionHelper = SystemManager.Instance.cardMotionHelper;
    }
    
    /// <summary>
    /// 攻撃演出開始
    /// </summary>
    /// <param name="blockCard">攻撃対象</param>
    /// <param name="isDead">攻撃後破壊されるかどうか</param>
    /// <param name="attackerSide">攻撃する側</param>
    /// <returns></returns>
    public IEnumerator StartAttack(
        Transform attackCard, 
        Transform blockCard, 
        PlayerSide attackerSide,
        bool isDirectAttack)
    {
        if (blockCard == null)
        {
            blockPosition = attackerSide == PlayerSide.Self
                ? blockPosition = enemyRectTransform.localPosition
                : enemyRectTransform.localPosition;
        }
        else
        {
            blockPosition = blockCard.position;
            Debug.Log("敵カードに攻撃" + blockPosition);
        }
        Vector3 myFirstPosition = attackCard.localPosition;

        // どれぐらい浮かせるか
        // 上昇処理の始点と終点を設定
        Vector3 upHeight = new Vector3(0f, 0f, -upOffset);
        Vector3 upStartPosition = myFirstPosition;
        Vector3 upEndPosition = myFirstPosition + upHeight;
>>>>>>> origin/tom

	private void Start()
	{
		cardMotionHelper = SystemManager.Instance.cardMotionHelper;
	}

	/// <summary>
	/// 攻撃演出開始
	/// </summary>
	/// <param name="blockCard">攻撃対象</param>
	/// <param name="isDead">攻撃後破壊されるかどうか</param>
	/// <param name="attackerSide">攻撃する側</param>
	/// <returns></returns>
	public IEnumerator StartAttack(Transform attackCard, Transform blockCard, PlayerSide attackerSide)
	{
		if (blockCard == null)
		{
			blockPosition = attackerSide == PlayerSide.Self
				? blockPosition = enemyRectTransform.localPosition
				: enemyRectTransform.localPosition;
		}
		else
		{
			blockPosition = blockCard.position;
			Debug.Log("敵カードに攻撃" + blockPosition);
		}
		Vector3 myFirstPosition = attackCard.localPosition;

		// どれぐらい浮かせるか
		// 上昇処理の始点と終点を設定
		Vector3 upHeight = new Vector3(0f, 0f, -upOffset);
		Vector3 upStartPosition = myFirstPosition;
		Vector3 upEndPosition = myFirstPosition + upHeight;

		// 浮かせる
		yield return StartCoroutine(cardMotionHelper.MoveTarget(attackCard, upTime, upStartPosition, upEndPosition, upCurve));

<<<<<<< HEAD
		// 下降処理の始点と終点を設定
		Vector3 downStartPosition = attackCard.localPosition;
		Vector3 downEndPosition = myFirstPosition;
=======
        // 攻撃対象へ向きを変えながら前進
        //StartCoroutine(cardMotionHelper.RotationTarget(attackCard, rotateTime, startRotation, endRotation, rotateCurve));
        yield return StartCoroutine(cardMotionHelper.MoveTarget(attackCard, forwardTime, forwardStartPosition, forwardEndPosition, forwardCurve));
        // 一旦null
        if(isDirectAttack)
            StartCoroutine(MotionManager.Instance.block.StartBlock(null));
>>>>>>> origin/tom

		// 前進処理の始点と終点を設定
		Vector3 forwardEndPosition = blockPosition + upHeight;
		Vector3 forwardStartPosition = attackCard.localPosition;

		// 回転の始点と終点を計算
		Quaternion startRotation = attackCard.localRotation;
		float targetYAngle = cardMotionHelper.GetYAngleToTarget(myFirstPosition, blockPosition);
		Quaternion endRotation = Quaternion.Euler(0f, 0f, targetYAngle) * startRotation;

		// 攻撃対象へ向きを変えながら前進
		//StartCoroutine(cardMotionHelper.RotationTarget(attackCard, rotateTime, startRotation, endRotation, rotateCurve));
		yield return StartCoroutine(cardMotionHelper.MoveTarget(attackCard, forwardTime, forwardStartPosition, forwardEndPosition, forwardCurve));
		// 一旦null
		StartCoroutine(MotionManager.Instance.block.StartBlock(null));

		// 元の位置に戻る
		yield return StartCoroutine(cardMotionHelper.MoveTarget(attackCard, upTime, forwardEndPosition, forwardStartPosition, upCurve));

		// 下げながら向きを戻す
		StartCoroutine(cardMotionHelper.MoveTarget(attackCard, downTime, downStartPosition, downEndPosition, downCurve));

		yield return new WaitForSeconds(0.2f);
	}
}