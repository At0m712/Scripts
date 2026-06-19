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

    void Start()
    {
        LoadCompetences();
        coutReductionBtn.onClick.AddListener(BuyCoutReduction);
        prodBonusBtn.onClick.AddListener(BuyProdBonus);
    }

    void Update()
    {
        // Grise les boutons si pas assez de cristaux
        double cristaux = GameManager.Instance.temporalCrystals;
        coutReductionBtn.interactable = (cristaux >= GetPrice(coutReductionPriceBase, coutReductionLevel));
        prodBonusBtn.interactable = (cristaux >= GetPrice(prodBonusPriceBase, prodBonusLevel));
    }

    // Le prix augmente de 50% à chaque niveau (Ex: 10, 15, 22, 33...)
    private double GetPrice(double basePrice, int level)
    {
        return Math.Floor(basePrice * Math.Pow(1.5f, level)); 
    }

    public void BuyCoutReduction()
    {
        double price = GetPrice(coutReductionPriceBase, coutReductionLevel);
        if (GameManager.Instance.temporalCrystals >= price)
        {
            GameManager.Instance.temporalCrystals -= (int)price;
            coutReductionLevel++;
            SaveCompetences();
        }
    }

    public void BuyProdBonus()
    {
        double price = GetPrice(prodBonusPriceBase, prodBonusLevel);
        if (GameManager.Instance.temporalCrystals >= price)
        {
            GameManager.Instance.temporalCrystals -= (int)price;
            prodBonusLevel++;
            SaveCompetences();
        }
    }

    private void SaveCompetences()
    {
        PlayerPrefs.SetInt("Arbre_CostReduc", coutReductionLevel);
        PlayerPrefs.SetInt("Arbre_ProdBonus", prodBonusLevel);
        PlayerPrefs.Save();
        
        GameManager.Instance.RecalculateMultiplier(); // Applique le bonus !
        UpdateUI();
    }

    private void LoadCompetences()
    {
        coutReductionLevel = PlayerPrefs.GetInt("Arbre_CostReduc", 0);
        prodBonusLevel = PlayerPrefs.GetInt("Arbre_ProdBonus", 0);
        UpdateUI();
    }

    public void UpdateUI()
    {
        cristauxDisponiblesText.text = "Cristaux : " + GameManager.Instance.temporalCrystals;
        
        coutReductionLevelText.text = "Niveau " + coutReductionLevel;
        coutReductionPriceText.text = GetPrice(coutReductionPriceBase, coutReductionLevel) + " Cristaux";
        
        prodBonusLevelText.text = "Niveau " + prodBonusLevel;
        prodBonusPriceText.text = GetPrice(prodBonusPriceBase, prodBonusLevel) + " Cristaux";
    }
}