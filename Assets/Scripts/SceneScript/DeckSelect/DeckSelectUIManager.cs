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

    private Outline currentOutline;

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

        var deckList = DeckDataManager.Instance.GetDeckListForDisplay();

        int viewCount = Mathf.Min(deckList.Count + 1, maxDeckViewCount);

        for (int i = 0; i < viewCount; i++)
        {
            if (i < deckList.Count)
                CreateDeckButton(deckList[i]);
            else
                CreateNewDeckButton();
        }
    }

    // =========================
    // Button Create
    // =========================
    void CreateDeckButton(string deckName)
    {
        GameObject obj = Instantiate(deckButtonPrefab, deckGridParent);
        Button button = obj.GetComponent<Button>();
        TMP_Text text = obj.GetComponentInChildren<TMP_Text>();
        Outline outline = obj.GetComponent<Outline>();

        text.text = deckName;

        if (outline != null)
            outline.enabled = false;

        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(() =>
        {
            OnSelectDeck(deckName, outline);
        });
    }
    void CreateNewDeckButton()
    {
        GameObject obj = Instantiate(deckButtonPrefab, deckGridParent);
        Button button = obj.GetComponent<Button>();
        TMP_Text text = obj.GetComponentInChildren<TMP_Text>();

        text.text = "＋ 新規作成";

        // 念のため Outline 無効
        Outline outline = obj.GetComponent<Outline>();
        if (outline != null)
            outline.enabled = false;

        button.onClick.AddListener(OnCreateNewDeck);
    }

    // =========================
    // Button Events
    // =========================
    void OnSelectDeck(string deckName, Outline outline)
    {
        Debug.Log("デッキ選択: " + deckName);

        // 前の赤枠を消す
        if (currentOutline != null)
            currentOutline.enabled = false;

        // 今回の赤枠をON
        if (outline != null)
        {
            outline.enabled = true;
            currentOutline = outline;
        }

        DeckDataManager.Instance.SelectDeck(deckName);
    }

    void OnCreateNewDeck()
    {
        Debug.Log("新規デッキ作成");
        // 前の赤枠を消す
        if (currentOutline != null)
            currentOutline.enabled = false;
        string deckName = DeckDataManager.Instance.CreateNewDeck();
        DeckDataManager.Instance.SelectDeck(deckName);

        //特例でシーン移動
        SceneManager.LoadScene("DeckEddit");
    }

    // =========================
    // Scene
    // =========================
    public void LoadEditScene()
    {
        if(IsAnyDeckSelected())
            SceneManager.LoadScene("DeckEddit");
    }
    public void LoadBattleScene()
    {
        if (IsAnyDeckSelected())
            SceneManager.LoadScene("GameAI");
    }
    public void LoadTitleScene()
    {
        SceneManager.LoadScene("Title");
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

    bool IsAnyDeckSelected()
    {
        return currentOutline != null && currentOutline.enabled;
    }
}