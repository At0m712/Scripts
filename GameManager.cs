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
        CalculerTempsEcoule(); // Restaure les boosts et calcule l'AFK
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

    private void CalculerTempsEcoule()
    {
        // 1. Restauration des Boosts Vidéo
        if (PlayerPrefs.HasKey("dateFinMultiplicateur"))
        {
            string dateFinStr = PlayerPrefs.GetString("dateFinMultiplicateur");
            DateTime dateFin;
            if (DateTime.TryParse(dateFinStr, out dateFin))
            {
                if (dateFin > DateTime.Now)
                {
                    // Le boost est encore valide !
                    adBoostTimer = (float)(dateFin - DateTime.Now).TotalSeconds;
                    adBoostMultiplier = PlayerPrefs.GetInt("multiplicateurArgentActuel", 1);
                }
                else
                {
                    adBoostTimer = 0f;
                    adBoostMultiplier = 1.0;
                }
            }
        }

        // 2. Calcul du temps Hors-Ligne pour l'écran de récompense
        if (PlayerPrefs.HasKey("LastLogoutTime"))
        {
            string lastLogoutStr = PlayerPrefs.GetString("LastLogoutTime");
            DateTime lastLogout;
            if (DateTime.TryParse(lastLogoutStr, out lastLogout))
            {
                TimeSpan tempsEcoule = DateTime.Now - lastLogout;
                double secondesDeconnecte = tempsEcoule.TotalSeconds;

                if (secondesDeconnecte >= 60 && OfflineGainsManager.Instance != null)
                {
                    OfflineGainsManager.Instance.AfficherGainsOffline(secondesDeconnecte);
                }
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
    public void CalculerDPSGlobal()
    {
        double newDPS = 0;
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

        // 3. Lecture du Boost Gacha Permanent
        float gachaBoost = PlayerPrefs.GetFloat("GachaPermanentBoost", 1f);
        
        // Formule globale avec la chance du Gacha intégrée
        globalMultiplier = (prestigeMultiplier + (prodBonusLevel * 0.10)) * gachaBoost; 
        
        // 4. Calcul de la Réduction de Coût (Arbre de compétences)
        int costReducLevel = PlayerPrefs.GetInt("Arbre_CostReduc", 0);
        costReductionBonus = costReducLevel * 0.02f; 
        if (costReductionBonus > 0.90f) costReductionBonus = 0.90f; 
        
        CalculerDPSGlobal();
        ActualiserTousLesEtages();
    }

    // ==========================================
    // 💾 SAUVEGARDE BASIQUE
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