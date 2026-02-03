using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// カットイン演出クラス
/// </summary>
public class CutIn : MonoBehaviour
{
    [Header("背景画像")]
    [SerializeField] private SpriteRenderer playerBackSpriteRenderer;
    [SerializeField] private SpriteRenderer enemyBackSpriteRenderer;
    [SerializeField] private GameObject characterIllust;
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI targetText;
    [SerializeField] private RectTransform rectTransform;
    [Space(10)]

    [Header("スプライト調整")]
    [Tooltip("スプライト拡大時のスケールと透明値調整時間")]
    [SerializeField] private float fadeInTime = 0.3f;
    [SerializeField] private float backScaleY = 1.0f;
    [Tooltip("スプライト縮小時の透明値調整時間")]
    [SerializeField] private float fadeOutTime = 0.3f;
    [SerializeField] private Vector3 characterIllustScale;
    [SerializeField] private Vector3 characterIllustStartPosition;
    [SerializeField] private Vector3 characterIllustEndPosition;
    [Space(10)]

    [Header("アニメーションを表示する時間")]
    [SerializeField] private float animationTime = 2f;

    [Header("イラストの移動速度変化グラフ")]
    [SerializeField] private AnimationCurve moveCurve;

    // 雷アニメーション、背景
    [SerializeField] private SpriteRenderer thunderSprite;
    [SerializeField] private SpriteRenderer characterRenderer;
    [SerializeField] private Animator animator;

    private SpriteRenderer backGroundSprite;

    private CardMotionHelper cardMotionHelper;
    private UIHelper uiHelper;

    private void Start()
    {
        cardMotionHelper = SystemManager.Instance.cardMotionHelper;
        uiHelper = SystemManager.Instance.uiHelper;
    }

    /// <summary>
    /// カットイン演出開始
    /// </summary>
    /// <param name="isPlayerTurn">プレイヤーのターンかどうか</param>
    /// <returns></returns>
    public IEnumerator StartCutIn(bool isPlayerTurn)
    {
        // 雷アニメーション再生
        animator.speed = 1f;
        animator.Play("CutInAnimation", 0, 0f);

        // SpriteRendererコンポーネント取得
        // 自分のカットイン
        backGroundSprite = isPlayerTurn ? playerBackSpriteRenderer : enemyBackSpriteRenderer;

        // スプライト拡大、透明値いじって表示
        Color endColor = new Color(1f, 1f, 1f, 1f);
        StartCoroutine(uiHelper.TextColoring(targetText, 0f, endColor));

        Vector3 endTextScale = new Vector3(rectTransform.localScale.x, 1, rectTransform.localScale.z);
        StartCoroutine(uiHelper.TextScaling(rectTransform, fadeInTime, endTextScale));

        // キャラクターイラスト、横から登場
        characterIllust.GetComponent<SpriteRenderer>().sprite = image.sprite;
        characterIllust.transform.localScale = characterIllustScale;
        StartCoroutine(cardMotionHelper.MoveTarget(characterIllust.transform, 0.1f, characterIllustStartPosition, characterIllustEndPosition, moveCurve));

        // フェードイン開始
        StartCoroutine(uiHelper.SpriteFadeIn(thunderSprite, fadeInTime));
        StartCoroutine(uiHelper.SpriteFadeIn(backGroundSprite, fadeInTime));
        StartCoroutine(uiHelper.SpriteFadeIn(characterRenderer, fadeInTime));

        // 縦に伸ばす
        Vector3 startScale = transform.localScale;
        Vector3 endScale = new Vector3(startScale.x, backScaleY, startScale.z);
        yield return StartCoroutine(cardMotionHelper.ScalingTarget(transform, fadeInTime, startScale, endScale));

        // AnimationTimeだけスプライトを表示する
        yield return new WaitForSeconds(animationTime);

        // スプライトを徐々に縮小する
        endTextScale = new Vector3(rectTransform.localScale.x, 0, rectTransform.localScale.z);
        StartCoroutine(uiHelper.TextScaling(rectTransform, fadeOutTime, endTextScale));
        endColor = new Color(1f, 1f, 1f, 0f);
        StartCoroutine(uiHelper.TextColoring(targetText, fadeOutTime, endColor));

        // 透明値をいじって非表示にする
        StartCoroutine(uiHelper.SpriteFadeOut(thunderSprite, fadeOutTime));
        StartCoroutine(uiHelper.SpriteFadeOut(backGroundSprite, fadeOutTime));
        StartCoroutine(uiHelper.SpriteFadeOut(characterRenderer, fadeOutTime));

        startScale = transform.localScale;
        endScale = new Vector3(startScale.x, 0f, startScale.z);
        StartCoroutine(cardMotionHelper.ScalingTarget(transform, fadeOutTime, startScale, endScale));

        startScale = characterIllust.transform.localScale;
        endScale = new Vector3(startScale.x, 0f, startScale.z);
        yield return StartCoroutine(cardMotionHelper.ScalingTarget(characterIllust.transform, fadeOutTime, startScale, endScale));

        animator.speed = 0f;
    }
}   
