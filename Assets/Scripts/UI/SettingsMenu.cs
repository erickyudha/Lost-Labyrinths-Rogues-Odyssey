using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;

public class SettingsMenu : MonoBehaviour
{
    public AudioMixer mainMixer;
    public TMP_Dropdown resDropdown;
    private Resolution[] resArray;
    void Start()
    {
        resArray = Screen.resolutions;
        List<string> options = new();
        
        int currentResolutionIndex = 0;
        for (int i = 0; i < resArray.Length; i++)
        {
            options.Add(resArray[i].width + " x " + resArray[i].height);

            if (resArray[i].width == Screen.currentResolution.width
                && resArray[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resDropdown.ClearOptions();
        resDropdown.AddOptions(options);
        resDropdown.value = currentResolutionIndex;
        resDropdown.RefreshShownValue();
    }
    // Convert linear slider value to logarithmic scale (from 0 to 1)
    private float ConvertToLogarithmicScale(float value)
    {
        float minVolume = -30f; // Minimum volume in dB
        float maxVolume = 0f;   // Maximum volume in dB

        float volume = Mathf.Lerp(minVolume, maxVolume, value); // Map logarithmic value to volume range

        if (value <= 0.01f)
        {
            volume = -80f;
        }
        return volume;
    }

    public void SetMasterVolume(float volume)
    {
        volume = ConvertToLogarithmicScale(volume);
        mainMixer.SetFloat("masterVolume", volume);
    }

    public void SetBGMVolume(float volume)
    {
        volume = ConvertToLogarithmicScale(volume);
        mainMixer.SetFloat("bgmVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        volume = ConvertToLogarithmicScale(volume);
        mainMixer.SetFloat("sfxVolume", volume);
    }

    public void SetGraphicQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resArray[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
}
