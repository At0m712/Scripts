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
    public Button boutonPrestige;

    private double multiplicateurGagne = 0;
    private int cristauxGagnes = 0;
    private double baseRequis = 1000000; 

    private double cacheManaTotal = -1;

    void OnEnable()
    {
        if (boutonPrestige != null)
        {
            boutonPrestige.onClick.RemoveAllListeners();
            boutonPrestige.onClick.AddListener(ValiderPrestige);
        }
        MettreAJourAffichage(true);
    }

    void Update()
    {
        if (GameManager.Instance == null) return;
        
        // OPTIMISATION MAX : On ne fait les calculs mathématiques et d'affichage que si l'argent total du joueur a monté !
        if (cacheManaTotal != GameManager.Instance.manaTotalProduced)
        {
            cacheManaTotal = GameManager.Instance.manaTotalProduced;
            MettreAJourAffichage(false);
        }
    }

    private void MettreAJourAffichage(bool forcer)
    {
        double manaTotal = GameManager.Instance.manaTotalProduced;
        double multiActuel = GameManager.Instance.prestigeMultiplier;
        double seuilPrestige = baseRequis * multiActuel;

        if (manaTotal >= seuilPrestige)
        {
            double ratio = manaTotal / seuilPrestige;
            multiplicateurGagne = 4.0 * Math.Pow(ratio, 0.4f);
        }
        else multiplicateurGagne = 0;

        double affichageGain = (multiplicateurGagne < 4.0 && manaTotal >= seuilPrestige) ? 4.0 : multiplicateurGagne;
        cristauxGagnes = (int)Math.Floor(Math.Sqrt(manaTotal / 100000.0));

        if (cristauxPossedesText != null) cristauxPossedesText.text = "Multiplicateur Global : X" + multiActuel.ToString("F1");

        if (cristauxGagnesText != null)
        {
            if (multiplicateurGagne > 0) cristauxGagnesText.text = "<color=#FFD700>GAIN : +X" + multiplicateurGagne.ToString("F1") + "</color>\n<size=50%>(et +" + cristauxGagnes + " Cristaux)</size>";
            else cristauxGagnesText.text = "PAS ASSEZ DE MANA";
        }

        if (prochainCristalText != null)
        {
            if (manaTotal < seuilPrestige) prochainCristalText.text = "Requis pour un bonus rentable : " + ScoreUI.FormatNumber(seuilPrestige - manaTotal) + " Mana";
            else prochainCristalText.text = "Plus vous attendez, plus le X monte !";
        }

        if (bonusActuelText != null)
        {
            double futurMulti = multiActuel + multiplicateurGagne;
            bonusActuelText.text = "Multiplicateur après prestige : X" + futurMulti.ToString("F1");
        }

        if (boutonPrestige != null) boutonPrestige.interactable = (multiplicateurGagne >= 4.0); 
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

        BoutiqueCartesManager[] managers = Resources.FindObjectsOfTypeAll<BoutiqueCartesManager>();
        if (managers.Length > 0)
        {
            foreach (CarteDef carte in managers[0].listeCartes) PlayerPrefs.DeleteKey("Achete_" + carte.idUnique);
        }

        // OPTIMISATION MAX : Utilisation de la liste rapide
        for(int i = 0; i < UpgradeShopUI.AllShops.Count; i++)
        {
            UpgradeShopUI etage = UpgradeShopUI.AllShops[i];
            string nomEtage = etage.myFloorData.name;

            int minLevels = PlayerPrefs.GetInt("BonusLevels_" + nomEtage, 0);
            etage.currentLevel = minLevels;
            PlayerPrefs.SetInt("FloorLevel_" + nomEtage, minLevels);
            PlayerPrefs.SetFloat("FloorTimer_" + nomEtage, 0f); 

            PlayerPrefs.DeleteKey("BonusProd_" + nomEtage);
            PlayerPrefs.DeleteKey("BonusCost_" + nomEtage);
            PlayerPrefs.DeleteKey("BonusLevels_" + nomEtage);
        }

        PlayerPrefs.SetString("manaCurrent", "0");
        PlayerPrefs.SetString("manaTotalProduced", "0");
        PlayerPrefs.SetInt("temporalCrystals", GameManager.Instance.temporalCrystals);
        PlayerPrefs.Save();

        if (AudioManager.Instance != null && AudioManager.Instance.buySound != null) AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.buySound);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}