using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class MultiplicateurUI : MonoBehaviour
{
    [Header("Textes Principaux")]
    public TextMeshProUGUI titreText;
    public TextMeshProUGUI boutonText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI texteStatut; 

    [Header("Chrono et Bouton")]
    public TextMeshProUGUI timerText;
    public Button boutonPub;

    [Header("Barre de Temps (Rouge)")]
    public Image barreTempsRouge; 

    [Header("Barre de progression Multiplicateur")]
    public Image fond2X;
    public Image fond3X;
    public Image fond4X;
    public Color couleurInactif = new Color(0.5f, 0.5f, 0.5f, 0.8f); 
    public Color couleurActif = new Color(0.2f, 0.8f, 0.2f, 1f); 

    [Header("Personnages (Objets à allumer/éteindre)")]
    public GameObject perso1X;
    public GameObject perso2X;
    public GameObject perso3X;
    public GameObject perso4X;

    [Header("Fonds Statut")]
    public GameObject fondStatut1X;
    public GameObject fondStatut2X;
    public GameObject fondStatut3X;
    public GameObject fondStatut4X;

    [Header("Élément Extra")]
    public GameObject elementExtra; 
    public GameObject objetVideo; 
    public GameObject imageExclamation; 

    [Header("Image dynamique")]
    public Image imageDynamiqueCouleur;
    public Color couleurImage1X = new Color(0.5f, 0.5f, 0.5f, 1f); 
    public Color couleurImage2X = new Color(0.2f, 0.8f, 0.2f, 1f); 
    public Color couleurImage3X = new Color(0.1f, 0.5f, 0.8f, 1f); 
    public Color couleurImage4X = new Color(1f, 0.8f, 0f, 1f);     

    private const float TEMPS_MAX_SECONDES = 14400f; 
    private int cacheSecondesTimer = -1;

    void Start()
    {
        if (boutonPub != null)
        {
            boutonPub.onClick.RemoveAllListeners();
            boutonPub.onClick.AddListener(LancerPubBoost);
        }
    }

    void Update()
    {
        MettreAJourAffichage();
    }

    public void LancerPubBoost()
    {
        if (boutonPub != null) boutonPub.interactable = false;

        if (AdMobManager.Instance != null) AdMobManager.Instance.ShowRewardedAd(() => AppliquerRecompensePub());
        else AppliquerRecompensePub();
    }

    private void AppliquerRecompensePub()
    {
        int currentMulti = PlayerPrefs.GetInt("multiplicateurArgentActuel", 1);
        if (currentMulti < 4) currentMulti++;

        DateTime finActuelle;
        string dateFinString = PlayerPrefs.GetString("dateFinMultiplicateur", "");

        if (DateTime.TryParse(dateFinString, out finActuelle) && finActuelle > DateTime.Now) finActuelle = finActuelle.AddHours(1);
        else finActuelle = DateTime.Now.AddHours(1);

        PlayerPrefs.SetInt("multiplicateurArgentActuel", currentMulti);
        PlayerPrefs.SetString("dateFinMultiplicateur", finActuelle.ToString());
        PlayerPrefs.Save(); 

        if (GameManager.Instance != null)
        {
            GameManager.Instance.adBoostMultiplier = currentMulti;
            GameManager.Instance.adBoostTimer = (float)(finActuelle - DateTime.Now).TotalSeconds;
            GameManager.Instance.ActualiserTousLesEtages();
        }

        if (AudioManager.Instance != null && AudioManager.Instance.buySound != null) AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.buySound);
    }

    public void MettreAJourAffichage()
    {
        if (GameManager.Instance == null) return;

        // OPTIMISATION MAX : Plus aucune lecture Disque, tout se fait via les infos RAM du GameManager
        int multi = (int)GameManager.Instance.adBoostMultiplier;
        float tempsRestantSec = GameManager.Instance.adBoostTimer;
        bool isActif = tempsRestantSec > 0;

        int secondesActuelles = (int)tempsRestantSec;

        // On ne met à jour les textes que si 1 seconde s'est écoulée
        if (cacheSecondesTimer != secondesActuelles)
        {
            cacheSecondesTimer = secondesActuelles;

            if (timerText != null)
            {
                if (isActif)
                {
                    TimeSpan ts = TimeSpan.FromSeconds(secondesActuelles);
                    timerText.text = string.Format("{0:D2}:{1:D2}:{2:D2}", (int)ts.TotalHours, ts.Minutes, ts.Seconds);
                }
                else timerText.text = "00:00:00";
            }

            if (barreTempsRouge != null)
            {
                float ratioTemps = tempsRestantSec / TEMPS_MAX_SECONDES;
                barreTempsRouge.fillAmount = isActif ? Mathf.Clamp01(ratioTemps) : 0f; 
            }

            // Gestion de l'affichage UI
            if (fond2X != null) fond2X.color = (multi >= 2) ? couleurActif : couleurInactif;
            if (fond3X != null) fond3X.color = (multi >= 3) ? couleurActif : couleurInactif;
            if (fond4X != null) fond4X.color = (multi >= 4) ? couleurActif : couleurInactif;

            if (multi <= 1) 
            {
                if (titreText != null) titreText.text = "BOOST INACTIF";
                if (boutonText != null) boutonText.text = "ACTIVER 2X (PUB)";
                if (descriptionText != null) descriptionText.text = "Regarde une pub pour doubler tes gains !";
                if (texteStatut != null) texteStatut.text = "Gains normaux (1X)";

                ActiverVisuels(true, false, false, false); 
                if (imageDynamiqueCouleur != null) imageDynamiqueCouleur.color = couleurImage1X;
                if (boutonPub != null) boutonPub.interactable = true;
            }
            else if (multi == 2) 
            {
                if (titreText != null) titreText.text = "BOOST ACTIF !";
                if (boutonText != null) boutonText.text = "PASSER EN 3X (PUB)";
                if (descriptionText != null) descriptionText.text = "Regarde une pub pour tripler tes gains !";
                if (texteStatut != null) texteStatut.text = "Gains doublés (2X)";

                ActiverVisuels(false, true, false, false); 
                if (imageDynamiqueCouleur != null) imageDynamiqueCouleur.color = couleurImage2X;
                if (boutonPub != null) boutonPub.interactable = true;
            }
            else if (multi == 3) 
            {
                if (titreText != null) titreText.text = "BOOST ACTIF !";
                if (boutonText != null) boutonText.text = "PASSER EN 4X (PUB)";
                if (descriptionText != null) descriptionText.text = "Regarde une pub pour quadrupler tes gains !";
                if (texteStatut != null) texteStatut.text = "Gains triplés (3X)";

                ActiverVisuels(false, false, true, false); 
                if (imageDynamiqueCouleur != null) imageDynamiqueCouleur.color = couleurImage3X;
                if (boutonPub != null) boutonPub.interactable = true;
            }
            else if (multi >= 4) 
            {
                if (titreText != null) titreText.text = "BOOST MAXIMUM !";
                if (texteStatut != null) texteStatut.text = "Gains quadruplés (4X)";

                ActiverVisuels(false, false, false, true); 
                if (imageDynamiqueCouleur != null) imageDynamiqueCouleur.color = couleurImage4X;
                
                if (secondesActuelles >= 14340f) // 239 min = 14340 sec
                {
                    if (boutonText != null) boutonText.text = "BOOST MAX";
                    if (descriptionText != null) descriptionText.text = "Tu as atteint le temps maximum !";
                    if (boutonPub != null) boutonPub.interactable = false;
                }
                else
                {
                    if (boutonText != null) boutonText.text = "+ 1 HEURE (PUB)";
                    if (descriptionText != null) descriptionText.text = "Regarde une pub pour ajouter 1H !";
                    if (boutonPub != null) boutonPub.interactable = true;
                }
            }
        }
    }

    private void ActiverVisuels(bool etat1X, bool etat2X, bool etat3X, bool etat4X)
    {
        if (perso1X != null && perso1X.activeSelf != etat1X) perso1X.SetActive(etat1X);
        if (perso2X != null && perso2X.activeSelf != etat2X) perso2X.SetActive(etat2X);
        if (perso3X != null && perso3X.activeSelf != etat3X) perso3X.SetActive(etat3X);
        if (perso4X != null && perso4X.activeSelf != etat4X) perso4X.SetActive(etat4X);

        if (fondStatut1X != null && fondStatut1X.activeSelf != etat1X) fondStatut1X.SetActive(etat1X);
        if (fondStatut2X != null && fondStatut2X.activeSelf != etat2X) fondStatut2X.SetActive(etat2X);
        if (fondStatut3X != null && fondStatut3X.activeSelf != etat3X) fondStatut3X.SetActive(etat3X);
        if (fondStatut4X != null && fondStatut4X.activeSelf != etat4X) fondStatut4X.SetActive(etat4X);

        bool showExtra = etat2X || etat3X || etat4X;
        if (elementExtra != null && elementExtra.activeSelf != showExtra) elementExtra.SetActive(showExtra);
        if (imageExclamation != null && imageExclamation.activeSelf != etat1X) imageExclamation.SetActive(etat1X);
        if (objetVideo != null && objetVideo.activeSelf != showExtra) objetVideo.SetActive(showExtra);
    }
}