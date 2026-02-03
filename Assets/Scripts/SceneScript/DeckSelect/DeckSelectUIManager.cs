using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class DeckSelectUIManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] Transform deckGridParent;   // GridLayoutGroup が付いた親
    [SerializeField] GameObject deckButtonPrefab; // デッキボタンPrefab
    [SerializeField] int maxDeckViewCount = 9;

    void Start()
    {
        // 使わない
    }
    void OnEnable()
    {
        if (!IsValid())
            return;

        if (DeckDataManager.Instance == null)
        {
            Debug.LogError("DeckDataManager が存在しません");
            return;
        }

        DeckDataManager.Instance.LoadDeckList();
        Refresh();
    }

    // =========================
    // UI Refresh
    // =========================
    public void Refresh()
    {

        ClearButtons();

        int deckCount = DeckDataManager.Instance.DeckCount;
        int viewCount = Mathf.Min(deckCount + 1, maxDeckViewCount);

        for (int i = 0; i < viewCount; i++)
        {
            if (i < deckCount)
                CreateDeckButton(i);
            else
                CreateNewDeckButton();
        }
    }

    // =========================
    // Button Create
    // =========================
    void CreateDeckButton(int index)
    {
        string deckName = DeckDataManager.Instance.GetDeckName(index);

        GameObject obj = Instantiate(deckButtonPrefab, deckGridParent);
        Button button = obj.GetComponent<Button>();
        TMP_Text text = obj.GetComponentInChildren<TMP_Text>();

        text.text = deckName;
        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(() =>
        {
            OnSelectDeck(deckName);
        });
    }
    void CreateNewDeckButton()
    {
        GameObject obj = Instantiate(deckButtonPrefab, deckGridParent);
        Button button = obj.GetComponent<Button>();
        TMP_Text text = obj.GetComponentInChildren<TMP_Text>();

        text.text = "＋ 新規作成";

        button.onClick.AddListener(OnCreateNewDeck);
    }

    // =========================
    // Button Events
    // =========================
    void OnSelectDeck(string deckName)
    {
        Debug.Log("デッキ選択: " + deckName);

        DeckDataManager.Instance.SelectDeck(deckName);
    }

    void OnCreateNewDeck()
    {
        Debug.Log("新規デッキ作成");

        string deckName = DeckDataManager.Instance.CreateNewDeck();
        DeckDataManager.Instance.SelectDeck(deckName);

        LoadEditScene();
    }

    // =========================
    // Scene
    // =========================
    public void LoadEditScene()
    {
        SceneManager.LoadScene("DeckEddit");
    }
    public void LoadBattleScene()
    {
        SceneManager.LoadScene("GameAI");
    }

    // =========================
    // Utility
    // =========================
    void ClearButtons()
    {
        foreach (Transform child in deckGridParent)
            Destroy(child.gameObject);
    }

    bool IsValid()
    {
        if (deckGridParent == null || deckButtonPrefab == null)
        {
            Debug.LogError("DeckSelectUIManager の Inspector 設定が不足しています");
            return false;
        }
        return true;
    }
}