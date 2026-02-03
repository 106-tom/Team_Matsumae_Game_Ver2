using Cinemachine;
using Effekseer;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// 破壊演出クラス
/// </summary>
public class Break : MonoBehaviour
{ 
    [Header("ディゾルブする際に邪魔なので非アクティブにする : 光沢用オブジェクト")]
    [SerializeField] private GameObject glassPlate;
    [Space(10)]

    [Header("エフェクト類")]
    [Tooltip("発光エフェクト")]
    [SerializeField] private ParticleSystem flashEffect;
    [Tooltip("円エフェクト")]
    [SerializeField] private EffekseerEffectAsset lingEffect;
    [Space(10)]

    [Header("画面振動用")]
    [Tooltip("振動の強さ")]
    [SerializeField] private float amplitude;
    [Tooltip("振動の速さ")]
    [SerializeField] private float frequency;
    [Tooltip("振動時間")]
    [SerializeField] private float shakeTime;
    [Space(10)]

    [Header("ディゾルブする時間")]
    [SerializeField] private float dissolveTime;

    [SerializeField] private FkingCardFXManager cardFXManager;

    [Header("破壊用オブジェクトのRigidBody")]
    [SerializeField] private Rigidbody[] rigidBodies;
    [Header("破壊用オブジェクトのRigidBody")]
    [SerializeField] private MeshRenderer[] renderers;

    private static readonly int colorID = Shader.PropertyToID("_Color");
    private static readonly int mainTexID = Shader.PropertyToID("_MainTex");

    /// <summary>
    /// 破壊演出開始
    /// </summary>
    /// <returns></returns>
    public IEnumerator StartBreak(Transform cardTransform)
    {
        Material[] materials = renderers.Select(r => r.material).ToArray();
        foreach (Material m in materials)
        {
            if (m.HasProperty(colorID)) m.SetColor(colorID, Color.black);
            if (m.HasProperty(mainTexID)) m.SetTexture(mainTexID, null);
        }
        cardFXManager.FadeCardOut(0f);
        
        // フラッシュエフェクト生成
        GameObject flashObj = EffectPoolManager.Instance.Get(flashEffect.gameObject, cardTransform.localPosition);
        ParticleSystem flashEffectInstance = flashObj.GetComponent<ParticleSystem>();

        // エフェクト再生
        Vector3 lingEffectPosition = new Vector3(cardTransform.localPosition.x, cardTransform.localPosition.y + 0.1f, cardTransform.localPosition.z);
        EffekseerHandle handle = EffekseerSystem.PlayEffect(lingEffect, lingEffectPosition);
        flashEffectInstance.Play();

        // 邪魔なので光沢用プレートを消す
        glassPlate.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        // カードをバラバラにする
        foreach (Rigidbody rb in rigidBodies)
        {
            rb.isKinematic = false;
        }

        // 画面振動開始
        CameraManager cameraManager = CameraManager.Instance;
        yield return StartCoroutine(cameraManager.CameraShake(0f, amplitude, frequency, shakeTime));
        yield return StartCoroutine(cameraManager.CameraShake(amplitude, 0f, frequency, shakeTime));

        yield return new WaitForSeconds(0.2f);

        // ディゾルブ開始
        float elapsedTime = 0f;
        while(elapsedTime < dissolveTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / dissolveTime;
            foreach(Material m in materials)
            {
                m.SetFloat("_Threshold", Mathf.Lerp(0.0f, 1.0f, t));
            }
            yield return null;
        }
    }
}
