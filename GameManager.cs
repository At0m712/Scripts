using UnityEngine;
using System; 

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Ressources")]
    public double manaCurrent = 0;
    public double manaTotalProduced = 0;
    public double manaPerSecond = 0;
    public int temporalCrystals = 0;

    [Header("Multiplicateurs")]
    public double globalMultiplier = 1.0;
    public double adBoostMultiplier = 1.0;
    public double prestigeMultiplier = 1.0; 
    public float adBoostTimer = 0f;
    public float costReductionBonus = 0f;
    public double rushMultiplier = 10.0;
    public bool IsRushActive = false;

    private float rushTimer = 0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        manaPerSecond = 0;
        
        if (PlayerPrefs.HasKey("prestigeMultiplier"))
        {
            double.TryParse(PlayerPrefs.GetString("prestigeMultiplier", "1"), out prestigeMultiplier);
        }
        if (prestigeMultiplier < 1.0) prestigeMultiplier = 1.0;
    }

    void Start()
    {
        string dateFinString = PlayerPrefs.GetString("dateFinMultiplicateur", "");
        if (!string.IsNullOrEmpty(dateFinString) && DateTime.TryParse(dateFinString, out DateTime finBonus))
        {
            if (finBonus > DateTime.Now)
            {
                adBoostMultiplier = PlayerPrefs.GetInt("multiplicateurArgentActuel", 1);
                adBoostTimer = (float)(finBonus - DateTime.Now).TotalSeconds;
            }
            else
            {
                PlayerPrefs.SetInt("multiplicateurArgentActuel", 1);
                adBoostMultiplier = 1.0;
                adBoostTimer = 0f;
            }
        }
        RecalculateMultiplier();
    }

    void Update()
    {
        if (adBoostTimer > 0)
        {
            adBoostTimer -= Time.deltaTime;
            if (adBoostTimer <= 0)
            {
                adBoostTimer = 0;
                adBoostMultiplier = 1.0; 
            }
        }

        if (IsRushActive)
        {
            rushTimer -= Time.deltaTime;
            if (rushTimer <= 0)
            {
                IsRushActive = false;
            }
        }
    }

    public void AddMana(double amount)
    {
        manaCurrent += amount;
        manaTotalProduced += amount;
    }

    public bool SpendMana(double amount)
    {
        if (manaCurrent >= amount)
        {
            manaCurrent -= amount;
            return true;
        }
        return false;
    }

    public void ActivateRush(float duration)
    {
        IsRushActive = true;
        rushTimer = duration;
    }

    public void RecalculateMultiplier()
    {
        int prodBonusLevel = PlayerPrefs.GetInt("Arbre_ProdBonus", 0);
        int costReducLevel = PlayerPrefs.GetInt("Arbre_CostReduc", 0);

        double bonusCristauxPassif = temporalCrystals * 0.02;
        double baseMulti = 1.0 + (prodBonusLevel * 0.05); 
        globalMultiplier = (baseMulti + bonusCristauxPassif) * prestigeMultiplier; 
        
        costReductionBonus = Mathf.Min(0.50f, costReducLevel * 0.02f); 
    }

    // Le GameManager interroge tous les étages pour afficher le "+ X / sec" correct en haut de l'écran
    public void CalculerDPSGlobal()
    {
        double totalDPS = 0;
        UpgradeShopUI[] tousLesEtages = FindObjectsOfType<UpgradeShopUI>();
        
        foreach (var etage in tousLesEtages)
        {
            if (etage.currentLevel > 0)
            {
                totalDPS += etage.ObtenirDPS();
            }
        }
        manaPerSecond = totalDPS;
    }
}