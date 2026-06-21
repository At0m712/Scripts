using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class ArbreCompetences : MonoBehaviour
{
    [Header("Texte d'En-tête")]
    public TextMeshProUGUI cristauxDisponiblesText;

    [Header("Système d'Onglets")]
    public GameObject panelCartes; // La fenêtre avec la grille de cartes
    public GameObject panelArbre;  // L'ancienne fenêtre avec les compétences globales
    public Button ongletCartesBtn;
    public Button ongletArbreBtn;
    
    [Header("Compétence 1 : Réduction de Coût Global")]
    public int coutReductionLevel = 0;
    public double coutReductionPriceBase = 10;
    public TextMeshProUGUI coutReductionLevelText;
    public TextMeshProUGUI coutReductionPriceText;
    public Button coutReductionBtn;

    [Header("Compétence 2 : Bonus Production Global")]
    public int prodBonusLevel = 0;
    public double prodBonusPriceBase = 25;
    public TextMeshProUGUI prodBonusLevelText;
    public TextMeshProUGUI prodBonusPriceText;
    public Button prodBonusBtn;

    private double currentCoutReductionPrice;
    private double currentProdBonusPrice;

    void Start()
    {
        LoadCompetences();
        
        if(coutReductionBtn != null) coutReductionBtn.onClick.AddListener(BuyCoutReduction);
        if(prodBonusBtn != null) prodBonusBtn.onClick.AddListener(BuyProdBonus);
        
        // --- GESTION DES ONGLETS ---
        if(ongletCartesBtn != null) ongletCartesBtn.onClick.AddListener(AfficherOngletCartes);
        if(ongletArbreBtn != null) ongletArbreBtn.onClick.AddListener(AfficherOngletArbre);
        
        AfficherOngletCartes(); // Ouvre les Cartes par défaut !
        
        InvokeRepeating(nameof(CheckButtonsState), 0.1f, 0.5f);
    }

    public void AfficherOngletCartes()
    {
        if (panelCartes != null) panelCartes.SetActive(true);
        if (panelArbre != null) panelArbre.SetActive(false);
    }

    public void AfficherOngletArbre()
    {
        if (panelCartes != null) panelCartes.SetActive(false);
        if (panelArbre != null) panelArbre.SetActive(true);
    }

    private void CheckButtonsState()
    {
        if (GameManager.Instance == null) return;
        
        double cristaux = GameManager.Instance.temporalCrystals;
        if(coutReductionBtn != null) coutReductionBtn.interactable = (cristaux >= currentCoutReductionPrice);
        if(prodBonusBtn != null) prodBonusBtn.interactable = (cristaux >= currentProdBonusPrice);
        
        MettreAJourEnTete();
    }

    public void MettreAJourEnTete()
    {
        if (cristauxDisponiblesText != null && GameManager.Instance != null)
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

        if(coutReductionLevelText != null) coutReductionLevelText.text = "Niveau " + coutReductionLevel;
        if(coutReductionPriceText != null) coutReductionPriceText.text = currentCoutReductionPrice + " Cristaux";
        
        if(prodBonusLevelText != null) prodBonusLevelText.text = "Niveau " + prodBonusLevel;
        if(prodBonusPriceText != null) prodBonusPriceText.text = currentProdBonusPrice + " Cristaux";
    }
}