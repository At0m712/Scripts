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

    [Header("Fonds Statut (Objets à allumer/éteindre)")]
    public GameObject fondStatut1X;
    public GameObject fondStatut2X;
    public GameObject fondStatut3X;
    public GameObject fondStatut4X;

    [Header("Élément Extra (S'allume à partir de 2X)")]
    public GameObject elementExtra; 

    [Header("Vidéo et Notifications")]
    [Tooltip("L'objet contenant la vidéo (S'allume de 2X à 4X)")]
    public GameObject objetVideo; 
    [Tooltip("L'image du point d'exclamation (S'allume uniquement en 1X)")]
    public GameObject imageExclamation; 

    [Header("Image dynamique (Changement de couleur)")]
    public Image imageDynamiqueCouleur;
    public Color couleurImage1X = new Color(0.5f, 0.5f, 0.5f, 1f); 
    public Color couleurImage2X = new Color(0.2f, 0.8f, 0.2f, 1f); 
    public Color couleurImage3X = new Color(0.1f, 0.5f, 0.8f, 1f); 
    public Color couleurImage4X = new Color(1f, 0.8f, 0f, 1f);     

    private const float TEMPS_MAX_SECONDES = 3600f; 

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
        boutonPub.interactable = false;

        if (AdMobManager.Instance != null)
        {
            AdMobManager.Instance.ShowRewardedAd(() => 
            {
                AppliquerRecompensePub();
            });
        }
        else
        {
            // Sécurité si on joue dans l'éditeur sans pub chargée
            AppliquerRecompensePub();
        }
    }

    private void AppliquerRecompensePub()
    {
        // 1. On lit le niveau actuel (1 par défaut)
        int currentMulti = PlayerPrefs.GetInt("multiplicateurArgentActuel", 1);
        
        // 2. On augmente jusqu'à 4 maximum
        if (currentMulti < 4) 
        {
            currentMulti++;
            PlayerPrefs.SetInt("multiplicateurArgentActuel", currentMulti);
        }

        // 3. On ajoute 1H au compteur
        DateTime finActuelle;
        string dateFinString = PlayerPrefs.GetString("dateFinMultiplicateur", "");

        if (DateTime.TryParse(dateFinString, out finActuelle) && finActuelle > DateTime.Now)
        {
            PlayerPrefs.SetString("dateFinMultiplicateur", finActuelle.AddHours(1).ToString());
        }
        else
        {
            PlayerPrefs.SetString("dateFinMultiplicateur", DateTime.Now.AddHours(1).ToString());
        }

        PlayerPrefs.Save(); // On force la sauvegarde

        // 4. On met à jour le GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.adBoostMultiplier = currentMulti;
            GameManager.Instance.adBoostTimer += 3600f; 
        }

        MettreAJourAffichage();

        if (AudioManager.Instance != null && AudioManager.Instance.buySound != null)
        {
            AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.buySound);
        }
    }

    public void MettreAJourAffichage()
    {
        // On lit les données depuis PlayerPrefs
        int multi = PlayerPrefs.GetInt("multiplicateurArgentActuel", 1);
        string dateFinString = PlayerPrefs.GetString("dateFinMultiplicateur", "");

        DateTime finBonus;
        DateTime maintenant = DateTime.Now;

        bool isActif = false;
        TimeSpan tempsRestant = TimeSpan.Zero;

        if (!string.IsNullOrEmpty(dateFinString) && DateTime.TryParse(dateFinString, out finBonus))
        {
            if (finBonus > maintenant)
            {
                isActif = true;
                tempsRestant = finBonus - maintenant;
            }
            else
            {
                multi = 1; 
                PlayerPrefs.SetInt("multiplicateurArgentActuel", 1); 
            }
        }

        if (isActif)
        {
            timerText.text = string.Format("{0:D2}:{1:D2}:{2:D2}", tempsRestant.Hours, tempsRestant.Minutes, tempsRestant.Seconds);
            
            if (barreTempsRouge != null)
            {
                float ratioTemps = (float)tempsRestant.TotalSeconds / TEMPS_MAX_SECONDES;
                barreTempsRouge.fillAmount = Mathf.Clamp01(ratioTemps); 
            }
        }
        else
        {
            timerText.text = "00:00:00";
            if (barreTempsRouge != null) barreTempsRouge.fillAmount = 0f;
        }

        fond2X.color = (multi >= 2) ? couleurActif : couleurInactif;
        fond3X.color = (multi >= 3) ? couleurActif : couleurInactif;
        fond4X.color = (multi >= 4) ? couleurActif : couleurInactif;

        if (multi <= 1) 
        {
            titreText.text = "BOOST INACTIF";
            boutonText.text = "ACTIVER 2X (PUB)";
            descriptionText.text = "Regarde une pub pour doubler tes gains !";
            texteStatut.text = "Gains normaux (1X)";

            ActiverVisuels(true, false, false, false); 
            if (imageDynamiqueCouleur != null) imageDynamiqueCouleur.color = couleurImage1X;
            boutonPub.interactable = true;
        }
        else if (multi == 2) 
        {
            titreText.text = "BOOST ACTIF !";
            boutonText.text = "PASSER EN 3X (PUB)";
            descriptionText.text = "Regarde une pub pour tripler tes gains !";
            texteStatut.text = "Gains doublés (2X)";

            ActiverVisuels(false, true, false, false); 
            if (imageDynamiqueCouleur != null) imageDynamiqueCouleur.color = couleurImage2X;
            boutonPub.interactable = true;
        }
        else if (multi == 3) 
        {
            titreText.text = "BOOST ACTIF !";
            boutonText.text = "PASSER EN 4X (PUB)";
            descriptionText.text = "Regarde une pub pour quadrupler tes gains !";
            texteStatut.text = "Gains triplés (3X)";

            ActiverVisuels(false, false, true, false); 
            if (imageDynamiqueCouleur != null) imageDynamiqueCouleur.color = couleurImage3X;
            boutonPub.interactable = true;
        }
        else if (multi >= 4) 
        {
            titreText.text = "BOOST MAXIMUM !";
            texteStatut.text = "Gains quadruplés (4X)";

            ActiverVisuels(false, false, false, true); 
            if (imageDynamiqueCouleur != null) imageDynamiqueCouleur.color = couleurImage4X;
            
            if (tempsRestant.TotalMinutes >= 59.9f)
            {
                boutonText.text = "BOOST MAX";
                descriptionText.text = "Tu as atteint le temps maximum !";
                boutonPub.interactable = false;
            }
            else
            {
                boutonText.text = "+ 1 HEURE (PUB)";
                descriptionText.text = "Regarde une pub pour ajouter 1H !";
                boutonPub.interactable = true;
            }
        }
    }

    private void ActiverVisuels(bool etat1X, bool etat2X, bool etat3X, bool etat4X)
    {
        if (perso1X != null) perso1X.SetActive(etat1X);
        if (perso2X != null) perso2X.SetActive(etat2X);
        if (perso3X != null) perso3X.SetActive(etat3X);
        if (perso4X != null) perso4X.SetActive(etat4X);

        if (fondStatut1X != null) fondStatut1X.SetActive(etat1X);
        if (fondStatut2X != null) fondStatut2X.SetActive(etat2X);
        if (fondStatut3X != null) fondStatut3X.SetActive(etat3X);
        if (fondStatut4X != null) fondStatut4X.SetActive(etat4X);

        if (elementExtra != null) elementExtra.SetActive(etat2X || etat3X || etat4X);
        if (imageExclamation != null) imageExclamation.SetActive(etat1X);
        if (objetVideo != null) objetVideo.SetActive(etat2X || etat3X || etat4X);
    }
}