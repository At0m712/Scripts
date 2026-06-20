using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class BoutonMultiplicateurUI : MonoBehaviour
{
    [Header("Éléments du Bouton")]
    [Tooltip("Le texte qui affiche le chrono (ex: 00:10:00)")]
    public TextMeshProUGUI texteChrono; 
    
    [Tooltip("L'image de la jauge qui se remplit (Type: Filled)")]
    public Image jaugeTemps; 
    
    [Tooltip("Le texte qui affiche le multiplicateur actuel (ex: 2X)")]
    public TextMeshProUGUI texteNiveau; 

    [Header("Visuels d'état (Optionnel)")]
    [Tooltip("Objet à allumer quand c'est inactif (ex: icône grise)")]
    public GameObject visuelInactif; 
    [Tooltip("Objet à allumer quand c'est actif (ex: icône qui brille)")]
    public GameObject visuelActif; 

    private const float TEMPS_MAX_SECONDES = 3600f; // 1 Heure

    void Update()
    {
        // On actualise le bouton en permanence pour suivre le chrono
        SynchroniserBouton();
    }

    private void SynchroniserBouton()
    {
        // 1. Lecture des données sauvegardées (Exactement comme dans le Pop-up)
        int multi = PlayerPrefs.GetInt("multiplicateurArgentActuel", 1);
        string dateFinString = PlayerPrefs.GetString("dateFinMultiplicateur", "");

        DateTime finBonus;
        DateTime maintenant = DateTime.Now;

        bool isActif = false;
        TimeSpan tempsRestant = TimeSpan.Zero;

        // 2. Vérification du temps restant
        if (!string.IsNullOrEmpty(dateFinString) && DateTime.TryParse(dateFinString, out finBonus))
        {
            if (finBonus > maintenant)
            {
                isActif = true;
                tempsRestant = finBonus - maintenant;
            }
            else
            {
                multi = 1; // Temps écoulé
            }
        }

        // 3. Synchronisation du Chrono
        if (texteChrono != null)
        {
            if (isActif)
            {
                texteChrono.text = string.Format("{0:D2}:{1:D2}:{2:D2}", tempsRestant.Hours, tempsRestant.Minutes, tempsRestant.Seconds);
            }
            else
            {
                // Quand c'est inactif
                texteChrono.text = "OFF"; 
            }
        }

        // 4. Synchronisation de la Jauge de temps
        if (jaugeTemps != null)
        {
            if (isActif)
            {
                float ratioTemps = (float)tempsRestant.TotalSeconds / TEMPS_MAX_SECONDES;
                jaugeTemps.fillAmount = Mathf.Clamp01(ratioTemps);
            }
            else
            {
                jaugeTemps.fillAmount = 0f;
            }
        }

        // 5. Synchronisation du Texte du Multiplicateur (1X, 2X, etc.)
        if (texteNiveau != null)
        {
            texteNiveau.text = multi + "X"; 
        }

        // 6. Gestion des visuels d'état
        if (visuelInactif != null) visuelInactif.SetActive(!isActif);
        if (visuelActif != null) visuelActif.SetActive(isActif);
    }
}