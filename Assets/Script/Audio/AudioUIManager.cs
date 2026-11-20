using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// オーディオ関係のＵＩをまとめて管理するクラス
/// </summary>
public class AudioUIManager : MonoBehaviour
{
    public static AudioUIManager Instance { get; private set; }

    [SerializeField] private AudioVolumeSlider masterVolumeSlider;
    [SerializeField] private AudioVolumeSlider bgmVolumeSlider;
    [SerializeField] private AudioVolumeSlider seVolumeSlider;

    public AudioVolumeSlider MasterVolumeSlider => masterVolumeSlider;
    public AudioVolumeSlider BGMVolumeSlider => bgmVolumeSlider;
    public AudioVolumeSlider SEVolumeSlider => seVolumeSlider;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        masterVolumeSlider.SetSliderStats(AudioManager.Instance.AudioVolumeSettings.MasterVolume);
        bgmVolumeSlider.SetSliderStats(AudioManager.Instance.AudioVolumeSettings.BGMVolume);
        seVolumeSlider.SetSliderStats(AudioManager.Instance.AudioVolumeSettings.SEVolume);
    }
}
