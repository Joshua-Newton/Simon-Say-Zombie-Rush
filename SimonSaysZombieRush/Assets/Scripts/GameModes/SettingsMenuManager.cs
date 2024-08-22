using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
public class SettingsMenuManager : MonoBehaviour
{
    [Range(0, 1)] public static float defaultMasterVol = 0.5f;
    [Range(0, 1)] public static float defaultMusicVol = 0.5f;
    [Range(0, 1)] public static float defaultSFXVol = 0.5f;
    [Range(0, 1)] public static float defaultMenuVol = 0.5f;

    public Slider masterVol, musicVol,menuVol, sfxVol;
    public AudioMixer mainAudioMixer;

    public const string MASTER_VOLUME_NAME = "MasterVol";
    public const string MUSIC_VOLUME_NAME = "MusicVol";
    public const string SFX_VOLUME_NAME = "SFXVol";
    public const string MENU_VOLUME_NAME = "MenuVol";

    private void Awake()
    {
        masterVol.value = PlayerPrefs.GetFloat(MASTER_VOLUME_NAME, defaultMasterVol);
        musicVol.value = PlayerPrefs.GetFloat(MUSIC_VOLUME_NAME, defaultMusicVol);
        sfxVol.value = PlayerPrefs.GetFloat(SFX_VOLUME_NAME, defaultSFXVol);
        menuVol.value = PlayerPrefs.GetFloat(MENU_VOLUME_NAME, defaultMenuVol);
    }

    public void ChangeMasterVolume()
    {
        mainAudioMixer.SetFloat(MASTER_VOLUME_NAME, SliderValToDecibels(masterVol.value));
        PlayerPrefs.SetFloat(MASTER_VOLUME_NAME, masterVol.value);
    }
    public void ChangeMusicVolume()
    {
        mainAudioMixer.SetFloat(MUSIC_VOLUME_NAME, SliderValToDecibels(musicVol.value));
        PlayerPrefs.SetFloat(MUSIC_VOLUME_NAME, musicVol.value);
    }
    public void ChangeSfxVolume()
    {
        mainAudioMixer.SetFloat(SFX_VOLUME_NAME, SliderValToDecibels(sfxVol.value));
        PlayerPrefs.SetFloat(SFX_VOLUME_NAME, sfxVol.value);
    }
    public void ChangeMenuVolume()
    {
        mainAudioMixer.SetFloat(MENU_VOLUME_NAME, SliderValToDecibels(menuVol.value));
        PlayerPrefs.SetFloat(MENU_VOLUME_NAME, menuVol.value);
    }

    public static float SliderValToDecibels(float val)
    {
        return (Mathf.Log10(val) * 20);
    }

}
