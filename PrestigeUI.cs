using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Localization;
using System.Globalization; // NOUVEAU : Indispensable pour la sécurité des sauvegardes

public class PrestigeUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI cristauxGagnesText;
    public TextMeshProUGUI multiplicateurText;
    public Button prestigeButton;

    [Header("Localisation")]
    public LocalizedString texteGainCristaux; 
    public LocalizedString texteMultiplicateur; 
    public LocalizedString texteTropTot; 

    private int cristauxGagnes;
    private double multiplicateurGagne;

    void Update()
    {
        if (GameManager.Instance == null) return;

        double totalMana = GameManager.Instance.manaTotalProduced;
        
        if (totalMana > 1000000)
        {
            // 🛡️ CORRECTION MATHÉMATIQUE : Utilisation de System.Math (Double) au lieu de Mathf (Float) 
            // pour éviter que le jeu ne crashe quand le joueur atteindra des sommes faramineuses (AA, AB...).
            cristauxGagnes = (int)(System.Math.Pow(totalMana / 1000000.0, 0.33));
            multiplicateurGagne = cristauxGagnes * 0.1; 
        }
        else
        {
            cristauxGagnes = 0;
            multiplicateurGagne = 0;
        }

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

        // 1. Gain des ressources (Ajout de CultureInfo.InvariantCulture pour éviter les bugs de virgule/point selon les pays)
        GameManager.Instance.prestigeMultiplier += multiplicateurGagne;
        GameManager.Instance.temporalCrystals += cristauxGagnes;
        PlayerPrefs.SetString("prestigeMultiplier", GameManager.Instance.prestigeMultiplier.ToString(CultureInfo.InvariantCulture));

        // 2. Reset du Mana
        GameManager.Instance.manaCurrent = 0;
        GameManager.Instance.manaTotalProduced = 0;
        GameManager.Instance.manaPerSecond = 0;

        // 🛡️ CORRECTION MAJEURE : On a SUPPRIMÉ la boucle qui détruisait les Cartes de la Boutique.
        // Les cartes Premium et le Gacha Survivent au Prestige !

        // 3. Reset de la progression des étages
        UpgradeShopUI[] tousLesEtages = FindObjectsOfType<UpgradeShopUI>();
        foreach (var etage in tousLesEtages)
        {
            string nomEtage = etage.myFloorData.name;
            
            // On conserve UNIQUEMENT le bonus de niveau de départ offert par les Cartes Sorciers Légendaires
            int minLevels = PlayerPrefs.GetInt("BonusLevels_" + nomEtage, 0);
            
            etage.currentLevel = minLevels;
            PlayerPrefs.SetInt("FloorLevel_" + nomEtage, minLevels);
            PlayerPrefs.SetFloat("FloorTimer_" + nomEtage, 0f); 
            
            // 🛡️ CORRECTION MAJEURE : On ne supprime plus "BonusProd_" et "BonusCost_", 
            // sinon les cartes achetées par le joueur perdaient leurs effets !
        }

        // 4. Sauvegardes finales (Utilisation du SaveManager pour tout sécuriser d'un coup)
        PlayerPrefs.SetString("manaCurrent", "0");
        PlayerPrefs.SetString("manaTotalProduced", "0");
        PlayerPrefs.SetInt("temporalCrystals", GameManager.Instance.temporalCrystals);
        
        if (SaveManager.Instance != null) SaveManager.Instance.DemanderSauvegarde();
        else PlayerPrefs.Save(); // Filet de sécurité

        // 5. Son et rechargement
        if (AudioManager.Instance != null && AudioManager.Instance.buySound != null)
        {
            AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.buySound);
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}