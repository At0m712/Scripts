using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Sources Audio")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Clips Audio UI")]
    public AudioClip clickSound;
    public AudioClip buySound;
    public AudioClip errorSound;
    public AudioClip cashSound; // Pour les récompenses

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayClick()
    {
        if (clickSound != null) sfxSource.PlayOneShot(clickSound);
    }

    public void PlayBuy()
    {
        if (buySound != null) sfxSource.PlayOneShot(buySound);
    }

    public void PlayError()
    {
        if (errorSound != null) sfxSource.PlayOneShot(errorSound);
    }

    public void PlayCash()
    {
        if (cashSound != null) sfxSource.PlayOneShot(cashSound);
    }
}