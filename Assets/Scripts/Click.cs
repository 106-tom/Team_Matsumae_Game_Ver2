using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Click : MonoBehaviour
{
    [SerializeField] private ParticleSystem clickEffect; // クリックエフェクト
    [SerializeField] private Vector3 effectRotate; // エフェクトの回転値

    // Update is called once per frame
    void Update()
    {
        // カーソル位置取得
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = 10.0f;
        Vector3 cursolPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        if (Input.GetMouseButtonDown(0))
        {
            // クリックエフェクト再生
            Quaternion rotate = Quaternion.Euler(effectRotate);
            ParticleSystem clickEffectInstance = Instantiate(
                clickEffect,
                cursolPosition,
                rotate);
            clickEffectInstance.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            clickEffectInstance.Play();
            // クリックエフェクト破棄
            Destroy(clickEffectInstance.gameObject, clickEffectInstance.main.duration);
        }
        transform.position = cursolPosition;
    }
}
