using UnityEngine;

public class InputManagerAI : MonoBehaviour
{
	public static InputManagerAI Instance;

	[Header("Raycast—pƒJƒƒ‰")]
	public Camera clickCamera;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
	}


	private void Update()
	{
		if (!Input.GetMouseButtonDown(0)) return;

		if (clickCamera == null)
		{
			Debug.LogError("InputManagerAI: clickCamera ‚ªİ’è‚³‚ê‚Ä‚¢‚Ü‚¹‚ñ");
			return;
		}

		Ray ray = clickCamera.ScreenPointToRay(Input.mousePosition);

		if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
		{
			var card = hit.collider.GetComponentInParent<IClickableAI>();
			if (card != null)
			{
				card.OnClick();
			}
		}
	}
}
