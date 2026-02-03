using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ManaManager : MonoBehaviour
{
    [SerializeField] private GameObject mana;
    [SerializeField] private ParticleSystem manaEffect;
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private float manaMoveDuration = 1.0f;

    private List<GameObject> manaObjectList = new List<GameObject>();
 
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            StartCoroutine(manaGenerateMove());
        }
    }

    // マナ生成処理
    private IEnumerator manaGenerateMove()
    {
        float elapsedTime = 0f;

        float length = 5.0f;

        Vector3 childEndPosition = new Vector3(0f, 0f, 0f);
     
        // 最後の子オブジェクトの取得
        int childCount = transform.childCount;
        if (childCount >= 1)
        {
            Transform lastChild = transform.GetChild(childCount - 1);

            // 移動の初期位置と最終位置の設定
            Vector3 childStartPosition = lastChild.position;
            childEndPosition = new Vector3(lastChild.position.x + length, 0f, 0f);

            Vector3 startPosition = transform.position;
            Vector3 endPosition = new Vector3(startPosition.x - length, 0f, 0f);

            // マナ移動
            while (elapsedTime < manaMoveDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / manaMoveDuration;
                float curveT = curve.Evaluate(t);

                transform.position = Vector3.Lerp(startPosition, endPosition, curveT);
                yield return null;
            }

            transform.position = endPosition;
        }

        // 新しいマナを生成
        GameObject newMana = Instantiate(
            mana,
            childEndPosition,
            Quaternion.identity,
            transform
            );
        manaObjectList.Add(newMana);

        // エフェクト再生
        ParticleSystem manaEffectInstance = Instantiate(
                manaEffect,
                childEndPosition,
                Quaternion.identity);
        manaEffectInstance.Play();
        Destroy(manaEffectInstance.gameObject, manaEffectInstance.main.duration);
        
        // リストに登録
        ManaUnit manaUnit = newMana.GetComponent<ManaUnit>();

        // 新しいマナの色を設定
        Renderer renderer = newMana.GetComponent<Renderer>();
        Material newMaterial = renderer.material;
        newMaterial.SetColor("_EmissionColor", manaUnit.afterUseEmissionColor);
        manaUnit.initializeMaterial(newMaterial);
        // マナの回復
        StartCoroutine(manaUnit.manaHeal());
    }
}
