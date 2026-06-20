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
    public double globalMultiplier = 1.0;
    public double adBoostMultiplier = 1.0;
    public float adBoostTimer = 0f;
    public float costReductionBonus = 0f;
    public double rushMultiplier = 10.0;
    public bool IsRushActive = false;

    private float rushTimer = 0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Au lancement, on force la production à 0. 
        // Ce sont les étages (UpgradeShopUI) qui vont la recalculer juste après !
        manaPerSecond = 0;
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

        if (manaPerSecond > 0)
        {
            double currentMulti = globalMultiplier * adBoostMultiplier;
            if (IsRushActive) currentMulti *= rushMultiplier;

            double manaToAdd = (manaPerSecond * currentMulti) * Time.deltaTime;
            manaCurrent += manaToAdd;
            manaTotalProduced += manaToAdd;
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

        globalMultiplier = 1.0 + (prodBonusLevel * 0.05); 
        costReductionBonus = Mathf.Min(0.50f, costReducLevel * 0.02f); 
    }
}