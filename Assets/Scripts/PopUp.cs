using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ポップアップ演出クラス
/// </summary>
public class PopUp : MonoBehaviour
{
    [Header("ポップアップ時のエフェクト")]
    [SerializeField] private ParticleSystem popUpEffect;
    [Space(10)]

    [Header("スプライト移動")]
    [SerializeField] private Vector3 startPosition;
    [SerializeField] private Vector3 endPosition;
    [SerializeField] private float moveTime = 0.2f;
    [Space(10)]

    [Header("フェードイン,アウトにかける時間")]
    [SerializeField] private float fadeInTime  = 0.2f;
    [SerializeField] private float fadeOutTime = 0.4f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            StartCoroutine(StartPopUp());
        }
    }

    /// <summary>
    /// ポップアップ演出開始
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartPopUp()
    {
        SpriteHelper spriteHelper = GetComponent<SpriteHelper>();
        MathHelper mathHelper = GetComponent<MathHelper>();

        // スプライトを透明にする
        SpriteRenderer targetSprite = GetComponent<SpriteRenderer>();
        targetSprite.color = new Color(targetSprite.color.r, targetSprite.color.g, targetSprite.color.b, 0f);

        transform.position = startPosition;

        Vector3 endScale = new Vector3(3f, 0f, 0f);

        // 画面中央に移動
        StartCoroutine(mathHelper.Move(moveTime, startPosition, endPosition));

        // フェードイン
        yield return StartCoroutine(spriteHelper.SpriteFadeIn(targetSprite, fadeInTime));

        // 少し待機
        yield return new WaitForSeconds(0.7f);

        // エフェクト再生
        ParticleSystem popUpEffectInstance = Instantiate(
                popUpEffect,
                new Vector3(0f, 0f, 0f),
                Quaternion.identity);
        popUpEffectInstance.Play();

        // 横に大きく伸ばしていく
        StartCoroutine(mathHelper.Scaling(fadeOutTime, transform.localScale, endScale));

        // フェードアウト
        yield return StartCoroutine(spriteHelper.SpriteFadeOut(targetSprite, fadeOutTime));

        // エフェクト削除
        Destroy(popUpEffectInstance.gameObject, popUpEffectInstance.main.duration);
    }
}
