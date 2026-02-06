using System.Collections;
using System.Collections.Generic;
//using Unity.VisualScripting.ReorderableList.Internal;
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