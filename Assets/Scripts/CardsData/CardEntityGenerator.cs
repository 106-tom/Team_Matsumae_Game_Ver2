#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using UnityEngine;

public class CardEntityGenerator
{
	[MenuItem("Tools/Generate Card Entities")]
	public static void GenerateEntities()
	{
		// JSONì«Ç›çûÇ›
		TextAsset jsonFile = Resources.Load<TextAsset>("Cards/card_data");
		if (jsonFile == null)
		{
			Debug.LogError("card_data.json Ç™å©Ç¬Ç©ÇËÇ‹ÇπÇÒ");
			return;
		}

		CardData[] cardArray = JsonHelper.FromJson<CardData>(jsonFile.text);

		// ï€ë∂êÊ
		string folderPath = "Assets/Resources/CardEntityList";
		if (!Directory.Exists(folderPath))
		{
			Directory.CreateDirectory(folderPath);
		}

		foreach (var data in cardArray)
		{
			CardEntity entity = ScriptableObject.CreateInstance<CardEntity>();
			entity.Init(data);

			string assetPath = $"{folderPath}/CardEntity_{data.cardID}.asset";

			if (!File.Exists(assetPath))
			{
				AssetDatabase.CreateAsset(entity, assetPath);
			}
			else
			{
				CardEntity existing = AssetDatabase.LoadAssetAtPath<CardEntity>(assetPath);
				existing.Init(data);
				EditorUtility.SetDirty(existing);
			}
		}

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		Debug.Log("CardEntity asset generation complete.");
	}
}
#endif
