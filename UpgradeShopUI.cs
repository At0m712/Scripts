using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class UpgradeShopUI : MonoBehaviour
{
    [Header("Données de l'étage")]
    public FloorData myFloorData; // Le ScriptableObject avec les infos de l'étage
    
    public int currentLevel = 0;
    
    [Header("UI Elements")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI productionText;
    public Button upgradeButton;
    public Image floorIconUI; 

    private double lastAddedProduction = 0;

    void Start()
    {
        if (myFloorData == null)
        {
            Debug.LogError("ATTENTION : Il manque le FloorData sur l'étage !");
            return;
        }

        // Charge le niveau sauvegardé
        currentLevel = PlayerPrefs.GetInt("Tour_" + myFloorData.floorName, 0);
        
        if (floorIconUI != null) 
            floorIconUI.sprite = myFloorData.iconEtage;
        
        UpdateGameManagerProduction();
        UpdateUI();

        // LIAISON AUTOMATIQUE DU BOUTON (C'est souvent ici que ça coince !)
        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveAllListeners(); // Sécurité
            upgradeButton.onClick.AddListener(BuyUpgrade);
        }
    }

    private double GetCurrentCost()
    {
        // On empêche le multiplicateur d'aller sous 1.05 (pour ne pas que ça devienne gratuit)
        float actualMultiplier = Mathf.Max(1.05f, myFloorData.baseCostMultiplier - GameManager.Instance.costReductionBonus);
        return myFloorData.baseCost * Math.Pow(actualMultiplier, currentLevel);
    }

    private double GetCurrentProduction()
    {
        if (currentLevel == 0) return 0;
        // Bonus spécial tous les 25 niveaux (multiplie la prod par 2)
        double palierBonus = Math.Pow(2, currentLevel / 25);
        return myFloorData.baseProduction * currentLevel * palierBonus;
    }

    private void UpdateGameManagerProduction()
    {
        if (GameManager.Instance == null) return;

        double newProduction = GetCurrentProduction();
        double difference = newProduction - lastAddedProduction;

        GameManager.Instance.manaPerSecond += difference;
        lastAddedProduction = newProduction;
    }

    // LA FONCTION D'ACHAT
    public void BuyUpgrade()
    {
        double cost = GetCurrentCost();

        // 1. On vérifie si le joueur a l'argent et on le dépense
        if (GameManager.Instance.SpendMana(cost))
        {
            // 2. On augmente le niveau
            currentLevel++;
            PlayerPrefs.SetInt("Tour_" + myFloorData.floorName, currentLevel);
            
            // 3. On met à jour la production et l'affichage
            UpdateGameManagerProduction();
            UpdateUI();
            
            // 4. Progression Quêtes
            if (QuestManager.Instance != null) QuestManager.Instance.AddUpgradeProgress();

            // 5. Son d'achat
            if (AudioManager.Instance != null && AudioManager.Instance.buySound != null)
            {
                AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.buySound);
            }

            // 6. Texte volant (- X Mana)
            if (ObjectPooler.Instance != null)
            {
                GameObject popup = ObjectPooler.Instance.SpawnFromPool(upgradeButton.transform.position);
                if (popup != null)
                {
                    TextMeshProUGUI textComp = popup.GetComponentInChildren<TextMeshProUGUI>();
                    if (textComp != null)
                    {
                        textComp.text = "- " + ScoreUI.FormatNumber(cost);
                        textComp.color = Color.red; 
                    }
                }
            }
        }
        else
        {
            // Son d'erreur si pas assez d'argent
            if (AudioManager.Instance != null && AudioManager.Instance.errorSound != null)
            {
                AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.errorSound);
            }
        }
    }

    private void UpdateUI()
    {
        nameText.text = myFloorData.floorName;
        levelText.text = "Lvl " + currentLevel;
        costText.text = ScoreUI.FormatNumber(GetCurrentCost());
        
        // Affiche la production actuelle, et ce que rapportera le niveau suivant
        productionText.text = "+ " + ScoreUI.FormatNumber(GetCurrentProduction() + myFloorData.baseProduction) + "/s";

        // Grise le bouton si le joueur n'a pas assez de mana
        if (upgradeButton != null && GameManager.Instance != null)
        {
            upgradeButton.interactable = (GameManager.Instance.manaCurrent >= GetCurrentCost());
        }
    }
}