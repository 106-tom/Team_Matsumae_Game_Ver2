using UnityEngine;
using UnityEditor;
using System.IO;

public class CardEntityGenerator
{
    [MenuItem("Tools/Generate Card Entities")]
    public static void GenerateEntities()
    {
        // JSON“Ç‚İ‚İ
        TextAsset jsonFile = Resources.Load<TextAsset>("Cards/card_data");
        if (jsonFile == null)
        {
            Debug.LogError("card_data.json ‚ªŒ©‚Â‚©‚è‚Ü‚¹‚ñ");
            return;
        }

        CardData[] cardArray = JsonHelper.FromJson<CardData>(jsonFile.text);

        // •Û‘¶æ
        string folderPath = "Assets/Resources/CardEntityList";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        foreach (var data in cardArray)
        {
            // ScriptableObject ì¬
            CardEntity entity = ScriptableObject.CreateInstance<CardEntity>();
            entity.Init(data);

            string assetPath = $"{folderPath}/CardEntity_{data.cardID}.asset";

            // ã‘‚«‰ñ”ğ
            if (!File.Exists(assetPath))
            {
                AssetDatabase.CreateAsset(entity, assetPath);
            }
            else
            {
                // Šù‘¶ƒf[ƒ^‚ğã‘‚«
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
