using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Localization;

public class PrestigeUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI cristauxGagnesText;
    public TextMeshProUGUI multiplicateurText;
    public Button prestigeButton;

    [Header("Localisation")]
    public LocalizedString texteGainCristaux; // Clé ex: +{0} Cristaux
    public LocalizedString texteMultiplicateur; // Clé ex: Bonus x{0}
    public LocalizedString texteTropTot; // Clé ex: Reviens plus tard !

    private int cristauxGagnes;
    private double multiplicateurGagne;

    void Update()
    {
        if (GameManager.Instance == null) return;

        // Calcul des gains de prestige (basé sur le Mana Total Produit)
        double totalMana = GameManager.Instance.manaTotalProduced;
        
        // Formule classique : Racine cubique des milliards générés
        if (totalMana > 1000000)
        {
            cristauxGagnes = (int)(Mathf.Pow((float)(totalMana / 1000000), 0.33f));
            multiplicateurGagne = cristauxGagnes * 0.1; // Chaque cristal donne +10%
        }
        else
        {
            cristauxGagnes = 0;
            multiplicateurGagne = 0;
        }

        // --- LOCALISATION ---
        if (multiplicateurGagne >= 4.0)
        {
            prestigeButton.interactable = true;
            
            texteGainCristaux.Arguments = new object[] { ScoreUI.FormatNumber(cristauxGagnes) };
            if (cristauxGagnesText != null) cristauxGagnesText.text = texteGainCristaux.GetLocalizedString();

            texteMultiplicateur.Arguments = new object[] { multiplicateurGagne.ToString("F2") };
            if (multiplicateurText != null) multiplicateurText.text = texteMultiplicateur.GetLocalizedString();
        }
        else
        {
            prestigeButton.interactable = false;
            if (cristauxGagnesText != null) cristauxGagnesText.text = texteTropTot.GetLocalizedString();
            if (multiplicateurText != null) multiplicateurText.text = "";
        }
    }

    public void ValiderPrestige()
    {
        if (multiplicateurGagne < 4.0) return;

        // 1. Gain des ressources
        GameManager.Instance.prestigeMultiplier += multiplicateurGagne;
        GameManager.Instance.temporalCrystals += cristauxGagnes;
        PlayerPrefs.SetString("prestigeMultiplier", GameManager.Instance.prestigeMultiplier.ToString());

        // 2. Reset du Mana
        GameManager.Instance.manaCurrent = 0;
        GameManager.Instance.manaTotalProduced = 0;
        GameManager.Instance.manaPerSecond = 0;

        // 3. Reset des Cartes de la boutique
        BoutiqueCartesManager[] managers = Resources.FindObjectsOfTypeAll<BoutiqueCartesManager>();
        if (managers.Length > 0)
        {
            foreach (CarteDef carte in managers[0].listeCartes)
            {
                PlayerPrefs.DeleteKey("Achete_" + carte.idUnique);
            }
        }

        // 4. Reset des étages
        UpgradeShopUI[] tousLesEtages = FindObjectsOfType<UpgradeShopUI>();
        foreach (var etage in tousLesEtages)
        {
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

        if (AudioManager.Instance != null && AudioManager.Instance.buySound != null)
        {
            AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.buySound);
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}