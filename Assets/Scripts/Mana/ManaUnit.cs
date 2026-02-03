using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaUnit : MonoBehaviour
{
    public Color baseEmissionColor = Color.red; // マナの元の色
    public Color afterUseEmissionColor  = Color.red; // 使用後マナの色

    public float emissionIntensity = 1.0f; // エミッション強度
    public float manaHealTime  = 2.0f; // マナ回復時間
    public float manaUseDuration   = 1.0f; // マナ使用時間

    private Material material; // エミッション用マテリアル

    private bool isUse = false; // マナ使用済みフラグ

    private Color healEmissiveColor = Color.white; // 回復時の発光最終色
    private Color healColor  = Color.white; // 回復時の最終色
    private Color afterUseColor   = Color.white; // 使用時の最終色

    // Start is called before the first frame update
    void Start()
    {
        isUse = false;
        healColor = baseEmissionColor * emissionIntensity;
        healEmissiveColor = baseEmissionColor * 5.0f;
        afterUseColor = afterUseEmissionColor;
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isUse)
        {
            // マナ回復
            StartCoroutine(manaHeal());
        }

        if (Input.GetKeyDown(KeyCode.Space) && !isUse)
        {
            // マナ使用
            StartCoroutine(manaUse());
        }
    }
    
    // マテリアル初期化
    public void initializeMaterial(Material newMaterial)
    {
        material = newMaterial;
    }

    public IEnumerator changeColor(Color endColor)
    {
        float elapsedTime = 0f;
        Color activeColor = material.GetColor("_EmissionColor"); // 現在の色

        while (elapsedTime < manaHealTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / manaHealTime;

            // 開始色から目標色へ滑らかに変化させる
            Color currentColor = Color.Lerp(activeColor, endColor, t);
            material.SetColor("_EmissionColor", currentColor);

            yield return null;
        }
    }

    // マナ回復処理
    public IEnumerator manaHeal()
    {
        float elapsedTime = 0f;
        Color activeColor = material.GetColor("_EmissionColor"); // 現在の色

        while (elapsedTime < manaHealTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / manaHealTime;

            // 開始色から目標色へ滑らかに変化させる
            Color currentColor = Color.Lerp(activeColor, healEmissiveColor, t);
            material.SetColor("_EmissionColor", currentColor);

            yield return null;
        }

        yield return StartCoroutine(changeColor(healColor));

        isUse = false;
    }

    // マナ使用処理
    private IEnumerator manaUse()
    {
        yield return StartCoroutine(changeColor(afterUseColor));
        
        isUse = true;
    }
}
