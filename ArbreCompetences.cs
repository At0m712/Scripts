using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class ArbreCompetences : MonoBehaviour
{
    [Header("Textes")]
    public TextMeshProUGUI cristauxDisponiblesText;
    
    [Header("Compétence 1 : Réduction de Coût")]
    public int coutReductionLevel = 0;
    public double coutReductionPriceBase = 10;
    public TextMeshProUGUI coutReductionLevelText;
    public TextMeshProUGUI coutReductionPriceText;
    public Button coutReductionBtn;

    [Header("Compétence 2 : Bonus Production")]
    public int prodBonusLevel = 0;
    public double prodBonusPriceBase = 25;
    public TextMeshProUGUI prodBonusLevelText;
    public TextMeshProUGUI prodBonusPriceText;
    public Button prodBonusBtn;

    // Cache pour éviter de calculer Math.Pow() 60 fois par seconde
    private double currentCoutReductionPrice;
    private double currentProdBonusPrice;

    void Start()
    {
        LoadCompetences();
        coutReductionBtn.onClick.AddListener(BuyCoutReduction);
        prodBonusBtn.onClick.AddListener(BuyProdBonus);
        
        // On vérifie si le joueur a l'argent 2 fois par sec (Ultra opti)
        InvokeRepeating(nameof(CheckButtonsState), 0.1f, 0.5f);
    }

    private void CheckButtonsState()
    {
        if (GameManager.Instance == null) return;
        
        double cristaux = GameManager.Instance.temporalCrystals;
        coutReductionBtn.interactable = (cristaux >= currentCoutReductionPrice);
        prodBonusBtn.interactable = (cristaux >= currentProdBonusPrice);
        
        cristauxDisponiblesText.text = "Cristaux : " + GameManager.Instance.temporalCrystals;
    }

    private double GetPrice(double basePrice, int level)
    {
        return Math.Floor(basePrice * Math.Pow(1.5f, level)); 
    }

    public void BuyCoutReduction()
    {
        if (GameManager.Instance.temporalCrystals >= currentCoutReductionPrice)
        {
            GameManager.Instance.temporalCrystals -= (int)currentCoutReductionPrice;
            coutReductionLevel++;
            SaveCompetences();
            
            if (AudioManager.Instance != null && AudioManager.Instance.buySound != null)
                AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.buySound);
        }
    }

    public void BuyProdBonus()
    {
        if (GameManager.Instance.temporalCrystals >= currentProdBonusPrice)
        {
            GameManager.Instance.temporalCrystals -= (int)currentProdBonusPrice;
            prodBonusLevel++;
            SaveCompetences();
            
            if (AudioManager.Instance != null && AudioManager.Instance.buySound != null)
                AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.buySound);
        }
    }

    private void SaveCompetences()
    {
        PlayerPrefs.SetInt("Arbre_CostReduc", coutReductionLevel);
        PlayerPrefs.SetInt("Arbre_ProdBonus", prodBonusLevel);
        PlayerPrefs.Save();
        
        GameManager.Instance.RecalculateMultiplier(); 
        UpdateUI();
        CheckButtonsState();
    }

    private void LoadCompetences()
    {
        coutReductionLevel = PlayerPrefs.GetInt("Arbre_CostReduc", 0);
        prodBonusLevel = PlayerPrefs.GetInt("Arbre_ProdBonus", 0);
        UpdateUI();
    }

    public void UpdateUI()
    {
        currentCoutReductionPrice = GetPrice(coutReductionPriceBase, coutReductionLevel);
        currentProdBonusPrice = GetPrice(prodBonusPriceBase, prodBonusLevel);

        coutReductionLevelText.text = "Niveau " + coutReductionLevel;
        coutReductionPriceText.text = currentCoutReductionPrice + " Cristaux";
        
        prodBonusLevelText.text = "Niveau " + prodBonusLevel;
        prodBonusPriceText.text = currentProdBonusPrice + " Cristaux";
    }
}