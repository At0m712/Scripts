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
    [Tooltip("Cette jauge se remplit avec le temps. Une fois pleine, elle donne le Mana.")]
    public Image jaugeProgression; 

    private double currentCostToBuy; 
    private int levelsToBuy; 
    
    // Variables de Temps
    private double currentProductionAmount;
    private float currentProductionTime;
    private float timer = 0f;

    void Start()
    {
        if (myFloorData == null) return;
        
        currentLevel = PlayerPrefs.GetInt("FloorLevel_" + myFloorData.name, 0);
        // On récupère le chrono là où il s'est arrêté à la dernière partie !
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
            // Le Rush accélère la jauge de l'étage x10 au lieu d'augmenter la valeur !
            float speedMultiplier = 1f;
            if (GameManager.Instance != null && GameManager.Instance.IsRushActive)
            {
                speedMultiplier = (float)GameManager.Instance.rushMultiplier;
            }

            timer += Time.deltaTime * speedMultiplier;

            if (jaugeProgression != null)
            {
                jaugeProgression.fillAmount = timer / currentProductionTime;
            }

            // DÈS QUE LA JAUGE EST PLEINE :
            if (timer >= currentProductionTime)
            {
                timer -= currentProductionTime; // On réinitialise sans perdre de millisecondes

                if (GameManager.Instance != null)
                {
                    double finalYield = currentProductionAmount * GameManager.Instance.globalMultiplier * GameManager.Instance.adBoostMultiplier;
                    GameManager.Instance.AddMana(finalYield);
                }
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
        bool canBuy = false;

        if (isFreeUnlock) canBuy = true;
        else canBuy = GameManager.Instance.SpendMana(currentCostToBuy);

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

    public void RecalculerStats()
    {
        if (myFloorData == null || GameManager.Instance == null) return;

        double discount = 1f - GameManager.Instance.costReductionBonus;
        double currentMana = GameManager.Instance.manaCurrent;
        
        levelsToBuy = 0;
        currentCostToBuy = 0;

        bool isFreeUnlock = (estLePremierEtage && currentLevel == 0);

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

            if (mode == BuyMode.MAX) CalculateMaxAffordable(currentLevel, currentMana, discount, out levelsToBuy, out currentCostToBuy);
            else
            {
                CalculateCostForLevels(currentLevel, targetLevelsToAdd, discount, out currentCostToBuy);
                levelsToBuy = targetLevelsToAdd;
            }
        }

        // --- LA MAGIE DU TEMPS ---
        double yield = myFloorData.baseProduction * currentLevel;
        float time = myFloorData.baseProductionTime;

        // Paliers de Vitesse
        int[] paliers = { 25, 50, 75, 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000 };

        foreach (int palier in paliers)
        {
            if (currentLevel >= palier)
            {
                time /= 2f; // On divise le temps par 2 à chaque palier atteint !
            }
        }

        // Si la barre va plus vite qu'1 seconde, on bloque le temps et on booste les gains
        if (time < 1f)
        {
            yield *= (1f / time);
            time = 1f;
        }

        currentProductionAmount = yield;
        currentProductionTime = time;

        // --- INTERFACE ---
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

        // Affichage dynamique (ex: 120 / 3s, ou 120 / sec)
        if (currentLevel == 0)
            productionText.text = "0 / sec";
        else
        {
            if (currentProductionTime <= 1f)
                productionText.text = ScoreUI.FormatNumber(currentProductionAmount) + " / sec";
            else if (currentProductionTime < 60f)
                productionText.text = ScoreUI.FormatNumber(currentProductionAmount) + " / " + Math.Round(currentProductionTime, 1) + "s";
            else
                productionText.text = ScoreUI.FormatNumber(currentProductionAmount) + " / " + Math.Round(currentProductionTime / 60f, 1) + "m";
        }

        GameManager.Instance.CalculerDPSGlobal();
    }

    public double ObtenirDPS()
    {
        if (currentProductionTime > 0) return currentProductionAmount / currentProductionTime;
        return 0;
    }

    private void CalculateCostForLevels(int startLevel, int levelsToAdd, double discount, out double totalCost)
    {
        double baseCost = myFloorData.baseCost;
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
        double baseCost = myFloorData.baseCost;
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

    private int ObtenirProchainPalier(int niveau)
    {
        if (niveau < 25) return 25;
        if (niveau < 50) return 50;
        if (niveau < 75) return 75;
        if (niveau < 100) return 100;
        return ((niveau / 100) + 1) * 100;
    }

    // Sauvegarde le timer quand on ferme le jeu pour ne pas perdre la production en cours
    void OnDisable()
    {
        if (myFloorData != null) PlayerPrefs.SetFloat("FloorTimer_" + myFloorData.name, timer);
    }
}