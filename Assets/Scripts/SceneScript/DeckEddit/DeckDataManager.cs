using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class DeckDataManager : MonoBehaviour
{
    public static DeckDataManager Instance { get; private set; }

    [Header("Edit Scene References")]
    DeckNameInput nameInput;
    DeckEditManager editManager;

    List<string> deckNames = new List<string>();

    const string SELECTED_DECK_KEY = "SelectedDeck";

    public int DeckCount => deckNames.Count;
    public string SelectedDeckName =>
        PlayerPrefs.GetString(SELECTED_DECK_KEY, "");

    string editingDeckName;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadDeckList();
    }

    // =========================
    // Deck Directory
    // =========================
    public string DeckDirectory
    {
        get
        {
            string docPath =
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            string dir = Path.Combine(
                docPath,
                "色彩の土地",
                "Decks"
            );

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            return dir;
        }
    }

    // =========================
    // Scene Register
    // =========================
    public void RegisterEditScene(DeckNameInput input, DeckEditManager manager)
    {
        nameInput = input;
        editManager = manager;
    }

    // =========================
    // Deck List
    // =========================
    public void LoadDeckList()
    {
        deckNames.Clear();

        foreach (var file in Directory.GetFiles(DeckDirectory, "*.json"))
        {
            var data = JsonUtility.FromJson<DeckSaveData>(
                File.ReadAllText(file)
            );

            // デッキ名が無い or 未保存なら除外
            if (string.IsNullOrEmpty(data.deckName))
                continue;

            deckNames.Add(data.deckName);
        }

        deckNames.Sort();
    }

    public string GetDeckName(int index)
    {
        if (index < 0 || index >= deckNames.Count)
            return string.Empty;
        return deckNames[index];
    }

    // =========================
    // Select
    // =========================
    public void SelectDeck(string deckName)
    {
        PlayerPrefs.SetString(SELECTED_DECK_KEY, deckName);
        PlayerPrefs.Save();
        editingDeckName = deckName;
    }

    // =========================
    // Create
    // =========================
    public string CreateNewDeck()
    {
        string baseName = "NewDeck";
        string deckName = GetUniqueDeckName(baseName);

        deckNames.Add(deckName);
        SelectDeck(deckName);

        if (nameInput != null)
            nameInput.SetDeckName(deckName);
        else
            Debug.LogWarning("CreateNewDeck: nameInput が null です。RegisterEditScene を確認してください。");

        return deckName;
    }

    // =========================
    // Delete
    // =========================
    void DeleteDeck(string deckName)
    {
        if (string.IsNullOrEmpty(deckName))
        {
            Debug.LogWarning("DeleteDeck: deckName が空");
            return;
        }
        string path = Path.Combine(DeckDirectory, deckName + ".json");

        Debug.Log("削除試行: " + path);

        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("ファイル削除成功: " + deckName);
        }
        else
        {
            Debug.LogWarning("削除対象が存在しない: " + path);
        }

        deckNames.Remove(deckName);
    }



    // =========================
    // Save / Load
    // =========================
    public void SaveDeckSmart()
    {
        if (editManager == null || nameInput == null)
        {
            Debug.LogError("EditScene が未登録です");
            return;
        }

        string inputName = nameInput.GetDeckName();   // UIの名前
        string editingName = SelectedDeckName;        // 元のデッキ名

        // 名前変更時の重複チェック
        if (!string.IsNullOrEmpty(editingName) && inputName != editingName)
        {
            string uniqueName = GetUniqueDeckName(inputName);

            if (uniqueName != inputName)
            {
                // 重複時は保存せず警告ログ
                Debug.LogWarning($"保存失敗: '{inputName}' は既に存在します。");
                // UI側で名前を元に戻す
                inputName = uniqueName;
                nameInput.SetDeckName(uniqueName);
            }

            // 古いファイル削除
            DeleteDeck(editingName);
        }

        // 新しい名前で保存
        SaveDeckInternal(inputName, editManager.GetDeckCardIds());

        // 選択中デッキを更新
        SelectDeck(inputName);

        // リスト更新
        LoadDeckList();

        Debug.Log($"デッキ保存（上書き）: {inputName}");
    }



    void SaveDeckInternal(string deckName, List<int> cardIds)
    {
        var data = new DeckSaveData
        {
            deckName = deckName,
            cardIds = cardIds
        };

        string path = Path.Combine(
            DeckDirectory,
            deckName + ".json"
        );

        File.WriteAllText(path, JsonUtility.ToJson(data, true));
    }

    public void LoadSelectedDeck()
    {
        if (string.IsNullOrEmpty(SelectedDeckName))
        {
            Debug.LogWarning("選択中デッキなし");
            return;
        }

        LoadDeck(SelectedDeckName);
    }

    public void LoadDeck(string deckName)
    {
        if (editManager == null || nameInput == null)
        {
            Debug.LogError("EditScene が未登録です");
            return;
        }

        string path = Path.Combine(
            DeckDirectory,
            deckName + ".json"
        );

        if (!File.Exists(path))
        {
            Debug.LogError("デッキが存在しません: " + deckName);
            return;
        }

        var data = JsonUtility.FromJson<DeckSaveData>(
            File.ReadAllText(path)
        );

        nameInput.SetDeckName(data.deckName);
        editManager.LoadFromData(data);

        Debug.Log("デッキロード: " + data.deckName);
    }

    // =========================
    // Folder Open
    // =========================
    public void OpenDeckFolder()
    {
        Application.OpenURL(DeckDirectory);
    }

    // =========================
    // Get Deck Name
    // =========================

    string GetUniqueDeckName(string baseName)
    {
        if (!deckNames.Contains(baseName))
            return baseName;

        int i = 1;
        string newName;
        do
        {
            newName = $"{baseName}({i})";
            i++;
        } while (deckNames.Contains(newName));

        return newName;
    }

    // =========================
    // Helper
    // =========================


}
