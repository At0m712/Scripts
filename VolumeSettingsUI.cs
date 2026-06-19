using UnityEngine;
using UnityEngine.UI;

public class VolumeSettingsUI : MonoBehaviour
{
    public Slider musicSlider;
    public Slider sfxSlider;

    void Start()
    {
        // Charge les préférences du joueur (ou met à 100% par défaut)
        float savedMusicVol = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float savedSFXVol = PlayerPrefs.GetFloat("SFXVolume", 1f);

        musicSlider.value = savedMusicVol;
        sfxSlider.value = savedSFXVol;

        // Met à jour l'audio au démarrage
        SetMusicVolume(savedMusicVol);
        SetSFXVolume(savedSFXVol);

        // Ajoute des "écouteurs" pour réagir quand le joueur bouge le curseur
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetMusicVolume(float volume)
    {
        if (AudioManager.Instance != null && AudioManager.Instance.musicSource != null)
        {
            AudioManager.Instance.musicSource.volume = volume;
        }
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        if (AudioManager.Instance != null && AudioManager.Instance.sfxSource != null)
        {
            AudioManager.Instance.sfxSource.volume = volume;
        }
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }
}