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
    public float costReductionBonus = 0f; // Réduction venant de l'arbre

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        // Gestion du boost publicitaire
        if (adBoostTimer > 0)
        {
            adBoostTimer -= Time.deltaTime;
            adBoostMultiplier = 2f;
        }
        else
        {
            adBoostMultiplier = 1f;
        }

        // Production de mana
        double gain = (manaPerSecond * globalMultiplier * adBoostMultiplier) * Time.deltaTime;
        manaCurrent += gain;
        manaTotalProduced += gain;
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