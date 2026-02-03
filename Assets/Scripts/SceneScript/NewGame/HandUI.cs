using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandUI : MonoBehaviour
{
	[SerializeField] private GameObject cardUIPrefab;

	private PlayerManager ownerPlayer;
	private Transform handParent;
	private readonly List<CardUI> cardUIList = new List<CardUI>();

	[SerializeField] private float rotateTime = 0.5f;
	[SerializeField] private AnimationCurve rotateCurve;

	public bool IsInitialized { get; private set; } = false;

	public void Initialize(PlayerManager player, Transform parent)
	{
		ownerPlayer = player;
		handParent = parent;
		IsInitialized = true;
	}

	public void CreateCardUI(CardInstance card)
	{
		if (!IsInitialized || ownerPlayer == null) return;

		var go = Instantiate(cardUIPrefab, handParent);
		var ui = go.GetComponent<CardUI>();

		bool faceUp = ownerPlayer.photonView.IsMine;
		ui.Setup(card.cardData, faceUp);
		ui.SetInstance(card);
		cardUIList.Add(ui);

		StartCoroutine(PlayDrawAnimation(go));
	}

	private IEnumerator PlayDrawAnimation(GameObject go)
	{
		Vector3 startRotation = new Vector3(90f, 0f, 0f);
		Vector3 endRotation = new Vector3(450f, 0f, 0f);
		float elapsed = 0f;
		while (elapsed < rotateTime)
		{
			elapsed += Time.deltaTime;
			float t = elapsed / rotateTime;
			go.transform.eulerAngles = Vector3.Lerp(startRotation, endRotation, rotateCurve.Evaluate(t));
			yield return null;
		}
	}
}
