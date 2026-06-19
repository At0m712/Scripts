using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class UpgradeShopUI : MonoBehaviour
{
    [Header("Données de l'étage")]
    public FloorData myFloorData; // Glisse ton fichier "Donnees Etage" ici !
    
    public int currentLevel = 0;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI productionText;
    public Button upgradeButton;
    public Image floorIconUI; // L'image de l'étage
    private double lastAddedProduction = 0;

    void Start()
    {
        // Utilisation des données du ScriptableObject
        currentLevel = PlayerPrefs.GetInt("Tour_" + myFloorData.floorName, 0);
        
        if (floorIconUI != null) floorIconUI.sprite = myFloorData.iconEtage;
        
        UpdateGameManagerProduction();
        UpdateUI();
    }

    void Update()
    {
        // Grise le bouton si on n'a pas assez d'argent
        upgradeButton.interactable = (GameManager.Instance.manaCurrent >= GetCurrentCost());
    }

    private double GetCurrentCost()
    {
        float actualMultiplier = Mathf.Max(1.05f, myFloorData.baseCostMultiplier - GameManager.Instance.costReductionBonus);
        return myFloorData.baseCost * System.Math.Pow(actualMultiplier, currentLevel);
    }

    private double GetCurrentProduction()
    {
        if (currentLevel == 0) return 0;
        double palierBonus = System.Math.Pow(2, currentLevel / 25);
        return myFloorData.baseProduction * currentLevel * palierBonus;
    }

    private void UpdateUI()
    {
        nameText.text = myFloorData.floorName;
        levelText.text = "Lvl " + currentLevel;
        costText.text = ScoreUI.FormatNumber(GetCurrentCost());
        productionText.text = "+ " + ScoreUI.FormatNumber(GetCurrentProduction() + myFloorData.baseProduction) + "/s";
    }

    public void BuyUpgrade()
    {
        double cost = GetCurrentCost();
        if (GameManager.Instance.SpendMana(cost))
        {
            currentLevel++;
            
            // CORRECTION ICI : Utilisation de myFloorData.floorName
            PlayerPrefs.SetInt("Tour_" + myFloorData.floorName, currentLevel); 
            
            UpdateGameManagerProduction();
            UpdateUI();
            
            // JUS VISUEL ET SONORE COMPLÉTÉ
            if (AudioManager.Instance != null && AudioManager.Instance.buySound != null)
            {
                AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.buySound);
            }

            if (ObjectPooler.Instance != null)
            {
                // Fait spawn un texte flottant à la position du bouton
                GameObject popup = ObjectPooler.Instance.SpawnFromPool(upgradeButton.transform.position);
                if (popup != null)
                {
                    TextMeshProUGUI textComp = popup.GetComponentInChildren<TextMeshProUGUI>();
                    if (textComp != null)
                    {
                        textComp.text = "- " + ScoreUI.FormatNumber(cost);
                        textComp.color = Color.red; // Texte en rouge pour une dépense
                    }
                }
            }
        }
    }

    private void UpdateGameManagerProduction()
    {
        if (GameManager.Instance == null) return;

        // 1. Calcule la production totale de cet étage précis
        double newProduction = GetCurrentProduction();

        // 2. Calcule la différence avec l'ancienne production
        double difference = newProduction - lastAddedProduction;

        // 3. Ajoute uniquement cette différence au gain par seconde global
        GameManager.Instance.manaPerSecond += difference;

        // 4. Met à jour la mémoire de l'étage
        lastAddedProduction = newProduction;
    }
}