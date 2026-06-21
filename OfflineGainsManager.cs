using UnityEngine;
using TMPro;
using System;
using UnityEngine.Localization; // INDISPENSABLE POUR LA TRADUCTION

public class OfflineGainsManager : MonoBehaviour
{
    public static OfflineGainsManager Instance;

    [Header("Configuration UI")]
    public GameObject popupOffline; 
    public TextMeshProUGUI texteGains;
    
    [Header("Localisation")]
    [Tooltip("Clé FR ex: Tes Sorciers ont travaillé ! Tu as récolté :\n<color=#FFD700>{0} Mana</color>")]
    public LocalizedString textePopupOffline;

    private double gainsEnAttente = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void AfficherGainsOffline(double tempsDeconnecteEnSecondes)
    {
        if (GameManager.Instance == null || GameManager.Instance.manaPerSecond <= 0) return;

        if (tempsDeconnecteEnSecondes >= 60)
        {
            gainsEnAttente = tempsDeconnecteEnSecondes * GameManager.Instance.manaPerSecond;
            
            // MAGIE DE LA LOCALISATION : On injecte le nombre formaté à la place du {0}
            textePopupOffline.Arguments = new object[] { ScoreUI.FormatNumber(gainsEnAttente) };
            texteGains.text = textePopupOffline.GetLocalizedString();
            
            popupOffline.SetActive(true);
        }
    }

    public void RecupererNormal()
    {
        GameManager.Instance.AddMana(gainsEnAttente);
        if (AudioManager.Instance != null && AudioManager.Instance.cashSound != null)
            AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.cashSound);
        FermerPopup();
    }

    public void RecupererAvecPubX7()
    {
        if (AdMobManager.Instance != null)
        {
            AdMobManager.Instance.ShowRewardedAd(() => { RecompensePubValidee(); });
        }
        else RecupererNormal();
    }

    private void RecompensePubValidee()
    {
        GameManager.Instance.AddMana(gainsEnAttente * 7);
        if (AudioManager.Instance != null && AudioManager.Instance.cashSound != null)
            AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.cashSound);
        FermerPopup();
    }

    public void FermerPopup()
    {
        gainsEnAttente = 0;
        popupOffline.SetActive(false);
    }
}