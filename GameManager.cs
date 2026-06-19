using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Ressources")]
    public double manaCurrent = 0;
    public double manaTotalProduced = 0;
    public double manaPerSecond = 0;
    public int temporalCrystals = 0;
    
    [Header("Multiplicateurs")]
    public float globalMultiplier = 1f;
    public float adBoostMultiplier = 1f;
    public float adBoostTimer = 0f;
    public float costReductionBonus = 0f; 

    [Header("Surcharge (Rush)")]
    public float rushTimer = 0f;
    public float rushMultiplier = 10f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        // 1. Gestion du boost publicitaire cumulatif (Max 24h = 86400 secondes)
        if (adBoostTimer > 0)
        {
            adBoostTimer -= Time.deltaTime;
            adBoostMultiplier = 2f;
        }
        else
        {
            adBoostTimer = 0;
            adBoostMultiplier = 1f;
        }

        // 2. Gestion de la Surcharge (Rush)
        float currentRush = 1f;
        if (rushTimer > 0)
        {
            rushTimer -= Time.deltaTime;
            currentRush = rushMultiplier;
        }

        // 3. Calcul de la production
        double gain = (manaPerSecond * globalMultiplier * adBoostMultiplier * currentRush) * Time.deltaTime;
        manaCurrent += gain;
        manaTotalProduced += gain;
    }

    // Fonction à appeler depuis un nouveau bouton "Sort de Surcharge"
    public void ActivateRush()
    {
        rushTimer = 10f; // Active le boost x10 pendant 10 secondes
    }

    // Fonction pour ajouter du temps de boost (ex: +4 heures par pub)
    public void AddAdBoostTime(float secondsToAdd)
    {
        adBoostTimer += secondsToAdd;
        if (adBoostTimer > 86400f) adBoostTimer = 86400f; // Plafond à 24h
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
            return true; // Achat réussi
        }
        return false; // Pas assez d'argent
    }

    public void RecalculateMultiplier()
    {
        // Exemple : +50% par niveau d'amélioration de l'Arbre
        int prodBonusLevel = PlayerPrefs.GetInt("Arbre_ProdBonus", 0);
        globalMultiplier = 1f + (prodBonusLevel * 0.5f);
        
        // Réduction du coût (-1% par niveau)
        int costReducLevel = PlayerPrefs.GetInt("Arbre_CostReduc", 0);
        costReductionBonus = costReducLevel * 0.01f;
    }
}