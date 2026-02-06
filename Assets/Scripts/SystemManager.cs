// ================= Editor 用名前空間 =================
#if UNITY_EDITOR
using UnityEditor;           // MenuItem などの Editor 機能
using UnityEditorInternal;   // ReorderableList
#endif

// ================= 通常名前空間 =================
using System.Collections;
using System.Collections.Generic;
<<<<<<< HEAD
//using Unity.VisualScripting.ReorderableList.Internal;
=======
>>>>>>> origin/washida2
using UnityEngine;


public class SystemManager : MonoBehaviour
{
	public static SystemManager Instance { get; private set; }

	public CardEffectHelper cardEffectHelper { get; private set; }
	public CardMotionHelper cardMotionHelper { get; private set; }
	public UIHelper uiHelper { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
			if (cardEffectHelper == null) cardEffectHelper = GetComponent<CardEffectHelper>();
			if (cardMotionHelper == null) cardMotionHelper = GetComponent<CardMotionHelper>();
			if (uiHelper == null) uiHelper = GetComponent<UIHelper>();
		}
		else
		{
			Destroy(gameObject);
		}
	}
}