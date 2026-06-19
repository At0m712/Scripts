using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class UpgradeShopUI : MonoBehaviour
{
    [Header("Data")]
    public FloorData myFloorData;
    public int currentLevel = 0;
    
    [Header("UI References")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI productionText;
    public Button upgradeButton;
    public Image floorIconUI; 

    private double lastAddedProduction = 0;
    private double currentCostCache = 0;

    void Start()
    {
        if (myFloorData == null) return;

        // 1. Chargement de la sauvegarde
        currentLevel = PlayerPrefs.GetInt("Tour_" + myFloorData.floorName, 0);
        if (floorIconUI != null) floorIconUI.sprite = myFloorData.iconEtage;
        
        // 2. Connexion INFAILLIBLE du bouton
        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(BuyUpgrade);
        }

        // 3. Initialisation
        RecalculateValues();
        
        // 4. Vérification du prix 5 fois par seconde (Ultra Optimisé)
        InvokeRepeating(nameof(CheckButtonState), 0.1f, 0.2f);
    }

    private void RecalculateValues()
    {
        // Formule du Prix
        float actualMultiplier = Mathf.Max(1.05f, myFloorData.baseCostMultiplier - GameManager.Instance.costReductionBonus);
        currentCostCache = myFloorData.baseCost * Math.Pow(actualMultiplier, currentLevel);

        // Formule de la Production
        double palierBonus = Math.Pow(2, currentLevel / 25); // Bonus x2 tous les 25 niveaux
        double newProduction = currentLevel > 0 ? (myFloorData.baseProduction * currentLevel * palierBonus) : 0;

        // Mise à jour du GameManager
        double difference = newProduction - lastAddedProduction;
        GameManager.Instance.manaPerSecond += difference;
        lastAddedProduction = newProduction;

        // Mise à jour des Textes
        nameText.text = myFloorData.floorName;
        levelText.text = "Lvl " + currentLevel;
        costText.text = FormatNumber(currentCostCache);
        productionText.text = "+ " + FormatNumber(newProduction + myFloorData.baseProduction) + "/s";
    }

    private void CheckButtonState()
    {
        // Grise le bouton si pas assez de mana (sans surcharger le processeur)
        if (upgradeButton != null && GameManager.Instance != null)
        {
            upgradeButton.interactable = (GameManager.Instance.manaCurrent >= currentCostCache);
        }
    }

    public void BuyUpgrade()
    {
        // Si l'achat réussit via le GameManager
        if (GameManager.Instance.SpendMana(currentCostCache))
        {
            currentLevel++;
            PlayerPrefs.SetInt("Tour_" + myFloorData.floorName, currentLevel);
            
            RecalculateValues(); // Met à jour prix, prod et textes

            // Son de succès
            if (AudioManager.Instance != null && AudioManager.Instance.buySound != null)
                AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.buySound);
        }
        else
        {
            // Son d'erreur
            if (AudioManager.Instance != null && AudioManager.Instance.errorSound != null)
                AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.errorSound);
        }
    }

    // Petit formatage rapide intégré
    private string FormatNumber(double num)
    {
        if (num < 1000) return Math.Floor(num).ToString();
        if (num < 1000000) return (num / 1000d).ToString("F2") + " K";
        if (num < 1000000000) return (num / 1000000d).ToString("F2") + " M";
        return (num / 1000000000d).ToString("F2") + " B";
    }
}