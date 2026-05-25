using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    [Header("Audio")]
    public AudioMixer audioMixer;
    public string volumeParameter = "MasterVolume"; 

    [Header("UI")]
    public Slider slider;

    private void Start()
    {
        slider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float value)
    {
        float volume = Mathf.Log10(value) * 20;
        audioMixer.SetFloat(volumeParameter, volume);
    }
}