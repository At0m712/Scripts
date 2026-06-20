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
    
    [Tooltip("Nouveau texte sur le bouton pour afficher +1, +10, +MAX")]
    public TextMeshProUGUI upgradeButtonText; 
    
    [Header("Jauge de Progression vers Palier")]
    public Image jaugeProgression; 

    private double currentCostToBuy; 
    private int levelsToBuy; 
    
    private double currentProduction;
    private double previousProduction = 0;

    void Start()
    {
        if (myFloorData == null) return;
        
        currentLevel = PlayerPrefs.GetInt("FloorLevel_" + myFloorData.name, 0);
        if (nameText != null) nameText.text = myFloorData.name;
        
        RecalculerStats();
        upgradeButton.onClick.AddListener(AcheterNiveau);
        InvokeRepeating(nameof(VerifierArgent), 0.1f, 0.2f);
    }

    private void VerifierArgent()
    {
        if (GameManager.Instance == null) return;
        
        // Si on est en mode MAX, le coût peut changer dès que notre mana monte
        if (BuyModeManager.Instance != null && BuyModeManager.Instance.currentMode == BuyMode.MAX)
        {
             RecalculerStats();
        }
        
        upgradeButton.interactable = (levelsToBuy > 0 && GameManager.Instance.manaCurrent >= currentCostToBuy);
    }

    public void AcheterNiveau()
    {
        if (levelsToBuy <= 0) return;

        if (GameManager.Instance.SpendMana(currentCostToBuy))
        {
            if (currentLevel > 0) GameManager.Instance.manaPerSecond -= previousProduction;

            currentLevel += levelsToBuy;
            PlayerPrefs.SetInt("FloorLevel_" + myFloorData.name, currentLevel);
            PlayerPrefs.Save();

            RecalculerStats();

            GameManager.Instance.manaPerSecond += currentProduction;

            if (AudioManager.Instance != null && AudioManager.Instance.buySound != null)
                AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.buySound);

            if (QuestManager.Instance != null)
            {
                for(int i=0; i < levelsToBuy; i++)
                    QuestManager.Instance.AddUpgradeProgress();
            }
        }
    }

    public void RecalculerStats()
    {
        if (myFloorData == null || GameManager.Instance == null) return;

        double discount = 1f - GameManager.Instance.costReductionBonus;
        double currentMana = GameManager.Instance.manaCurrent;
        
        levelsToBuy = 0;
        currentCostToBuy = 0;
        
        BuyMode mode = BuyModeManager.Instance != null ? BuyModeManager.Instance.currentMode : BuyMode.x1;

        int targetLevelsToAdd = 1;
        
        if (mode == BuyMode.x1) targetLevelsToAdd = 1;
        else if (mode == BuyMode.x10) targetLevelsToAdd = 10;
        else if (mode == BuyMode.x100) targetLevelsToAdd = 100;
        else if (mode == BuyMode.NEXT) 
        {
            int nextPalier = ObtenirProchainPalier(currentLevel);
            targetLevelsToAdd = nextPalier - currentLevel;
        }

        // Calcul des coûts
        if (mode == BuyMode.MAX)
        {
            CalculateMaxAffordable(currentLevel, currentMana, discount, out levelsToBuy, out currentCostToBuy);
        }
        else
        {
            CalculateCostForLevels(currentLevel, targetLevelsToAdd, discount, out currentCostToBuy);
            levelsToBuy = targetLevelsToAdd;
        }

        // Production
        double multiplicateurPalier = CalculerMultiplicateurPalier(currentLevel);
        if (currentLevel == 0) currentProduction = 0;
        else currentProduction = myFloorData.baseProduction * currentLevel * multiplicateurPalier;
        
        previousProduction = currentProduction;

        // Interface UI
        levelText.text = "Niv. " + currentLevel;
        
        if (levelsToBuy > 0)
        {
            costText.text = ScoreUI.FormatNumber(currentCostToBuy);
            if (upgradeButtonText != null) 
                upgradeButtonText.text = (mode == BuyMode.MAX) ? "+MAX (" + levelsToBuy + ")" : "+" + levelsToBuy;
        }
        else
        {
            // Même si on ne peut pas l'acheter, on affiche le prix d'1 niveau
            CalculateCostForLevels(currentLevel, 1, discount, out double costForOne);
            costText.text = ScoreUI.FormatNumber(costForOne);
            if (upgradeButtonText != null) upgradeButtonText.text = "+1";
        }

        productionText.text = ScoreUI.FormatNumber(currentProduction) + " / sec";
        MettreAJourJauge();
    }

    // Mathématiques de la série géométrique
    private void CalculateCostForLevels(int startLevel, int levelsToAdd, double discount, out double totalCost)
    {
        double baseCost = myFloorData.baseCost;
        double multiplier = myFloorData.costMultiplier;
        
        double a = (baseCost * Math.Pow(multiplier, startLevel)) * discount;
        totalCost = a * (Math.Pow(multiplier, levelsToAdd) - 1) / (multiplier - 1);
    }

    private void CalculateMaxAffordable(int startLevel, double currentMana, double discount, out int affordableLevels, out double totalCost)
    {
        affordableLevels = 0;
        totalCost = 0;
        double a = (myFloorData.baseCost * Math.Pow(myFloorData.costMultiplier, startLevel)) * discount;
        
        if (a > currentMana) return; 

        double rhs = (currentMana * (myFloorData.costMultiplier - 1.0) / a) + 1.0;
        affordableLevels = (int)Math.Floor(Math.Log(rhs, myFloorData.costMultiplier));
        
        if (affordableLevels > 0)
        {
            CalculateCostForLevels(startLevel, affordableLevels, discount, out totalCost);
        }
    }

    // Moteur des Paliers
    private double CalculerMultiplicateurPalier(int niveau)
    {
        double mult = 1.0;
        if (niveau >= 25) mult *= 1.5; 
        if (niveau >= 75) mult *= 2.0; 
        if (niveau >= 100)
        {
            int centaines = niveau / 100;
            mult *= Math.Pow(2.0, centaines); 
        }
        return mult;
    }

    private void MettreAJourJauge()
    {
        if (jaugeProgression == null) return;
        int prochainPalier = ObtenirProchainPalier(currentLevel);
        int palierPrecedent = ObtenirPalierPrecedent(currentLevel);
        float progression = (float)(currentLevel - palierPrecedent) / (prochainPalier - palierPrecedent);
        jaugeProgression.fillAmount = Mathf.Clamp01(progression);
    }

    private int ObtenirProchainPalier(int niveau)
    {
        if (niveau < 25) return 25;
        if (niveau < 75) return 75;
        if (niveau < 100) return 100;
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