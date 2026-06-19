using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class UpgradeShopUI : MonoBehaviour
{
    public string floorName;
    public int currentLevel = 0;
    public double baseCost = 10;
    public float baseCostMultiplier = 1.15f;
    public double baseProduction = 1;

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI productionText;
    public Button upgradeButton;

    void Start()
    {
        currentLevel = PlayerPrefs.GetInt("Tour_" + floorName, 0);
        UpdateGameManagerProduction();
        UpdateUI();
    }

    void Update()
    {
        // Grise le bouton si on n'a pas assez d'argent
        upgradeButton.interactable = (GameManager.Instance.manaCurrent >= GetCurrentCost());
    }

    public void BuyUpgrade()
    {
        double cost = GetCurrentCost();
        if (GameManager.Instance.SpendMana(cost))
        {
            currentLevel++;
            PlayerPrefs.SetInt("Tour_" + floorName, currentLevel);
            UpdateGameManagerProduction();
            UpdateUI();
            
            // TODO: Jouer le son d'achat et la particule ici
        }
    }

    private double GetCurrentCost()
    {
        float actualMultiplier = Mathf.Max(1.05f, baseCostMultiplier - GameManager.Instance.costReductionBonus);
        return baseCost * Math.Pow(actualMultiplier, currentLevel);
    }

    private double GetCurrentProduction()
    {
        if (currentLevel == 0) return 0;
        // x2 tous les 25 niveaux
        double palierBonus = Math.Pow(2, currentLevel / 25);
        return baseProduction * currentLevel * palierBonus;
    }

    private void UpdateUI()
    {
        nameText.text = floorName;
        levelText.text = "Lvl " + currentLevel;
        costText.text = ScoreUI.FormatNumber(GetCurrentCost());
        productionText.text = "+ " + ScoreUI.FormatNumber(GetCurrentProduction() + baseProduction) + "/s"; // Affiche la prochaine prod
    }

    private void UpdateGameManagerProduction()
    {
        // Cette fonction recalculera la production totale de la tour. 
        // Dans une version complète, le GameManager boucle sur tous les étages.
    }
}