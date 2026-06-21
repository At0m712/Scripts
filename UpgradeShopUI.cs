using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.Localization;

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
    public GameObject objetChevrons; 

    [Header("Localisation des Textes Dynamiques")]
    public LocalizedString texteNiveau; 
    public LocalizedString texteGratuit; 
    public LocalizedString texteDebloquer; 
    public LocalizedString textePlusMax; 
    public LocalizedString textePlus; 
    public LocalizedString texteZeroSec; 
    public LocalizedString texteParSec; 
    public LocalizedString texteParSecArrondi; 
    public LocalizedString texteParMinArrondi; 

    private double currentCostToBuy; 
    private int levelsToBuy; 
    private double currentBaseYield; 
    private float currentProductionTime;
    private float timer = 0f;

    void Start()
    {
        if (myFloorData == null) return;
        
        int minLevels = PlayerPrefs.GetInt("BonusLevels_" + myFloorData.name, 0);
        currentLevel = PlayerPrefs.GetInt("FloorLevel_" + myFloorData.name, minLevels);

        timer = PlayerPrefs.GetFloat("FloorTimer_" + myFloorData.name, 0f);
        if (nameText != null) nameText.text = myFloorData.name; // Le nom est géré par la trad du FloorData
        
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
            {
                speedMultiplier = (float)GameManager.Instance.rushMultiplier;
            }

            timer += Time.deltaTime * speedMultiplier;

            if (currentProductionTime <= 1f)
            {
                if (jaugeProgression != null) jaugeProgression.fillAmount = 1f;
                if (objetChevrons != null) objetChevrons.SetActive(true);
            }
            else
            {
                if (jaugeProgression != null) jaugeProgression.fillAmount = timer / currentProductionTime;
                if (objetChevrons != null) objetChevrons.SetActive(false);
            }

            if (timer >= currentProductionTime)
            {
                timer -= currentProductionTime; 
                if (GameManager.Instance != null) 
                {
                    GameManager.Instance.AddMana(GetActualYield());
                }
            }
        }
        else
        {
            if (jaugeProgression != null) jaugeProgression.fillAmount = 0f;
            if (objetChevrons != null) objetChevrons.SetActive(false);
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

    private double GetDiscountedBaseCost()
    {
        float cardDiscount = PlayerPrefs.GetFloat("BonusCost_" + myFloorData.name, 1f); 
        return myFloorData.baseCost * cardDiscount;
    }

    public void RecalculerStats()
    {
        if (myFloorData == null || GameManager.Instance == null) return;

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
                CalculateMaxAffordable(currentLevel, currentMana, discount, out levelsToBuy, out currentCostToBuy);
            else
            {
                CalculateCostForLevels(currentLevel, targetLevelsToAdd, discount, out currentCostToBuy);
                levelsToBuy = targetLevelsToAdd;
            }
        }

        double baseYield = 0;
        float time = myFloorData.baseProductionTime;

        if (currentLevel > 0)
        {
            float cardProdMulti = PlayerPrefs.GetFloat("BonusProd_" + myFloorData.name, 1f);
            baseYield = myFloorData.baseProduction * currentLevel * cardProdMulti;

            int[] paliers = { 25, 50, 75, 100, 150, 200, 250, 300, 400, 500, 600, 700, 800, 900, 1000 };
            
            foreach (int palier in paliers)
            {
                if (currentLevel >= palier) 
                {
                    if (time > 0.1f) time /= 2f;
                    else baseYield *= 2f;
                }
            }

            if (time < 0.1f && time > 0f)
            {
                baseYield *= (0.1f / time);
                time = 0.1f;
            }
        }

        currentBaseYield = baseYield;
        currentProductionTime = time;

        // LOCALISATION NIVEAU
        texteNiveau.Arguments = new object[] { currentLevel };
        levelText.text = texteNiveau.GetLocalizedString();
        
        // LOCALISATION BOUTON ACHAT
        if (isFreeUnlock)
        {
            costText.text = texteGratuit.GetLocalizedString();
            if (upgradeButtonText != null) upgradeButtonText.text = texteDebloquer.GetLocalizedString();
        }
        else if (levelsToBuy > 0)
        {
            costText.text = ScoreUI.FormatNumber(currentCostToBuy);
            BuyMode mode = BuyModeManager.Instance != null ? BuyModeManager.Instance.currentMode : BuyMode.x1;
            
            if (upgradeButtonText != null) 
            {
                if (mode == BuyMode.MAX)
                {
                    textePlusMax.Arguments = new object[] { levelsToBuy };
                    upgradeButtonText.text = textePlusMax.GetLocalizedString();
                }
                else
                {
                    textePlus.Arguments = new object[] { levelsToBuy };
                    upgradeButtonText.text = textePlus.GetLocalizedString();
                }
            }
        }
        else
        {
            CalculateCostForLevels(currentLevel, 1, discount, out double costForOne);
            costText.text = ScoreUI.FormatNumber(costForOne);
            
            if (upgradeButtonText != null) 
            {
                textePlus.Arguments = new object[] { 1 };
                upgradeButtonText.text = textePlus.GetLocalizedString();
            }
        }

        // LOCALISATION DPS
        double yieldToDisplay = GetActualYield();

        if (currentLevel == 0) 
        {
            productionText.text = texteZeroSec.GetLocalizedString();
        }
        else
        {
            if (currentProductionTime <= 1f) 
            {
                texteParSec.Arguments = new object[] { ScoreUI.FormatNumber(yieldToDisplay / currentProductionTime) };
                productionText.text = texteParSec.GetLocalizedString();
            }
            else if (currentProductionTime < 60f) 
            {
                int tempsArrondi = Mathf.RoundToInt(currentProductionTime);
                texteParSecArrondi.Arguments = new object[] { ScoreUI.FormatNumber(yieldToDisplay), tempsArrondi };
                productionText.text = texteParSecArrondi.GetLocalizedString();
            }
            else 
            {
                int tempsArrondiMin = Mathf.RoundToInt(currentProductionTime / 60f);
                texteParMinArrondi.Arguments = new object[] { ScoreUI.FormatNumber(yieldToDisplay), tempsArrondiMin };
                productionText.text = texteParMinArrondi.GetLocalizedString();
            }
        }

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