using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AudioVolumeSlider : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TextMeshProUGUI volumeText;

    public Slider VolumeSlider => volumeSlider;

    public void SetSliderStats(float value, float minValue = 0, float maxValue = 1)
    {
        volumeSlider.minValue = minValue;
        volumeSlider.maxValue = maxValue;
        volumeSlider.value = value;
    }

    private void Start()
    {
        VolumeSliderAddLister(UpdateVolumeText);
    }

    public void VolumeSliderAddLister(UnityAction<float> call) => VolumeSlider?.onValueChanged.AddListener(call);
    private void UpdateVolumeText(float volume) => volumeText.text = (volume * 100).ToString("F0") + " %";
}