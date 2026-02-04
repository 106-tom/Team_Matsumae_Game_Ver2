using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class ResultUI : MonoBehaviour
{
    public static ResultUI Instance;

    [SerializeField] GameObject panel;
    [SerializeField] Image resultImage;
    [SerializeField] Sprite winSprite;
    [SerializeField] Sprite loseSprite;

    bool canClick = false;

    private void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void ShowResult(bool isWin)
    {
        panel.SetActive(true);
        resultImage.sprite = isWin ? winSprite : loseSprite;

        Time.timeScale = 0f; // ÉQÅ[ÉÄí‚é~
        StartCoroutine(EnableClick());
    }

    IEnumerator EnableClick()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        canClick = true;
    }

    void Update()
    {
        if (!canClick) return;

        if (Input.GetMouseButtonDown(0))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("Title");
        }
    }
}
