using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Localization;

public class ArbreCompetences : MonoBehaviour
{
    [Header("UI Production Bonus")]
    public TextMeshProUGUI prodBonusLevelText;
    public TextMeshProUGUI prodBonusCostText;
    public Button buyProdBonusButton;

    [Header("UI Cost Reduction")]
    public TextMeshProUGUI costReducLevelText;
    public TextMeshProUGUI costReducCostText;
    public Button buyCostReducButton;

    [Header("Localisation")]
    public LocalizedString texteNiveauMax; // Clé ex: Niv. {0} / {1}
    public LocalizedString textePrixCristaux; // Clé ex: {0} Cristaux
    public LocalizedString texteMaxAtteint; // Clé ex: MAX

    private int prodBonusLevel = 0;
    private int costReducLevel = 0;

    private int maxProdBonusLevel = 50; 
    private int maxCostReducLevel = 25; 

    void Start()
    {
        prodBonusLevel = PlayerPrefs.GetInt("Arbre_ProdBonus", 0);
        costReducLevel = PlayerPrefs.GetInt("Arbre_CostReduc", 0);

        buyProdBonusButton.onClick.AddListener(BuyProdBonus);
        buyCostReducButton.onClick.AddListener(BuyCostReduc);

        UpdateUI();
    }

    void Update()
    {
        // Mise à jour des boutons en temps réel selon les cristaux
        if (GameManager.Instance != null)
        {
            buyProdBonusButton.interactable = (prodBonusLevel < maxProdBonusLevel) && (GameManager.Instance.temporalCrystals >= GetProdBonusCost());
            buyCostReducButton.interactable = (costReducLevel < maxCostReducLevel) && (GameManager.Instance.temporalCrystals >= GetCostReducCost());
        }
    }

    private int GetProdBonusCost() { return 5 * (prodBonusLevel + 1); }
    private int GetCostReducCost() { return 10 * (costReducLevel + 1); }

    private void UpdateUI()
    {
        // LOCALISATION BONUS PRODUCTION
        texteNiveauMax.Arguments = new object[] { prodBonusLevel, maxProdBonusLevel };
        prodBonusLevelText.text = texteNiveauMax.GetLocalizedString();

        if (prodBonusLevel < maxProdBonusLevel)
        {
            textePrixCristaux.Arguments = new object[] { GetProdBonusCost() };
            prodBonusCostText.text = textePrixCristaux.GetLocalizedString();
        }
        else
        {
            prodBonusCostText.text = texteMaxAtteint.GetLocalizedString();
        }

        // LOCALISATION RÉDUCTION DE COÛT
        texteNiveauMax.Arguments = new object[] { costReducLevel, maxCostReducLevel };
        costReducLevelText.text = texteNiveauMax.GetLocalizedString();

        if (costReducLevel < maxCostReducLevel)
        {
            textePrixCristaux.Arguments = new object[] { GetCostReducCost() };
            costReducCostText.text = textePrixCristaux.GetLocalizedString();
        }
        else
        {
            costReducCostText.text = texteMaxAtteint.GetLocalizedString();
        }
    }

    public void BuyProdBonus()
    {
        int cost = GetProdBonusCost();
        if (GameManager.Instance.temporalCrystals >= cost && prodBonusLevel < maxProdBonusLevel)
        {
            GameManager.Instance.temporalCrystals -= cost;
            prodBonusLevel++;
            PlayerPrefs.SetInt("Arbre_ProdBonus", prodBonusLevel);
            PlayerPrefs.SetInt("temporalCrystals", GameManager.Instance.temporalCrystals);
            
            FinaliserAchat();
        }
    }

    public void BuyCostReduc()
    {
        int cost = GetCostReducCost();
        if (GameManager.Instance.temporalCrystals >= cost && costReducLevel < maxCostReducLevel)
        {
            GameManager.Instance.temporalCrystals -= cost;
            costReducLevel++;
            PlayerPrefs.SetInt("Arbre_CostReduc", costReducLevel);
            PlayerPrefs.SetInt("temporalCrystals", GameManager.Instance.temporalCrystals);
            
            FinaliserAchat();
        }
    }

    private void FinaliserAchat()
    {
        PlayerPrefs.Save();
        GameManager.Instance.RecalculateMultiplier();
        GameManager.Instance.ActualiserTousLesEtages();

        if (AudioManager.Instance != null && AudioManager.Instance.buySound != null)
            AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.buySound);

        UpdateUI();
    }
}