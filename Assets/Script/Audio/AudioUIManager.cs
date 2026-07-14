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

    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider seVolumeSlider;

    public Slider MasterVolumeSlider => masterVolumeSlider;
    public Slider BGMVolumeSlider => bgmVolumeSlider;
    public Slider SEVolumeSlider => seVolumeSlider;

    [SerializeField] private TextMeshProUGUI masterVolumeText;
    [SerializeField] private TextMeshProUGUI bgmVolumeText;
    [SerializeField] private TextMeshProUGUI seVolumeText;

    // 各音量調整スライダーにイベントを追加する用の関数
    public void MasterSliderAddListner(UnityAction<float> call) => MasterVolumeSlider?.onValueChanged.AddListener(call);
    public void BGMSliderAddListner(UnityAction<float> call) => BGMVolumeSlider?.onValueChanged.AddListener(call);
    public void SESliderAddListner(UnityAction<float> call) => SEVolumeSlider?.onValueChanged.AddListener(call);

    private void OnDestroy()
    {
        if (Instance != null)
        {
            Instance = null;
        }
    }

    private void Awake()
    {
        SetInstance();

        if (masterVolumeSlider == null)
        {
            masterVolumeSlider = GameObject.Find("MasterVolumeSlider").GetComponent<Slider>();
        }
        if (bgmVolumeSlider == null)
        {
            bgmVolumeSlider = GameObject.Find("BGMVolumeSlider").GetComponent<Slider>();
        }
        if (seVolumeSlider == null)
        {
            seVolumeSlider = GameObject.Find("SEVolumeSlider").GetComponent<Slider>();
        }
    }

    private void Start()
    {
        MasterSliderAddListner(UpdateMasterVolumeText);
        BGMSliderAddListner(UpdateBGMVolumeText);
        SESliderAddListner(UpdateSEVolumeText);

        SetSliderValues(MasterVolumeSlider, AudioManager.Instance.AudioVolumeSettings.MasterVolume, 0, 1);
        SetSliderValues(BGMVolumeSlider, AudioManager.Instance.AudioVolumeSettings.BGMVolume, 0, 1);
        SetSliderValues(SEVolumeSlider, AudioManager.Instance.AudioVolumeSettings.SEVolume, 0, 1);
    }

    private void SetInstance()
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
    }

    public void UpdateMasterVolumeText(float volume) => UpdateVolumeText(masterVolumeText, volume);
    public void UpdateBGMVolumeText(float volume) => UpdateVolumeText(bgmVolumeText, volume);
    public void UpdateSEVolumeText(float volume) => UpdateVolumeText(seVolumeText, volume);

    private void UpdateVolumeText(TextMeshProUGUI text, float volume) => text.text = (volume * 100).ToString("F0") + " %";

    // 引数のスライダーの現在値、最小値、最大値を設定する
    public void SetSliderValues(Slider slider, float value, float minValue = 0, float maxValue = 1)
    {
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        slider.value = value;
    }
}
