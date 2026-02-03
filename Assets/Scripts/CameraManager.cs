using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    static public CameraManager Instance { get; private set; }
    [SerializeField] private CinemachineVirtualCamera VirtualCamera;
    [SerializeField] private Camera MainCamera;
    public CinemachineVirtualCamera virtualCamera => VirtualCamera;
    public Camera mainCamera => MainCamera;
    public CinemachineBasicMultiChannelPerlin noise { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (noise == null)
            {
                noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            }
        }
    }

    /// <summary>
    /// ƒJƒƒ‰U“®
    /// </summary>
    /// <param name="startAmp"></param>
    /// <param name="endAmp"></param>
    /// <param name="freq"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    public IEnumerator CameraShake(
        float startAmp, float endAmp, float freq, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            noise.m_AmplitudeGain = Mathf.Lerp(startAmp, endAmp, t);
            noise.m_FrequencyGain = (endAmp > startAmp) ? Mathf.Lerp(0, freq, t) : freq; // •K—v‚É‰‚¶‚Ä’²®
            yield return null;
        }
        noise.m_AmplitudeGain = endAmp;
    }
}
