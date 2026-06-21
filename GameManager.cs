using UnityEngine;
using System; 

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Ressources")]
    public double manaCurrent = 0;
    public double manaTotalProduced = 0;
    public double manaPerSecond = 0; // Sert uniquement à l'affichage Global désormais !
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
        // On gère uniquement les chronos ici. 
        // L'AJOUT DE MANA A ÉTÉ SUPPRIMÉ ! Ce sont les étages qui gèrent.

        if (adBoostTimer > 0)
        {
            adBoostTimer -= Time.deltaTime;
            if (adBoostTimer <= 0)
            {
                adBoostTimer = 0;
                adBoostMultiplier = 1.0; 
                ActualiserTousLesEtages(); // On met à jour l'UI quand la pub se termine
            }
        }

        if (IsRushActive)
        {
            rushTimer -= Time.deltaTime;
            if (rushTimer <= 0)
            {
                IsRushActive = false;
                CalculerDPSGlobal(); // On remet le texte DPS normal
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
        CalculerDPSGlobal(); // Met à jour le texte visuel avec le x10
    }

    public void RecalculateMultiplier()
    {
        int prodBonusLevel = PlayerPrefs.GetInt("Arbre_ProdBonus", 0);
        int costReducLevel = PlayerPrefs.GetInt("Arbre_CostReduc", 0);

        double bonusCristauxPassif = temporalCrystals * 0.02;
        double baseMulti = 1.0 + (prodBonusLevel * 0.05); 
        
        // LE SEUL ENDROIT où le Prestige est calculé !
        globalMultiplier = (baseMulti + bonusCristauxPassif) * prestigeMultiplier; 
        
        costReductionBonus = Mathf.Min(0.50f, costReducLevel * 0.02f); 
    }

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

    public void ActualiserTousLesEtages()
    {
        UpgradeShopUI[] tousLesEtages = FindObjectsOfType<UpgradeShopUI>();
        foreach (var etage in tousLesEtages) etage.RecalculerStats();
    }
}