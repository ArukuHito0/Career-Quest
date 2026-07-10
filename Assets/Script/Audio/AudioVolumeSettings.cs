using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// オーディオの音量設定を管理するクラス
/// </summary>
[System.Serializable]
public class AudioVolumeSettings
{
    private AudioMixer audioMixer;

    [SerializeField, Range(0f, 1f)] private float masterVolume = 0.5f;
    [SerializeField, Range(0f, 1f)] private float bgmVolume = 0.5f;
    [SerializeField, Range(0f, 1f)] private float seVolume = 0.5f;

    public float MasterVolume => masterVolume;
    public float BGMVolume => bgmVolume;
    public float SEVolume => seVolume;

    private const string MASTER_VOLUME = "Master_Volume";
    private const string BGM_VOLUME = "BGM_Volume";
    private const string SE_VOLUME = "SE_Volume";

    public void SetMasterVolume(float volume)
    {
        SetVolume(MASTER_VOLUME, volume);
        masterVolume = volume;
    }

    public void SetBGMVolume(float volume)
    {
        SetVolume(BGM_VOLUME, volume);
        bgmVolume = volume;
    }

    public void SetSEVolume(float volume)
    {
        SetVolume(SE_VOLUME, volume);
        seVolume = volume;
    }

    // オーディオミキサーの音量設定を変更する
    private void SetVolume(string target, float volume)
    {
        if (volume <= 0f)
        {
            bool success = audioMixer.SetFloat(target, -80f);  // 無音
            if (success)
            {
                Debug.Log($"{target}のVolumeを{volume}に設定しました");
            }
            else
            {
                Debug.LogError($"{target}のVolumeの設定に失敗しました");
            }
        }
        else
        {
            bool success = audioMixer.SetFloat(target, Mathf.Log10(volume) * 20f);  // dbに変換
            if (success)
            {
                Debug.Log($"{target}のVolumeを{volume}に設定しました");
            }
            else
            {
                Debug.LogError($"{target}のVolumeの設定に失敗しました");
            }
        }
    }

    public AudioVolumeSettings(AudioMixer mixer, AudioUIManager manager)
    {
        audioMixer = mixer;

        manager.MasterSliderAddListner(SetMasterVolume);
        manager.BGMSliderAddListner(SetBGMVolume);
        manager.SESliderAddListner(SetSEVolume);
    }
}