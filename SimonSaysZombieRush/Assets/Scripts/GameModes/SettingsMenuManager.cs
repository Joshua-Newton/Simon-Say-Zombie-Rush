using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
public class SettingsMenuManager : MonoBehaviour
{
    public Slider masterVol, musicVol,menuVol, sfxVol;
    public AudioMixer mainAudioMixer;

    public void ChangeMasterVolume()
    {
        mainAudioMixer.SetFloat("MasterVol", masterVol.value);
    }
    public void ChangeMusicVolume()
    {
        mainAudioMixer.SetFloat("MusicVol", musicVol.value);
    }
    public void ChangeSfxVolume()
    {
        mainAudioMixer.SetFloat("SFXVol", sfxVol.value);
    }
    public void ChangeMenuVolume()
    {
        mainAudioMixer.SetFloat("MenuVol", menuVol.value);
    }

}
