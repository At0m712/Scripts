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
    public float globalMultiplier = 1f; 
    public float adBoostMultiplier = 1f; 
    public float adBoostTimer = 0f; // Réintégré pour la pub
    public float costReductionBonus = 0f;

    [Header("Surcharge (Rush)")]
    public float rushMultiplier = 10f;
    private float rushTimer = 0f;
    public bool IsRushActive => rushTimer > 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        // 1. Gestion du Boost Pub
        if (adBoostTimer > 0)
        {
            adBoostTimer -= Time.deltaTime;
            adBoostMultiplier = 2f;
        }
        else
        {
            adBoostMultiplier = 1f;
        }

        // 2. Gestion du Timer de Surcharge
        if (rushTimer > 0)
        {
            rushTimer -= Time.deltaTime;
        }

        // 3. Production de mana par seconde
        if (manaPerSecond > 0)
        {
            double currentMulti = globalMultiplier * adBoostMultiplier;
            if (IsRushActive) currentMulti *= rushMultiplier;

            double manaToGive = (manaPerSecond * currentMulti) * Time.deltaTime;
            AddMana(manaToGive);
        }
    }

    public void AddMana(double amount)
    {
        if (double.IsNaN(amount) || amount <= 0) return; 
        manaCurrent += amount;
        manaTotalProduced += amount;
    }

    public bool SpendMana(double amount)
    {
        if (manaCurrent >= amount && amount > 0)
        {
            manaCurrent -= amount;
            return true; 
        }
        return false; 
    }

    public void ActivateRush(float durationInSeconds)
    {
        rushTimer = durationInSeconds;
    }

    // RÉINTÉGRATION DE LA FONCTION POUR L'ARBRE DE COMPÉTENCES
    public void RecalculateMultiplier()
    {
        int prodBonusLevel = PlayerPrefs.GetInt("Arbre_ProdBonus", 0);
        globalMultiplier = 1f + (prodBonusLevel * 0.5f);
        
        int costReducLevel = PlayerPrefs.GetInt("Arbre_CostReduc", 0);
        costReductionBonus = costReducLevel * 0.01f;
    }
}