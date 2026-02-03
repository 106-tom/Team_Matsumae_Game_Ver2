using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// エフェクト補助クラス
/// </summary>
public class CardEffectHelper : MonoBehaviour
{
    /// <summary>
    /// カード光沢演出
    /// </summary>
    /// <param name="mainCardIDDisplay">レンダーテクスチャに表示する絵柄</param>
    /// <param name="UICamera">レンダーテクスチャ用カメラ</param>
    /// <param name="glassPlate">光沢演出用プレート</param>
    /// <param name="renderTexture">絵柄を表示するレンダーテクスチャ</param>
    /// <param name="time">光沢演出時間</param>
    /// <returns></returns>
    public IEnumerator PlayShineEffect(
        GameObject mainCardIDDisplay,
        GameObject UICamera,
        GameObject glassPlate,
        RenderTexture renderTexture,
        FkingCardFXManager cardFXManager,
        float time,
        Color color)
    {
        GameObject camera;
        GameObject cardVisual;
        Material cardMaterial;

        // レンダーテクスチャの絵柄を設定
        SettingRenderTecture(mainCardIDDisplay, UICamera, renderTexture, cardFXManager,
            out glassPlate, out camera, out cardVisual, out cardMaterial);

        // 光らせるかどうかのフラグを設定
        int isShiningID = Shader.PropertyToID("_IsShining");
        float effectTimer = 0;
        cardMaterial.SetFloat(isShiningID, 1);

        // 光沢の色を変更
        Color buffColor = color;
        int shineColorID = Shader.PropertyToID("_ShineColor");
        cardMaterial.SetColor(shineColorID, buffColor);

        int effectTimeID = Shader.PropertyToID("_EffectTime");
        // 光沢演出開始
        while (effectTimer < time)
        {
            // 時間によって光を動かす
            effectTimer += Time.deltaTime;
            float t = effectTimer / time;
            cardMaterial.SetFloat(effectTimeID, t);

            yield return null;
        }
        // プレートを元の位置に戻す
        glassPlate.transform.localPosition = new Vector3(0f, 0f, -0.03f);
        cardMaterial.SetFloat(isShiningID, 0);
        cardMaterial.SetFloat(effectTimeID, 0f);

        Destroy(camera.gameObject);
        Destroy(cardVisual.gameObject);
    }

    /// <summary>
    /// レンダーテクスチャの絵柄を設定
    /// </summary>
    /// <param name="mainCardIDDisplay"></param>
    /// <param name="UICamera"></param>
    /// <param name="renderTexture"></param>
    /// <param name="glassPlate"></param>
    /// <param name="camera"></param>
    /// <param name="cardVisual"></param>
    /// <param name="cardMaterial"></param>

    public void SettingRenderTecture(
        GameObject mainCardIDDisplay,
        GameObject UICamera,
        RenderTexture renderTexture,
        FkingCardFXManager cardFXManager,
        out GameObject glassPlate,
        out GameObject camera,
        out GameObject cardVisual,
        out Material cardMaterial
        )
    {
        // RenderTexture用カメラ作成
        camera = Instantiate(UICamera);

        // RenderTexture表示用カード生成
        Quaternion rotation = Quaternion.Euler(90f, 0f, 0f);
        Vector3 cardPosition = new Vector3(0f, -3f, 0f);
        cardVisual = Instantiate(mainCardIDDisplay, cardPosition, rotation);
        cardVisual.transform.localScale = new Vector3(0.0008f, 0.001f, 1f);

        // 光沢演出用プレートを取得
        FkingCardFXManager card = cardFXManager;
        glassPlate = card.GetGlossPlate();

        // プレートの位置をカードよりも少し上に設定
        glassPlate.transform.localPosition = new Vector3(0f, 0f, -0.08f);

        // プレートのテクスチャを変更
        MeshRenderer renderer = glassPlate.GetComponent<MeshRenderer>();
        cardMaterial = renderer.material;
        int mainTexID = Shader.PropertyToID("_MainTex");
        cardMaterial.SetTexture(mainTexID, renderTexture);
    }
}
