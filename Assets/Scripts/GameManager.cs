using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Slider[] musicSliders;
    public Slider[] sfxSliders;

    private void Start()
    {
        InitializeVolumeSliders();
    }

    private void InitializeVolumeSliders()
    {
        foreach (Slider s in musicSliders)
        {
            s.onValueChanged.AddListener(SetMusicVolume);
        }
        foreach (Slider s in sfxSliders)
        {
            s.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    public void SetMusicVolume(float value)
    {
        SoundManager.Instance.SetParameter("MusicVolume", value);
        foreach (Slider s in musicSliders)
        {
            s.value = value;
        }
    }

    public void SetSFXVolume(float value)
    {
        SoundManager.Instance.SetParameter("SFXVolume", value);
        foreach (Slider s in sfxSliders)
        {
            s.value = value;
        }
    }
}
