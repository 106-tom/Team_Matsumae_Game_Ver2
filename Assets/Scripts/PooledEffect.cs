using ExitGames.Client.Photon;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// エフェクトをプールから出し入れするためのクラス
/// </summary>
public class PooledEffect : MonoBehaviour
{
    private IObjectPool<GameObject> effectPool;
    private ParticleSystem ps;

    public void Init(IObjectPool<GameObject> pool)
    {
        effectPool = pool;
        ps = GetComponent<ParticleSystem>();
    }
    private void OnEnable()
    {
        if (ps != null)
        {
            // パーティクルの再生が終わる時間に返却
            Invoke(nameof(Release), ps.main.duration + ps.main.startLifetime.constantMax);
        }
    }

    private void Release()
    {
        // マネージャーを介さず直接自分のプールへ返却
        effectPool.Release(gameObject);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }
}
