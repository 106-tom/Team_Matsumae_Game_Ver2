using UnityEngine;

public class BGMManager : MonoBehaviour
{
	public AudioSource audioSource; // AudioSourceをInspectorでセット
	public AudioClip bgmClip;       // MP3ファイル

	void Start()
	{
		audioSource.clip = bgmClip;
		audioSource.loop = true;
		audioSource.Play();

		Debug.Log("再生中? " + audioSource.isPlaying);
	}


	// 任意: 音量調整
	public void SetVolume(float vol)
	{
		audioSource.volume = Mathf.Clamp01(vol);
	}

	// 任意: 一時停止
	public void PauseBGM()
	{
		audioSource.Pause();
	}

	// 任意: 再開
	public void ResumeBGM()
	{
		audioSource.UnPause();
	}
}
