using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class BoutonMultiplicateurUI : MonoBehaviour
{
    [Header("Éléments du Bouton")]
    public TextMeshProUGUI texteChrono; 
    public Image jaugeTemps; 
    public TextMeshProUGUI texteNiveau; 

    [Header("Visuels d'état")]
    public GameObject visuelInactif; 
    public GameObject visuelActif; 

    private const float TEMPS_MAX_SECONDES = 3600f; 
    private int cacheSecondes = -1;

    void Update()
    {
        SynchroniserBouton();
    }

    private void SynchroniserBouton()
    {
        if (GameManager.Instance == null) return;

        float tempsRestantSec = GameManager.Instance.adBoostTimer;
        int multi = (int)GameManager.Instance.adBoostMultiplier;
        bool isActif = tempsRestantSec > 0;
        int secActuelles = (int)tempsRestantSec;

        // Mise à jour uniquement 1 fois par seconde ou au changement d'état !
        if (cacheSecondes != secActuelles)
        {
            cacheSecondes = secActuelles;

            if (texteChrono != null)
            {
                if (isActif)
                {
                    TimeSpan ts = TimeSpan.FromSeconds(secActuelles);
                    texteChrono.text = string.Format("{0:D2}:{1:D2}:{2:D2}", ts.Hours, ts.Minutes, ts.Seconds);
                }
                else texteChrono.text = "OFF"; 
            }

            if (jaugeTemps != null)
            {
                float ratioTemps = tempsRestantSec / TEMPS_MAX_SECONDES;
                jaugeTemps.fillAmount = isActif ? Mathf.Clamp01(ratioTemps) : 0f;
            }

            if (texteNiveau != null) texteNiveau.text = multi + "X"; 
            
            if (visuelInactif != null && visuelInactif.activeSelf == isActif) visuelInactif.SetActive(!isActif);
            if (visuelActif != null && visuelActif.activeSelf != isActif) visuelActif.SetActive(isActif);
        }
    }
}