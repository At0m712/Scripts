using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Localization;

public class BoutonMultiplicateurUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI texteChrono;
    public Image jaugeTemps;
    public TextMeshProUGUI texteNiveau; 
    public GameObject visuelInactif;
    public GameObject visuelActif;

    [Header("Localisation")]
    public LocalizedString texteMultiplicateurActif; // Clé ex: {0}X
    public LocalizedString texteBoostPret;           // Clé ex: BOOST PUB

    // Note : Ajuste cette valeur selon la durée max de ton boost (ex: 14400 = 4 heures)
    private float dureeMaxBoost = 14400f; 

    void Update()
    {
        if (GameManager.Instance == null) return;

        // SI LE BOOST EST ACTIF
        if (GameManager.Instance.adBoostTimer > 0)
        {
            if (visuelActif != null) visuelActif.SetActive(true);
            if (visuelInactif != null) visuelInactif.SetActive(false);

            // Formatage du chrono en HH:MM:SS
            TimeSpan timeSpan = TimeSpan.FromSeconds(GameManager.Instance.adBoostTimer);
            if (texteChrono != null) 
            {
                texteChrono.text = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
            }
            
            // Jauge visuelle
            if (jaugeTemps != null) 
            {
                jaugeTemps.fillAmount = (float)(GameManager.Instance.adBoostTimer / dureeMaxBoost); 
            }

            // Localisation du texte (ex: "4X")
            if (texteNiveau != null)
            {
                texteMultiplicateurActif.Arguments = new object[] { GameManager.Instance.adBoostMultiplier };
                texteNiveau.text = texteMultiplicateurActif.GetLocalizedString();
            }
        }
        // SI LE BOOST EST TERMINÉ / INACTIF
        else
        {
            if (visuelActif != null) visuelActif.SetActive(false);
            if (visuelInactif != null) visuelInactif.SetActive(true);

            if (texteChrono != null) texteChrono.text = "";
            if (jaugeTemps != null) jaugeTemps.fillAmount = 0;
            
            // Localisation du bouton prêt
            if (texteNiveau != null) 
            {
                texteNiveau.text = texteBoostPret.GetLocalizedString();
            }
        }
    }
}