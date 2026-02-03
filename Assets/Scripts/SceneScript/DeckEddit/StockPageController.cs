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

    [Header("Buttons")]
    [SerializeField] Button redButton;
    [SerializeField] Button blueButton;
    [SerializeField] Button greenButton;
    [SerializeField] Button yellowButton;
    [SerializeField] Button purpleButton;

    GameObject[] pages;
    Button[] buttons;

    [SerializeField] Color selectedColor = Color.white;
    [SerializeField] Color unselectedColor = new Color(0.6f, 0.6f, 0.6f, 1f);

    void Awake()
    {
        // 全ページを配列にまとめる
        pages = new GameObject[]
        {
            redPage,
            bluePage,
            greenPage,
            yellowPage,
            purplePage
        };

        buttons = new Button[]
        {
            redButton,
            blueButton,
            greenButton,
            yellowButton,
            purpleButton
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

    // ===== ボタン用 =====
    public void ShowRed()
    {
        ShowPage(redPage, redButton);
    }

    public void ShowBlue()
    {
        ShowPage(bluePage, blueButton);
    }

    public void ShowGreen()
    {
        ShowPage(greenPage, greenButton);
    }

    public void ShowYellow()
    {
        ShowPage(yellowPage, yellowButton);
    }

    public void ShowPurple()
    {
        ShowPage(purplePage, purpleButton);
    }

}
