using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// オーディオ関係のＵＩをまとめて管理するクラス
/// </summary>
public class AudioUIManager : MonoBehaviour
{
    [SerializeField] private AudioVolumeSlider masterVolumeSlider;
    [SerializeField] private AudioVolumeSlider bgmVolumeSlider;
    [SerializeField] private AudioVolumeSlider seVolumeSlider;

    public AudioVolumeSlider MasterVolumeSlider => masterVolumeSlider;
    public AudioVolumeSlider BGMVolumeSlider => bgmVolumeSlider;
    public AudioVolumeSlider SEVolumeSlider => seVolumeSlider;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        masterVolumeSlider.SetSliderStats(AudioManager.Instance.AudioVolumeSettings.MasterVolume);
        bgmVolumeSlider.SetSliderStats(AudioManager.Instance.AudioVolumeSettings.BGMVolume);
        seVolumeSlider.SetSliderStats(AudioManager.Instance.AudioVolumeSettings.SEVolume);
    }
}
