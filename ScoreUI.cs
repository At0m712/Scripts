using UnityEngine;
using TMPro;
using System;

public class ScoreUI : MonoBehaviour
{
    public TextMeshProUGUI manaText;
    public TextMeshProUGUI crystalsText;
    public TextMeshProUGUI manaPerSecText;

    // Variables de cache pour l'optimisation
    private double lastMana = -1;
    private int lastCrystals = -1;
    private double lastProd = -1;

    void Update()
    {
        if (GameManager.Instance == null) return;

        // 1. Optimisation : On ne modifie le texte QUE si la valeur a changé
        if (Math.Floor(GameManager.Instance.manaCurrent) != Math.Floor(lastMana))
        {
            lastMana = GameManager.Instance.manaCurrent;
            manaText.text = FormatNumber(lastMana);
        }

        // 2. Optimisation : Cristaux
        if (GameManager.Instance.temporalCrystals != lastCrystals)
        {
            lastCrystals = GameManager.Instance.temporalCrystals;
            crystalsText.text = lastCrystals.ToString();
        }
        
        // 3. Calcul de la production
        double currentMulti = GameManager.Instance.globalMultiplier * GameManager.Instance.adBoostMultiplier;
        if (GameManager.Instance.IsRushActive) currentMulti *= GameManager.Instance.rushMultiplier;
        double actualProd = GameManager.Instance.manaPerSecond * currentMulti;

        // 4. Optimisation : Production / sec
        if (actualProd != lastProd)
        {
            lastProd = actualProd;
            manaPerSecText.text = "+ " + FormatNumber(actualProd) + " / sec";
        }
    }

    public static string FormatNumber(double value)
{
    // SÉCURITÉ 1 : Si le nombre est cassé ou négatif, on affiche 0
    if (double.IsNaN(value) || value < 0) return "0";
    
    // SÉCURITÉ 2 : Si le nombre est trop grand (Infini), on bloque
    if (double.IsInfinity(value)) return "MAX";

    if (value < 1000) return Math.Floor(value).ToString();

    string[] suffixes = { "", "k", "M", "B", "T", "aa", "ab", "ac", "ad", "ae", "af", "ag", "ah", "ai", "aj", "ak", "al", "am", "an", "ao", "ap", "aq", "ar", "as", "at", "au", "av", "aw", "ax", "ay", "az" };
    
    int suffixIndex = (int)Math.Floor(Math.Log10(value) / 3);

    // SÉCURITÉ 3 : On empêche l'index de sortir des limites de la liste "suffixes"
    if (suffixIndex < 0) suffixIndex = 0;
    if (suffixIndex >= suffixes.Length) suffixIndex = suffixes.Length - 1;

    double displayNum = value / Math.Pow(10, suffixIndex * 3);
    return displayNum.ToString("F2") + " " + suffixes[suffixIndex];
}
}