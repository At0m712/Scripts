using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class PrestigeUI : MonoBehaviour
{
    [Header("Textes UI")]
    public TextMeshProUGUI cristauxPossedesText;
    public TextMeshProUGUI cristauxGagnesText;
    public TextMeshProUGUI prochainCristalText; 
    public TextMeshProUGUI bonusActuelText;

    [Header("Bouton de Prestige")]
    public Button boutonPrestige;

    private double multiplicateurGagne = 0;
    private int cristauxGagnes = 0;
    
    // La base est de 1 Million, mais elle va s'adapter au niveau actuel du joueur !
    private double baseRequis = 1000000; 

    void OnEnable()
    {
        if (boutonPrestige != null)
        {
            boutonPrestige.onClick.RemoveAllListeners();
            boutonPrestige.onClick.AddListener(ValiderPrestige);
        }
        MettreAJourAffichage();
    }

    void Update()
    {
        MettreAJourAffichage();
    }

    private void MettreAJourAffichage()
    {
        if (GameManager.Instance == null) return;

        double manaTotal = GameManager.Instance.manaTotalProduced;
        double multiActuel = GameManager.Instance.prestigeMultiplier;
        double seuilPrestige = baseRequis * multiActuel;

        if (manaTotal >= seuilPrestige)
        {
            // On vérifie si on dépasse le seuil de base
            // Si le mana total est pile au seuil, on gagne 4.0
            // Sinon, on applique la progression exponentielle au-delà
            double ratio = manaTotal / seuilPrestige;
            
            // La formule commence à grimper seulement si ratio > 1
            multiplicateurGagne = 4.0 * Math.Pow(ratio, 0.4f);
        }
        else
        {
            multiplicateurGagne = 0;
        }

        // --- AFFICHAGE ---
        // Ici, on s'assure de ne jamais afficher moins de 4.0 si le prestige est dispo
        double affichageGain = (multiplicateurGagne < 4.0 && manaTotal >= seuilPrestige) ? 4.0 : multiplicateurGagne;
        
        // ... (la suite reste identique à ton code précédent) ...

        // Les cristaux se basent sur le mana total brut (1 cristal tous les 100k avec une racine carrée)
        // Run 1 (1M) = 3 cristaux. Run 2 (5M) = 7 cristaux. Run 3 (13M) = 11 cristaux.
        cristauxGagnes = (int)Math.Floor(Math.Sqrt(manaTotal / 100000.0));

        // --- AFFICHAGE UI ---
        if (cristauxPossedesText != null)
            cristauxPossedesText.text = "Multiplicateur Global : X" + multiActuel.ToString("F1");

        if (cristauxGagnesText != null)
        {
            if (multiplicateurGagne > 0)
                cristauxGagnesText.text = "<color=#FFD700>GAIN : +X" + multiplicateurGagne.ToString("F1") + "</color>\n<size=50%>(et +" + cristauxGagnes + " Cristaux)</size>";
            else
                cristauxGagnesText.text = "PAS ASSEZ DE MANA";
        }

        if (prochainCristalText != null)
        {
            if (manaTotal < seuilPrestige)
                prochainCristalText.text = "Requis pour un bonus rentable : " + ScoreUI.FormatNumber(seuilPrestige - manaTotal) + " Mana";
            else
                prochainCristalText.text = "Plus vous attendez, plus le X monte !";
        }

        if (bonusActuelText != null)
        {
            double futurMulti = multiActuel + multiplicateurGagne;
            bonusActuelText.text = "Multiplicateur après prestige : X" + futurMulti.ToString("F1");
        }

        if (boutonPrestige != null)
            boutonPrestige.interactable = (multiplicateurGagne >= 4.0); 
    }

    public void ValiderPrestige()
    {
        if (multiplicateurGagne < 4.0) return;

        GameManager.Instance.prestigeMultiplier += multiplicateurGagne;
        GameManager.Instance.temporalCrystals += cristauxGagnes;

        PlayerPrefs.SetString("prestigeMultiplier", GameManager.Instance.prestigeMultiplier.ToString());

        GameManager.Instance.manaCurrent = 0;
        GameManager.Instance.manaTotalProduced = 0;
        GameManager.Instance.manaPerSecond = 0;

        UpgradeShopUI[] tousLesEtages = FindObjectsOfType<UpgradeShopUI>();
        foreach (var etage in tousLesEtages)
        {
            etage.currentLevel = 0;
            PlayerPrefs.SetInt("FloorLevel_" + etage.myFloorData.name, 0);
            PlayerPrefs.SetFloat("FloorTimer_" + etage.myFloorData.name, 0f); 
        }

        PlayerPrefs.SetString("manaCurrent", "0");
        PlayerPrefs.SetString("manaTotalProduced", "0");
        PlayerPrefs.SetInt("temporalCrystals", GameManager.Instance.temporalCrystals);
        PlayerPrefs.Save();

        if (AudioManager.Instance != null && AudioManager.Instance.buySound != null)
        {
            AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.buySound);
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}