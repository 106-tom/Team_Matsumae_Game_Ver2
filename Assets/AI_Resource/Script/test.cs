using UnityEngine;

public class test : MonoBehaviour
{
	void Update()
	{
		// InputManagerAI からカメラを取得
		Camera cam = InputManagerAI.Instance?.clickCamera;
		if (cam == null)
			return;

		if (Input.GetMouseButtonDown(0))
		{
			Ray ray = cam.ScreenPointToRay(Input.mousePosition);

			// デバッグ可視化（Sceneビュー）
			Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 1f);

			if (Physics.Raycast(ray, out RaycastHit hit))
			{
				if (hit.collider.gameObject == gameObject)
				{
					Debug.Log("オブジェクトがクリックされました！: " + hit.collider.name);
				}
			}
		}
	}
}
