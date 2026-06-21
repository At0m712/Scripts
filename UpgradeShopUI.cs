using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class UpgradeShopUI : MonoBehaviour
{
    [Header("Configuration")]
    public FloorData myFloorData;
    public int currentLevel = 0;
    public bool estLePremierEtage = false; 

    [Header("UI Elements")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI productionText;
    public Button upgradeButton;
    public TextMeshProUGUI upgradeButtonText; 
    
    [Header("Jauge de Production")]
    public Image jaugeProgression; 

    private double currentCostToBuy; 
    private int levelsToBuy; 
    private double currentBaseYield; 
    private float currentProductionTime;
    private float timer = 0f;

    void Start()
    {
        if (myFloorData == null) return;
        
        // CORRECTION NIVEAU DE DÉPART (Cartes)
        int minLevels = PlayerPrefs.GetInt("Carte_NiveauxDepart_" + myFloorData.name, 0) * 10;
        currentLevel = PlayerPrefs.GetInt("FloorLevel_" + myFloorData.name, minLevels);

        timer = PlayerPrefs.GetFloat("FloorTimer_" + myFloorData.name, 0f);
        if (nameText != null) nameText.text = myFloorData.name;
        
        RecalculerStats();
        upgradeButton.onClick.AddListener(AcheterNiveau);
        InvokeRepeating(nameof(VerifierArgent), 0.1f, 0.2f);
    }

    void Update()
    {
        if (currentLevel > 0 && currentProductionTime > 0)
        {
            float speedMultiplier = 1f;
            if (GameManager.Instance != null && GameManager.Instance.IsRushActive)
                speedMultiplier = (float)GameManager.Instance.rushMultiplier;

            timer += Time.deltaTime * speedMultiplier;

            if (jaugeProgression != null) jaugeProgression.fillAmount = timer / currentProductionTime;

            if (timer >= currentProductionTime)
            {
                timer -= currentProductionTime; 
                if (GameManager.Instance != null) GameManager.Instance.AddMana(GetActualYield());
            }
        }
        else
        {
            if (jaugeProgression != null) jaugeProgression.fillAmount = 0f;
        }
    }

    private void VerifierArgent()
    {
        if (GameManager.Instance == null) return;
        
        if (BuyModeManager.Instance != null && BuyModeManager.Instance.currentMode == BuyMode.MAX)
            RecalculerStats();
        
        bool isFreeUnlock = (estLePremierEtage && currentLevel == 0);
        
        if (isFreeUnlock) upgradeButton.interactable = true;
        else upgradeButton.interactable = (levelsToBuy > 0 && GameManager.Instance.manaCurrent >= currentCostToBuy);
    }

    public void AcheterNiveau()
    {
        if (levelsToBuy <= 0) return;

        bool isFreeUnlock = (estLePremierEtage && currentLevel == 0);
        bool canBuy = isFreeUnlock ? true : GameManager.Instance.SpendMana(currentCostToBuy);

        if (canBuy)
        {
            currentLevel += levelsToBuy;
            PlayerPrefs.SetInt("FloorLevel_" + myFloorData.name, currentLevel);
            RecalculerStats();

            if (AudioManager.Instance != null && AudioManager.Instance.buySound != null)
                AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.buySound);

            if (QuestManager.Instance != null)
            {
                int questTicks = Mathf.Min(levelsToBuy, 100);
                for(int i=0; i < questTicks; i++) QuestManager.Instance.AddUpgradeProgress();
            }
        }
    }

    // --- NOUVEAU : Récupère le prix de base AVEC la réduction des Cartes Sorciers ---
    private double GetDiscountedBaseCost()
    {
        // Par défaut le bonus est de 1f (pas de réduction). Si on a acheté une carte à 0.90, ça multiplie.
        float cardDiscount = PlayerPrefs.GetFloat("BonusCost_" + myFloorData.name, 1f); 
        return myFloorData.baseCost * cardDiscount;
    }

    public void RecalculerStats()
    {
        if (myFloorData == null || GameManager.Instance == null) return;

        // --- 1. LECTURE DES NIVEAUX DE DÉPART (Bonus Cartes) ---
        int minLevels = PlayerPrefs.GetInt("BonusLevels_" + myFloorData.name, 0);
        if (currentLevel < minLevels) 
        {
            currentLevel = minLevels;
            PlayerPrefs.SetInt("FloorLevel_" + myFloorData.name, currentLevel);
        }

        double discount = 1f - GameManager.Instance.costReductionBonus;
        double currentMana = GameManager.Instance.manaCurrent;
        
        levelsToBuy = 0;
        currentCostToBuy = 0;

        bool isFreeUnlock = (estLePremierEtage && currentLevel == 0);

        // --- 2. GESTION DES COÛTS ET DES MODES D'ACHAT ---
        if (isFreeUnlock)
        {
            levelsToBuy = 1;
            currentCostToBuy = 0;
        }
        else
        {
            BuyMode mode = BuyModeManager.Instance != null ? BuyModeManager.Instance.currentMode : BuyMode.x1;
            int targetLevelsToAdd = 1;
            
            if (mode == BuyMode.x1) targetLevelsToAdd = 1;
            else if (mode == BuyMode.x10) targetLevelsToAdd = 10;
            else if (mode == BuyMode.x100) targetLevelsToAdd = 100;
            else if (mode == BuyMode.NEXT) targetLevelsToAdd = ObtenirProchainPalier(currentLevel) - currentLevel;

            if (mode == BuyMode.MAX) 
            {
                CalculateMaxAffordable(currentLevel, currentMana, discount, out levelsToBuy, out currentCostToBuy);
            }
            else
            {
                CalculateCostForLevels(currentLevel, targetLevelsToAdd, discount, out currentCostToBuy);
                levelsToBuy = targetLevelsToAdd;
            }
        }

        double baseYield = 0;
        float time = myFloorData.baseProductionTime;

        // --- 3. CALCUL DE LA PRODUCTION ET DU TEMPS ---
        if (currentLevel > 0)
        {
            // Lecture du multiplicateur de production (Bonus Cartes)
            float cardProdMulti = PlayerPrefs.GetFloat("BonusProd_" + myFloorData.name, 1f);

            // Calcul de base combinant Niveau + Paliers + Cartes
            baseYield = myFloorData.baseProduction * currentLevel * CalculerMultiplicateurPalier(currentLevel) * cardProdMulti;

            // Réduction de temps agressive
            int[] paliersTemps = { 25, 50, 75, 100, 150, 200, 250, 300, 400, 500, 600, 700, 800, 900, 1000 };
            
            foreach (int palier in paliersTemps)
            {
                if (currentLevel >= palier) 
                {
                    if (palier >= 100) time /= 4f; // À partir du niv 100, on divise violemment par 4
                    else time /= 3f;               // Pour 25, 50 et 75, on divise par 3
                }
            }

            // On bride visuellement à 0.1s (effet "mitraillette") et on convertit l'excédent en Mana brut pour ne rien perdre
            if (time < 0.1f && time > 0f)
            {
                baseYield *= (0.1f / time);
                time = 0.1f;
            }
        }

        currentBaseYield = baseYield;
        currentProductionTime = time;

        // --- 4. MISE À JOUR DE L'AFFICHAGE (TEXTES) ---
        levelText.text = "Niv. " + currentLevel;
        
        if (isFreeUnlock)
        {
            costText.text = "GRATUIT";
            if (upgradeButtonText != null) upgradeButtonText.text = "DÉBLOQUER";
        }
        else if (levelsToBuy > 0)
        {
            costText.text = ScoreUI.FormatNumber(currentCostToBuy);
            BuyMode mode = BuyModeManager.Instance != null ? BuyModeManager.Instance.currentMode : BuyMode.x1;
            if (upgradeButtonText != null) 
                upgradeButtonText.text = (mode == BuyMode.MAX) ? "+MAX (" + levelsToBuy + ")" : "+" + levelsToBuy;
        }
        else
        {
            CalculateCostForLevels(currentLevel, 1, discount, out double costForOne);
            costText.text = ScoreUI.FormatNumber(costForOne);
            if (upgradeButtonText != null) upgradeButtonText.text = "+1";
        }

        // --- 5. AFFICHAGE DE LA RENTABILITÉ (DPS) ---
        double yieldToDisplay = GetActualYield();

        if (currentLevel == 0) 
        {
            productionText.text = "0 / sec";
        }
        else
        {
            // Affiche le vrai DPS calculé correctement, même si la barre descend sous les 1s
            if (currentProductionTime <= 1f) 
                productionText.text = ScoreUI.FormatNumber(yieldToDisplay / currentProductionTime) + " / sec";
            else if (currentProductionTime < 60f) 
                productionText.text = ScoreUI.FormatNumber(yieldToDisplay) + " / " + Math.Round(currentProductionTime, 1) + "s";
            else 
                productionText.text = ScoreUI.FormatNumber(yieldToDisplay) + " / " + Math.Round(currentProductionTime / 60f, 1) + "m";
        }

        // --- 6. ON PRÉVIENT LE MOTEUR DU JEU ---
        GameManager.Instance.CalculerDPSGlobal();
    }

    public double GetActualYield()
    {
        if (GameManager.Instance == null) return currentBaseYield;
        return currentBaseYield * GameManager.Instance.globalMultiplier * GameManager.Instance.adBoostMultiplier;
    }

    public double ObtenirDPS()
    {
        if (currentProductionTime > 0) 
        {
            double dps = GetActualYield() / currentProductionTime;
            if (GameManager.Instance != null && GameManager.Instance.IsRushActive) dps *= GameManager.Instance.rushMultiplier;
            return dps;
        }
        return 0;
    }

    private void CalculateCostForLevels(int startLevel, int levelsToAdd, double discount, out double totalCost)
    {
        // On utilise la nouvelle fonction de prix réduit !
        double baseCost = GetDiscountedBaseCost();
        double multiplier = myFloorData.costMultiplier;
        
        if (multiplier <= 1.001)
        {
            totalCost = (baseCost * discount) * levelsToAdd;
            return;
        }

        double a = (baseCost * Math.Pow(multiplier, startLevel)) * discount;
        totalCost = a * (Math.Pow(multiplier, levelsToAdd) - 1) / (multiplier - 1);
        if (double.IsNaN(totalCost) || double.IsInfinity(totalCost)) totalCost = double.MaxValue;
    }

    private void CalculateMaxAffordable(int startLevel, double currentMana, double discount, out int affordableLevels, out double totalCost)
    {
        affordableLevels = 0;
        totalCost = 0;
        // On utilise la nouvelle fonction de prix réduit !
        double baseCost = GetDiscountedBaseCost();
        double multiplier = myFloorData.costMultiplier;

        if (multiplier <= 1.001)
        {
            double singleCost = baseCost * discount;
            if (singleCost > 0) affordableLevels = (int)(currentMana / singleCost);
            totalCost = affordableLevels * singleCost;
            return;
        }

        double a = (baseCost * Math.Pow(multiplier, startLevel)) * discount;
        if (a > currentMana) return; 

        double rhs = (currentMana * (multiplier - 1.0) / a) + 1.0;
        if (rhs <= 0) return; 

        affordableLevels = (int)Math.Floor(Math.Log(rhs, multiplier));
        if (affordableLevels > 0) CalculateCostForLevels(startLevel, affordableLevels, discount, out totalCost);
    }

    private double CalculerMultiplicateurPalier(int niveau)
    {
        double mult = 1.0;
        
        // Paliers de base (x2 au lieu de x1.5 pour un meilleur ressenti)
        if (niveau >= 25) mult *= 2.0; 
        if (niveau >= 50) mult *= 2.0; 
        if (niveau >= 75) mult *= 2.0; 
        
        // Boucle infinie pour le Late-Game
        if (niveau >= 100)
        {
            int centaines = niveau / 100;
            // On booste le late-game : x3 tous les 100 niveaux (au lieu de x2) !
            mult *= Math.Pow(3.0, centaines); 
        }
        return mult;
    }

    private int ObtenirProchainPalier(int niveau)
    {
        if (niveau < 25) return 25;
        if (niveau < 50) return 50;
        if (niveau < 75) return 75;
        if (niveau < 100) return 100;
        return ((niveau / 100) + 1) * 100;
    }

    void OnDisable()
    {
        if (myFloorData != null) PlayerPrefs.SetFloat("FloorTimer_" + myFloorData.name, timer);
    }
}