using UnityEngine;
using UnityEngine.UI;


public class StockPageController : MonoBehaviour
{
    [Header("Pages")]
    [SerializeField] GameObject redPage;
    [SerializeField] GameObject bluePage;
    [SerializeField] GameObject greenPage;
    [SerializeField] GameObject yellowPage;
    [SerializeField] GameObject purplePage;
    [SerializeField] GameObject whitePage;

    [Header("Buttons")]
    [SerializeField] Button redButton;
    [SerializeField] Button blueButton;
    [SerializeField] Button greenButton;
    [SerializeField] Button yellowButton;
    [SerializeField] Button purpleButton;
    [SerializeField] Button whiteButton;

    GameObject[] pages;
    Button[] buttons;

    [SerializeField] Color selectedColor = Color.white;
    [SerializeField] Color unselectedColor = new Color(0.6f, 0.6f, 0.6f, 1f);
    int currentIndex = 0;

    public int PageCount => pages.Length;
    public int CurrentIndex => currentIndex;

    void Awake()
    {
        // 全ページを配列にまとめる
        pages = new GameObject[]
        {
            redPage,
            bluePage,
            greenPage,
            yellowPage,
            purplePage,
            whitePage
        };

        buttons = new Button[]
        {
            redButton,
            blueButton,
            greenButton,
            yellowButton,
            purpleButton,
            whiteButton
        };
    }

    void Start()
    {
        ShowRed();
    }

    void ShowPage(GameObject targetPage, Button targetButton)
    {
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(pages[i] == targetPage);

            var image = buttons[i].GetComponent<Image>();
            image.color = (buttons[i] == targetButton)
                ? selectedColor
                : unselectedColor;
        }
    }
    public void ShowPageByIndex(int index)
    {
        currentIndex = (index + pages.Length) % pages.Length;

        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(i == currentIndex);

            var image = buttons[i].GetComponent<Image>();
            image.color = (i == currentIndex)
                ? selectedColor
                : unselectedColor;
        }
    }
    // ===== ボタン用 =====
    public void ShowRed() => ShowPageByIndex(0);
    public void ShowBlue() => ShowPageByIndex(1);
    public void ShowGreen() => ShowPageByIndex(2);
    public void ShowYellow() => ShowPageByIndex(3);
    public void ShowPurple() => ShowPageByIndex(4);
    public void ShowWhite() => ShowPageByIndex(5);
}
