using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
public class SettingsMenuManager : MonoBehaviour
{
    public Slider masterVol, musicVol,menuVol, sfxVol;
    public AudioMixer mainAudioMixer;

    public void ChangeMasterVolume()
    {
        mainAudioMixer.SetFloat("MasterVol", Mathf.Log10(masterVol.value) * 20f);
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
