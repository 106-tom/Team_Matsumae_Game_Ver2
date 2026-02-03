using Photon.Pun.Demo.PunBasics;
using System.Collections;
using UnityEngine;

public class GameSetup : MonoBehaviour
{
	//private void Start() => StartCoroutine(SetupRoutine());
	//
	//private IEnumerator SetupRoutine()
	//{
	//	// ① Local / Remote の登録待ち
	//	while (PlayerManager.LocalPlayer == null || PlayerManager.RemotePlayer == null)
	//		yield return null;
	//
	//	// ② UI が確実に生成されるまで数フレーム待つ
	//	// GameStartInitializer が Start() 内で Instantiate しているので
	//	// フレームを 2 回以上待てば必ず UI が存在する
	//	yield return null;
	//	yield return null;
	//
	//	// ③ PlayerManager.Init()（UI参照もこの時点では必ず入る）
	//	PlayerManager.LocalPlayer.Init();
	//	PlayerManager.RemotePlayer.Init();
	//
	//	// ④ ターン開始
	//	TurnManager.Instance.StartTurn();
	//}
}
