using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// メインキャンバスのシングルトンクラス
/// </summary>
public class EffectCanvas : MonoBehaviour
{
    public static EffectCanvas Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI GuardText;

    public Transform canvasTransform => transform;

    public TextMeshProUGUI blockText => GuardText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }
}
