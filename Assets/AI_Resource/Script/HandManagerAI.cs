//using Unity.VisualScripting;
//using UnityEngine;
//
//public class HandManagerAI : MonoBehaviour
//{
//	static public HandManagerAI Instance { get; private set; }
//
//	public Transform handZoneParent;
//	public float offsetX = 120f;   // カード間の距離（調整可）
//
//    private void Awake()
//    {
//        Instance = this;
//    }
//
//    public void ArrangeHand()
//	{
//		int count = handZoneParent.childCount;
//		for (int i = 0; i < count; i++)
//		{
//			Transform card = handZoneParent.GetChild(i);
//			float posX = i * offsetX;
//			card.localPosition = new Vector3(posX, 0, 0);
//		}
//	}
//}

using UnityEngine;

public class HandManagerAI : MonoBehaviour
{
	public static HandManagerAI Instance;

	public Transform handParent;
	public float spacing = 1.5f;

	void Awake()
	{
		Instance = this;
	}

	public void ArrangeHand()
	{
		for (int i = 0; i < handParent.childCount; i++)
		{
			Transform card = handParent.GetChild(i);
			card.localPosition = new Vector3(i * spacing, 0, 0);
		}
	}
}

