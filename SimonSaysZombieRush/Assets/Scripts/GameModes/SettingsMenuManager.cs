using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
public class SettingsMenuManager : MonoBehaviour
{
    public Slider masterVol, musicVol,menuVol, sfxVol;
    public AudioMixer mainAudioMixer;

    private void Awake()
    {
        mainAudioMixer.SetFloat("MasterVol", -40);
        masterVol.value = -40;
        mainAudioMixer.SetFloat("MusicVol", 0);
        mainAudioMixer.SetFloat("SFXVol", 0);
        mainAudioMixer.SetFloat("MenuVol", 0);
    }

    public void ChangeMasterVolume()
    {
        mainAudioMixer.SetFloat("MasterVol", masterVol.value);
    }
    public void ChangeMusicVolume()
    {
        mainAudioMixer.SetFloat("MusicVol", Mathf.Log10(musicVol.value) * 20f);
    }
    public void ChangeSfxVolume()
    {
        mainAudioMixer.SetFloat("SFXVol", Mathf.Log10(sfxVol.value) * 20f);
    }
    public void ChangeMenuVolume()
    {
        mainAudioMixer.SetFloat("MenuVol", Mathf.Log10(menuVol.value) * 20f);
    }

}
