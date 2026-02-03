using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    public CanvasGroup fader;     // 黒全画面Imageに CanvasGroup
    public float fadeDuration = 0.25f;
    static SceneTransition I;

    void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
        if (fader) { fader.alpha = 1f; StartCoroutine(Fade(0)); }
    }

    public static void Load(string sceneName)
    {
        if (I == null) { SceneManager.LoadScene(sceneName); return; }
        I.StartCoroutine(I.LoadRoutine(sceneName));
    }

    IEnumerator LoadRoutine(string sceneName)
    {
        yield return Fade(1);
        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        yield return null;               // 1フレーム待つ
        yield return Fade(0);
    }

    IEnumerator Fade(float target)
    {
        if (!fader) yield break;
        fader.blocksRaycasts = true;
        float start = fader.alpha;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            fader.alpha = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }
        fader.alpha = target;
        fader.blocksRaycasts = target > 0.9f; // 黒の時だけブロック
    }
}
