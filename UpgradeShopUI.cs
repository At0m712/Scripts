using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeShopUI : MonoBehaviour
{
    [Header("Configuration de l'Étage")]
    public string floorName;
    public int currentLevel = 0;
    public double baseCost = 10;
    public double baseProduction = 1;

    [Header("Interface")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI productionText;
    public Button upgradeButton;

    private double currentCost;
    private double currentProduction;

    private void Start()
    {
        upgradeButton.onClick.AddListener(BuyUpgrade);
        CalculateStats();
    }

    private void Update()
    {
        // Active le bouton uniquement si le joueur a assez de mana
        upgradeButton.interactable = (GameManager.Instance.manaCurrent >= currentCost);
    }

    private void CalculateStats()
    {
        // Formule du GDD : Cost = Base_Cost * (1.14 ^ Level)
        currentCost = baseCost * Mathf.Pow(1.14f, currentLevel);
        
        // Production de base
        currentProduction = baseProduction * currentLevel;

        // Paliers (Milestones) : la vitesse double (donc prod x2) tous les 25 niveaux
        int milestonesReached = currentLevel / 25;
        if (milestonesReached > 0)
        {
            currentProduction *= Mathf.Pow(2, milestonesReached);
        }

        UpdateUI();
        UpdateGlobalProduction();
    }

    public void BuyUpgrade()
    {
        if (GameManager.Instance.SpendMana(currentCost))
        {
            currentLevel++;
            CalculateStats();
        }
    }

    private void UpdateGlobalProduction()
    {
        // Recalcule la production totale (Simplifié pour le prototype)
        // Dans une version finale, GameManager devrait additionner tous les étages actifs
        UpgradeShopUI[] allFloors = FindObjectsOfType<UpgradeShopUI>();
        double totalMPS = 0;
        foreach (var floor in allFloors)
        {
            totalMPS += floor.currentProduction;
        }
        GameManager.Instance.manaPerSecond = totalMPS;
    }

    private void UpdateUI()
    {
        nameText.text = floorName;
        levelText.text = "Lvl " + currentLevel;
        
        ScoreUI scoreUI = FindObjectOfType<ScoreUI>();
        costText.text = "Coût: " + scoreUI.FormatNumber(currentCost);
        productionText.text = "+" + scoreUI.FormatNumber(currentProduction) + "/s";
    }
}