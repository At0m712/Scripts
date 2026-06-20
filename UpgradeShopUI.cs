using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class UpgradeShopUI : MonoBehaviour
{
    [Header("Configuration")]
    public FloorData myFloorData;
    public int currentLevel = 0;

    [Header("UI Elements")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI productionText;
    public Button upgradeButton;
    
    [Header("Jauge de Progression vers Palier")]
    [Tooltip("L'image de la barre qui se remplit (Jauge_Fill)")]
    public Image jaugeProgression; 

    private double currentCost;
    private double currentProduction;
    private double previousProduction = 0; // Pour retirer l'ancienne prod au GameManager

    void Start()
    {
        if (myFloorData == null) return;
        
        // Charge le niveau sauvegardé (Clé unique par nom d'étage)
        currentLevel = PlayerPrefs.GetInt("FloorLevel_" + myFloorData.name, 0);
        
        if (nameText != null) nameText.text = myFloorData.name;
        
        RecalculerStats();
        upgradeButton.onClick.AddListener(AcheterNiveau);
        
        // On vérifie si on a l'argent 5 fois par seconde (très optimisé)
        InvokeRepeating(nameof(VerifierArgent), 0.1f, 0.2f);
    }

    private void VerifierArgent()
    {
        if (GameManager.Instance == null) return;
        
        // On calcule le prix final en appliquant la réduction de l'Arbre de compétences
        double actualCost = currentCost * (1f - GameManager.Instance.costReductionBonus);
        upgradeButton.interactable = (GameManager.Instance.manaCurrent >= actualCost);
    }

    public void AcheterNiveau()
    {
        double actualCost = currentCost * (1f - GameManager.Instance.costReductionBonus);

        if (GameManager.Instance.SpendMana(actualCost))
        {
            // 1. On retire l'ancienne production de cet étage
            if (currentLevel > 0) GameManager.Instance.manaPerSecond -= previousProduction;

            // 2. On monte de niveau et on sauvegarde
            currentLevel++;
            PlayerPrefs.SetInt("FloorLevel_" + myFloorData.name, currentLevel);
            PlayerPrefs.Save();

            // 3. On recalcule tout
            RecalculerStats();

            // 4. On ajoute la NOUVELLE production boostée
            GameManager.Instance.manaPerSecond += currentProduction;

            // 5. Son et Quête
            if (AudioManager.Instance != null && AudioManager.Instance.buySound != null)
                AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.buySound);

            if (QuestManager.Instance != null)
                QuestManager.Instance.AddUpgradeProgress();
        }
    }

    private void RecalculerStats()
    {
        // 1. Calcul du coût actuel (Formule classique Idle : Base * Multiplier ^ Niveau)
        currentCost = myFloorData.baseCost * Math.Pow(myFloorData.costMultiplier, currentLevel);
        
        // 2. Calcul des PALIERS (Milestones)
        double multiplicateurPalier = CalculerMultiplicateurPalier(currentLevel);
        
        // 3. Calcul de la Production
        if (currentLevel == 0) 
        {
            currentProduction = 0;
        }
        else
        {
            currentProduction = myFloorData.baseProduction * currentLevel * multiplicateurPalier;
        }

        previousProduction = currentProduction;

        // 4. Mise à jour des Textes
        double actualCost = currentCost * (1f - GameManager.Instance.costReductionBonus);
        
        levelText.text = "Niv. " + currentLevel;
        costText.text = ScoreUI.FormatNumber(actualCost);
        productionText.text = ScoreUI.FormatNumber(currentProduction) + " / sec";

        // 5. Mise à jour de la Jauge
        MettreAJourJauge();
    }

    // --- LE MOTEUR DES PALIERS ---
    private double CalculerMultiplicateurPalier(int niveau)
    {
        double mult = 1.0;

        if (niveau >= 25) mult *= 1.5; // +50% au niveau 25
        if (niveau >= 75) mult *= 2.0; // x2 au niveau 75
        
        // À partir de 100, on double à CHAQUE centaine (100, 200, 300...)
        if (niveau >= 100)
        {
            int centaines = niveau / 100;
            mult *= Math.Pow(2.0, centaines); // x2 pour 100, x4 pour 200, x8 pour 300...
        }

        return mult;
    }

    // --- GESTION DE LA JAUGE VISUELLE ---
    private void MettreAJourJauge()
    {
        if (jaugeProgression == null) return;

        int prochainPalier = ObtenirProchainPalier(currentLevel);
        int palierPrecedent = ObtenirPalierPrecedent(currentLevel);

        // Calcule le pourcentage entre le niveau actuel et le prochain boost (ex: entre niv 0 et 25)
        float progression = (float)(currentLevel - palierPrecedent) / (prochainPalier - palierPrecedent);
        jaugeProgression.fillAmount = Mathf.Clamp01(progression);
    }

    private int ObtenirProchainPalier(int niveau)
    {
        if (niveau < 25) return 25;
        if (niveau < 75) return 75;
        if (niveau < 100) return 100;
        
        // S'il est niv 140, le prochain est 200. S'il est 310, le prochain est 400.
        return ((niveau / 100) + 1) * 100;
    }

    private int ObtenirPalierPrecedent(int niveau)
    {
        if (niveau < 25) return 0;
        if (niveau < 75) return 25;
        if (niveau < 100) return 75;
        
        return (niveau / 100) * 100;
    }
}