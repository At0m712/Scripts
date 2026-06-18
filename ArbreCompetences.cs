using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ArbreCompetences : MonoBehaviour
{
    [Header("UI Textes Généraux")]
    public TextMeshProUGUI cristauxDisponiblesText;
    
    [Header("Compétence 1 : Réduction Coût des Étages")]
    public TextMeshProUGUI coutReductionLevelText;
    public TextMeshProUGUI coutReductionPriceText;
    public Button coutReductionBtn;
    public int coutReductionLevel = 0;
    private double coutReductionPriceBase = 10; // Prix de départ en cristaux

    [Header("Compétence 2 : Bonus Production Globale")]
    public TextMeshProUGUI prodBonusLevelText;
    public TextMeshProUGUI prodBonusPriceText;
    public Button prodBonusBtn;
    public int prodBonusLevel = 0;
    private double prodBonusPriceBase = 25; // Prix de départ en cristaux

    private void Start()
    {
        LoadCompetences();
        
        coutReductionBtn.onClick.AddListener(BuyCoutReduction);
        prodBonusBtn.onClick.AddListener(BuyProdBonus);
        
        UpdateUI();
    }

    private void Update()
    {
        // Mise à jour de l'interactivité des boutons selon les cristaux disponibles
        double cristaux = GameManager.Instance.temporalCrystals;
        coutReductionBtn.interactable = (cristaux >= GetPrice(coutReductionPriceBase, coutReductionLevel));
        prodBonusBtn.interactable = (cristaux >= GetPrice(prodBonusPriceBase, prodBonusLevel));
    }

    // L'inflation des prix dans l'arbre de talent (coûte 1.5x plus cher à chaque niveau)
    private double GetPrice(double basePrice, int level)
    {
        return basePrice * Mathf.Pow(1.5f, level); 
    }

    public void BuyCoutReduction()
    {
        double price = GetPrice(coutReductionPriceBase, coutReductionLevel);
        if (GameManager.Instance.temporalCrystals >= price)
        {
            GameManager.Instance.temporalCrystals -= price;
            coutReductionLevel++;
            SaveCompetences();
            UpdateUI();
            
            // Note: Pour appliquer la réduction, votre script UpgradeShopUI devra lire `coutReductionLevel`
        }
    }

    public void BuyProdBonus()
    {
        double price = GetPrice(prodBonusPriceBase, prodBonusLevel);
        if (GameManager.Instance.temporalCrystals >= price)
        {
            GameManager.Instance.temporalCrystals -= price;
            prodBonusLevel++;
            ApplyCompetences();
            SaveCompetences();
            UpdateUI();
        }
    }

    private void ApplyCompetences()
    {
        // Chaque niveau donne +10% de production de base dans le GameManager
        GameManager.Instance.globalMultiplier = 1f + (prodBonusLevel * 0.1f); 
    }

    private void UpdateUI()
    {
        ScoreUI scoreUI = FindObjectOfType<ScoreUI>();
        
        if(scoreUI != null)
        {
            cristauxDisponiblesText.text = "Cristaux: " + scoreUI.FormatNumber(GameManager.Instance.temporalCrystals);
            
            coutReductionPriceText.text = scoreUI.FormatNumber(GetPrice(coutReductionPriceBase, coutReductionLevel)) + " Cristaux";
            prodBonusPriceText.text = scoreUI.FormatNumber(GetPrice(prodBonusPriceBase, prodBonusLevel)) + " Cristaux";
        }

        coutReductionLevelText.text = "Niveau " + coutReductionLevel;
        prodBonusLevelText.text = "Niveau " + prodBonusLevel;
    }

    private void SaveCompetences()
    {
        PlayerPrefs.SetInt("Skill_CoutReduction", coutReductionLevel);
        PlayerPrefs.SetInt("Skill_ProdBonus", prodBonusLevel);
        PlayerPrefs.Save();
    }

    private void LoadCompetences()
    {
        coutReductionLevel = PlayerPrefs.GetInt("Skill_CoutReduction", 0);
        prodBonusLevel = PlayerPrefs.GetInt("Skill_ProdBonus", 0);
        ApplyCompetences();
    }
}