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

	[Header("攻撃対象のアイコン位置")]
	[SerializeField] private RectTransform playerRectTransform;
	[SerializeField] private RectTransform enemyRectTransform;
	[Space(10)]

	[Header("画面振動用")]
	[Tooltip("振動の強さ")]
	[SerializeField] private float amplitude;
	[Tooltip("振動の速さ")]
	[SerializeField] private float frequency;
	[Tooltip("振動時間")]
	[SerializeField] private float shakeTime;

	private CardMotionHelper cardMotionHelper;

	private Vector3 blockPosition = new Vector3();

	private void Start()
	{
		cardMotionHelper = SystemManager.Instance.cardMotionHelper;
	}

	/// <summary>
	/// 攻撃演出開始
	/// </summary>
	public IEnumerator StartAttack(
		Transform attackCard,
		Transform blockCard,
		PlayerSide attackerSide,
		bool isDirectAttack)
	{
		// 攻撃対象位置の決定
		if (blockCard == null)
		{
			blockPosition = attackerSide == PlayerSide.Self
				? enemyRectTransform.localPosition
				: playerRectTransform.localPosition;
		}
		else
		{
			blockPosition = blockCard.localPosition;
			blockPosition += new Vector3(0f, 150f, 0f);
			Debug.Log("敵カードに攻撃" + blockPosition);
		}

		Vector3 myFirstPosition = attackCard.localPosition;

		// 浮上
		Vector3 upHeight = new Vector3(0f, 0f, -upOffset);
		Vector3 upStartPosition = myFirstPosition;
		Vector3 upEndPosition = myFirstPosition + upHeight;

		yield return StartCoroutine(
			cardMotionHelper.MoveTarget(
				attackCard,
				upTime,
				upStartPosition,
				upEndPosition,
				upCurve
			)
		);

		// 前進開始位置と終了位置
		Vector3 forwardStartPosition = attackCard.localPosition;
		Vector3 forwardEndPosition = blockPosition + upHeight;

		// 前進
		yield return StartCoroutine(
			cardMotionHelper.MoveTarget(
				attackCard,
				forwardTime,
				forwardStartPosition,
				forwardEndPosition,
				forwardCurve
			)
		);

		// ブロック演出
		if (!isDirectAttack)
			StartCoroutine(MotionManager.Instance.block.StartBlock(null));

		// 画面振動
		CameraManager cameraManager = CameraManager.Instance;
		yield return StartCoroutine(
			cameraManager.CameraShake(0f, amplitude, frequency, shakeTime)
		);

		StartCoroutine(
			cameraManager.CameraShake(amplitude, 0f, frequency, shakeTime)
		);

		// 戻る（浮上しながら）
		yield return StartCoroutine(
			cardMotionHelper.MoveTarget(
				attackCard,
				upTime,
				forwardEndPosition,
				forwardStartPosition,
				upCurve
			)
		);

		// 元の位置に戻る（下降）
		Vector3 downStartPosition = attackCard.localPosition;
		Vector3 downEndPosition = myFirstPosition;

		StartCoroutine(
			cardMotionHelper.MoveTarget(
				attackCard,
				downTime,
				downStartPosition,
				downEndPosition,
				downCurve
			)
		);

		yield return new WaitForSeconds(0.2f);
	}
}
