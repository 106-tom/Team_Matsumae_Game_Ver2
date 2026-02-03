using UnityEngine;

public static class CardVisualLayoutPreset
{
	// ==============================
	// ŽèŽD
	// ==============================
	public static readonly RectLayout Hand_CardIllust = new RectLayout
	{
		position = new Vector2(0f, 20f),
		size = new Vector2(330f, 400f),
		rotation = Vector3.zero
	};

	public static readonly RectLayout Hand_TextImage = new RectLayout
	{
		position = new Vector2(0f, -25f),
		size = new Vector2(330f, 450f),
		rotation = Vector3.zero
	};

	public static readonly RectLayout Hand_BattleFrame = new RectLayout
	{
		position = Vector2.zero,
		size = new Vector2(100f, 100f),
		rotation = Vector3.zero
	};

	public static readonly RectLayout Hand_Frame = Hand_BattleFrame;
	public static readonly RectLayout Hand_Red = new RectLayout
	{
		position = new Vector2(75f, 170f),
		size = new Vector2(35f, 35f),
		rotation = Vector3.zero
	};

	public static readonly RectLayout Hand_NameText = new RectLayout
	{
		position = new Vector2(70f, 185f),
		size = new Vector2(500f, 0f),
		rotation = Vector3.zero
	};

	public static readonly RectLayout Hand_Effect = new RectLayout
	{
		position = new Vector2(0f, -40f),
		size = new Vector2(300f, 0f),
		rotation = Vector3.zero
	};
}
