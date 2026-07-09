using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// オーディオ関係をまとめて管理するクラス
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioMixer audioMixer;  // オーディオミキサー
    [SerializeField] private AudioDatabase database; // サウンドクリップがまとめられたSO

    [SerializeField] private AudioVolumeSettings volumeSettings;    // 音量設定関係のクラス
    public AudioVolumeSettings AudioVolumeSettings => volumeSettings;

    private AudioClipProvider clipProvider; // オーディオクリップ取得用クラス

    private AudioSource bgmSource;  // BGM用オーディオソース
    private AudioSource seSource;   // SE用オーディオソース

    private void OnDestroy()
    {
        if (Instance != null)
        {
            Instance = null;
        }
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        if (bgmSource == null)
        {
            bgmSource = transform.Find("BGMSource").GetComponent<AudioSource>();
        }
        if (seSource == null)
        {
            seSource = transform.Find("SESource").GetComponent<AudioSource>();
        }

        // BGMのオーディオソースの設定
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;

        // SEのオーディオソースの設定
        seSource.playOnAwake = false;

        // 各クラスのインスタンスを作成
        clipProvider = new AudioClipProvider(database);
        volumeSettings = new AudioVolumeSettings(audioMixer, AudioUIManager.Instance);
    }

    // BGMを再生する関数
    public void PlayBGM(BGMType type, float volume = 1, float pitch = 1, float delay = 0)
    {
        var clip = clipProvider.GetBGMClip(type);  // プロバイダーから対象のオーディオクリップを取得
        if (clip == null)
        {
            Debug.LogError("対象のAudioClipがnullか参照先が見つかりません");
            return;
        }
        else
        {
            if (bgmSource == null)
            {
                Debug.LogError("BGM用のAudioSourceが見つかりません");
                return;
            }

            // 対象のクリップをセット
            bgmSource.clip = clip;

            // 基本どちらも1で再生される(音量調整はオーディオミキサーで行っているので)
            bgmSource.volume = volume;
            bgmSource.pitch = pitch;

            // ディレイが必要な場合は、遅延してBGMを再生
            if (delay > 0)
            {
                bgmSource.PlayDelayed(delay);
            }
            else
            {
                bgmSource.Play();
            }
        }
    }

    // 効果音を再生する関数
    public void PlaySE(SEType type, float volume = 1, float pitch = 1, float delay = 0)
    {
        var clip = clipProvider.GetSEClip(type);  // プロバイダーから対象のオーディオクリップを取得
        if (clip == null)
        {
            Debug.LogError("対象のAudioClipがnullか参照先が見つかりません");
            return;
        }
        else
        {
            if (seSource == null)
            {
                Debug.LogError("SE用のAudioSourceが見つかりません");
                return;
            }

            // 基本どちらも1で再生される(音量調整はオーディオミキサーで行っているので)
            seSource.volume = volume;
            seSource.pitch = pitch;

            // ディレイが必要な場合は、遅延してSEを再生
            if (delay > 0)
            {
                StartCoroutine(PlaySE(clip, delay));
            }
            else
            {
                seSource.PlayOneShot(clip);
            }
        }

        // 次回効果音を鳴らす際に一応デフォルトの設定で鳴らすために初期化
        seSource.volume = 1;
        seSource.pitch = 1;
    }

    // BGMの一時停止
    public void PauseBGM()
    {
        bgmSource?.Pause();
    }

    // BGMの一時停止解除
    public void UnPauseBGM()
    {
        bgmSource?.UnPause();
    }

    // BGMを停止
    public void StopBGM()
    {
        bgmSource?.Stop();
    }

    private IEnumerator PlaySE(AudioClip clip, float delay = 0)
    {
        yield return new WaitForSeconds(delay);

        seSource.PlayOneShot(clip);
    }
}
