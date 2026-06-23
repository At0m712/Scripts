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

    [Header("Multiplicateurs Globaux")]
    public double globalMultiplier = 1.0;
    public double adBoostMultiplier = 1.0;
    public double prestigeMultiplier = 1.0;
    public float adBoostTimer = 0f;
    public float costReductionBonus = 0f;

    [Header("Surcharge Temporelle (Rush)")]
    public double rushMultiplier = 10.0;
    public bool IsRushActive = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ChargerSauvegardeBasique();
        RecalculateMultiplier();
    }

    void Update()
    {
        // Gestion du chrono de la publicité
        if (adBoostTimer > 0)
        {
            adBoostTimer -= Time.deltaTime;
            if (adBoostTimer <= 0)
            {
                adBoostTimer = 0;
                adBoostMultiplier = 1.0;
                ActualiserTousLesEtages();
            }
        }
    }

    // ==========================================
    // 💰 GESTION DU MANA
    // ==========================================
    public void AddMana(double amount)
    {
        if (amount <= 0) return;
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

    // ==========================================
    // 🔄 ACTUALISATION DES ÉTAGES
    // ==========================================
    
    // Remplace l'ancien "UpgradeShopUI.AllShops"
    public void CalculerDPSGlobal()
    {
        double newDPS = 0;
        
        // On trouve tous les étages actifs dans la scène
        UpgradeShopUI[] tousLesEtages = FindObjectsOfType<UpgradeShopUI>();
        
        foreach (var etage in tousLesEtages)
        {
            newDPS += etage.ObtenirDPS();
        }
        
        manaPerSecond = newDPS;
    }

    public void ActualiserTousLesEtages()
    {
        UpgradeShopUI[] tousLesEtages = FindObjectsOfType<UpgradeShopUI>();
        
        foreach (var etage in tousLesEtages)
        {
            etage.RecalculerStats();
        }
    }


    // ==========================================
    // ⚙️ GESTION DES MULTIPLICATEURS ET ARBRE
    // ==========================================
    public void RecalculateMultiplier()
    {
        // 1. Calcul du Prestige
        if (PlayerPrefs.HasKey("prestigeMultiplier"))
        {
            string savedPrestige = PlayerPrefs.GetString("prestigeMultiplier", "1");
            double.TryParse(savedPrestige, out prestigeMultiplier);
            if (prestigeMultiplier < 1.0) prestigeMultiplier = 1.0;
        }
        
        // 2. Calcul du Bonus de Production Global (Arbre de compétences)
        int prodBonusLevel = PlayerPrefs.GetInt("Arbre_ProdBonus", 0);

        // 3. NOUVEAU : Lecture du Boost Gacha Permanent (Par défaut 1 = pas de boost)
        float gachaBoost = PlayerPrefs.GetFloat("GachaPermanentBoost", 1f);
        
        // La formule GLOBALE : (Prestige + Bonus Arbre) multiplié par la chance du Gacha
        globalMultiplier = (prestigeMultiplier + (prodBonusLevel * 0.10)) * gachaBoost; 
        
        // 4. Calcul de la Réduction de Coût (Arbre de compétences)
        int costReducLevel = PlayerPrefs.GetInt("Arbre_CostReduc", 0);
        
        // La formule : Niveau Arbre * 2% de réduction (max limité à -90% pour éviter le gratuit)
        costReductionBonus = costReducLevel * 0.02f; 
        if (costReductionBonus > 0.90f) costReductionBonus = 0.90f; 
        
        // 5. Mise à jour des valeurs dans la scène
        CalculerDPSGlobal();
        ActualiserTousLesEtages();
    }

    // ==========================================
    // 💾 SAUVEGARDE BASIQUE (En soutien du SaveManager)
    // ==========================================
    private void ChargerSauvegardeBasique()
    {
        if (PlayerPrefs.HasKey("manaCurrent"))
        {
            string savedMana = PlayerPrefs.GetString("manaCurrent", "0");
            double.TryParse(savedMana, out manaCurrent);
        }
        
        if (PlayerPrefs.HasKey("manaTotalProduced"))
        {
            string savedTotal = PlayerPrefs.GetString("manaTotalProduced", "0");
            double.TryParse(savedTotal, out manaTotalProduced);
        }
        
        temporalCrystals = PlayerPrefs.GetInt("temporalCrystals", 0);
    }
}