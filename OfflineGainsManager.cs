using UnityEngine;
using TMPro;
using System;

public class OfflineGainsManager : MonoBehaviour
{
    public static OfflineGainsManager Instance;

    [Header("Configuration UI")]
    [Tooltip("L'objet Popup_OfflineGains en entier")]
    public GameObject popupOffline; 
    public TextMeshProUGUI texteGains;
    
    private double gainsEnAttente = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // Fonction généralement appelée par ton SaveManager au démarrage du jeu
    public void AfficherGainsOffline(double tempsDeconnecteEnSecondes)
    {
        if (GameManager.Instance == null || GameManager.Instance.manaPerSecond <= 0) return;

        // On affiche la popup seulement si le joueur est parti plus de 60 secondes
        if (tempsDeconnecteEnSecondes >= 60)
        {
            gainsEnAttente = tempsDeconnecteEnSecondes * GameManager.Instance.manaPerSecond;
            
            // On met à jour le texte avec un beau style doré pour le montant
            texteGains.text = "Tes Sorciers ont travaillé en ton absence !\nTu as récolté :\n<color=#FFD700>" 
                              + ScoreUI.FormatNumber(gainsEnAttente) + " Mana</color>";
            
            popupOffline.SetActive(true);
        }
    }

    // ==========================================
    // 🖱️ BOUTON RÉCUPÉRER NORMAL (x1)
    // ==========================================
    public void RecupererNormal()
    {
        GameManager.Instance.AddMana(gainsEnAttente);
        
        if (AudioManager.Instance != null && AudioManager.Instance.cashSound != null)
            AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.cashSound);

        FermerPopup();
    }

    // ==========================================
    // 📺 BOUTON PUB (x7)
    // ==========================================
    public void RecupererAvecPubX7()
    {
        if (AdMobManager.Instance != null)
        {
            // Appelle la publicité vidéo. La fonction () => RecompensePubValidee() 
            // indique à Unity d'exécuter la récompense uniquement SI la pub a été vue jusqu'au bout.
            AdMobManager.Instance.ShowRewardedAd(() => 
            {
                RecompensePubValidee();
            });
        }
        else
        {
            Debug.LogWarning("AdMobManager absent. On donne la récompense normale par sécurité.");
            RecupererNormal();
        }
    }

    private void RecompensePubValidee()
    {
        GameManager.Instance.AddMana(gainsEnAttente * 7); // Le fameux multiplicateur x7 !
        
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