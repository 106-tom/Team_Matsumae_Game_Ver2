using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// エフェクトプールクラス
/// </summary>
public class EffectPoolManager : MonoBehaviour
{
    public static EffectPoolManager Instance { get; private set; }

    // プレファブをキーとしてエフェクト毎のプールを返してくれる
    private Dictionary<GameObject, IObjectPool<GameObject>> effectPools = new Dictionary<GameObject, IObjectPool<GameObject>>();

    private void Awake()
    {
        Instance = this;
    }

    // プールからエフェクト取得
    public GameObject Get(GameObject effectPrefab, Vector3 position)
    {
        //Debug.Log("プールを取得");
        // 指定したエフェクトのプールが無かったら作る
        if (!effectPools.ContainsKey(effectPrefab))
        {
            effectPools.Add(effectPrefab, CreateNewPool(effectPrefab));
        }

        // エフェクト取得
        GameObject effect = effectPools[effectPrefab].Get();
        effect.transform.localPosition = position;
        return effect;
    }


    private IObjectPool<GameObject> CreateNewPool(GameObject effectPrefab)
    {
        //Debug.Log("プールを生成");
        return new ObjectPool<GameObject>(
            createFunc: () =>
            {
                GameObject effect = Instantiate(effectPrefab);
                effect.SetActive(false);
                PooledEffect item = effect.GetComponent<PooledEffect>() ?? effect.AddComponent<PooledEffect>();
                item.Init(effectPools[effectPrefab]);
                return effect;
            },
            actionOnGet: effect => effect.SetActive(true),
            actionOnRelease: effect => effect.SetActive(false),
            actionOnDestroy: effect => Destroy(effect),
            collectionCheck: true,
            defaultCapacity: 10,
            maxSize: 100
            );
    }
}
