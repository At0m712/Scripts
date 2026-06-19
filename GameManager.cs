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
    public float globalMultiplier = 1f; // Lié à l'arbre de compétences
    public float adBoostMultiplier = 1f; // Pub (x2)
    public float adBoostTimer = 0f;
    public float costReductionBonus = 0f;

    [Header("Surcharge (Rush)")]
    public float rushMultiplier = 10f;
    private float rushTimer = 0f;
    public bool IsRushActive => rushTimer > 0;

    void Awake()
    {
        // Pattern Singleton sécurisé
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        // 1. Gestion du Timer de Surcharge
        if (rushTimer > 0)
        {
            rushTimer -= Time.deltaTime;
        }

        // 2. Calcul du gain par seconde (optimisé avec Time.deltaTime)
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
        if (double.IsNaN(amount) || amount <= 0) return; // Sécurité anti-bug
        manaCurrent += amount;
        manaTotalProduced += amount;
    }

    // FONCTION CRUCIALE POUR LES ACHATS
    public bool SpendMana(double amount)
    {
        if (manaCurrent >= amount && amount > 0)
        {
            manaCurrent -= amount;
            return true; // Achat autorisé
        }
        return false; // Fonds insuffisants
    }

    public void ActivateRush(float durationInSeconds)
    {
        rushTimer = durationInSeconds;
    }
}