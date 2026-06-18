using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Économie")]
    public double manaCurrent = 0;
    public double manaTotalProduced = 0;
    public double manaPerSecond = 0;
    
    [Header("Prestige")]
    public double temporalCrystals = 0;
    public float globalMultiplier = 1f;

    [Header("Boosts Pubs")]
    public float adBoostMultiplier = 1f;
    public float adBoostTimer = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        // Calcul du multiplicateur total (Base * Prestige * Pubs)
        float totalMultiplier = globalMultiplier * adBoostMultiplier;
        
        // Bonus des Cristaux Temporels (+2% par cristal)
        totalMultiplier += (float)(temporalCrystals * 0.02f); 

        // Ajout du mana chaque frame proportionnellement au temps écoulé
        double generatedThisFrame = (manaPerSecond * totalMultiplier) * Time.deltaTime;
        
        manaCurrent += generatedThisFrame;
        manaTotalProduced += generatedThisFrame;

        // Gestion du timer de publicité
        if (adBoostTimer > 0)
        {
            adBoostTimer -= Time.deltaTime;
            if (adBoostTimer <= 0) adBoostMultiplier = 1f; // Fin du boost
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
}