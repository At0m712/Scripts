using UnityEngine;
using TMPro;
using System;

public class ScoreUI : MonoBehaviour
{
    public TextMeshProUGUI manaText;
    public TextMeshProUGUI crystalsText;
    public TextMeshProUGUI manaPerSecText;

    private double lastMana = -1;
    private int lastCrystals = -1;
    private double lastProd = -1;

    // Tableaux précalculés (Extrêmement rapide pour l'affichage)
    private static readonly string[] suffixes = { "", "k", "M", "B", "T", "aa", "ab", "ac", "ad", "ae", "af", "ag", "ah", "ai", "aj", "ak", "al", "am", "an", "ao", "ap", "aq", "ar", "as", "at", "au", "av", "aw", "ax", "ay", "az" };
    private static readonly double[] pow10 = { 1, 1e3, 1e6, 1e9, 1e12, 1e15, 1e18, 1e21, 1e24, 1e27, 1e30, 1e33, 1e36, 1e39, 1e42, 1e45, 1e48, 1e51, 1e54, 1e57, 1e60, 1e63, 1e66, 1e69, 1e72, 1e75, 1e78, 1e81, 1e84, 1e87, 1e90 };

    void Update()
    {
        if (GameManager.Instance == null) return;

        if (Math.Floor(GameManager.Instance.manaCurrent) != Math.Floor(lastMana))
        {
            lastMana = GameManager.Instance.manaCurrent;
            manaText.text = FormatNumber(lastMana);
        }

        if (GameManager.Instance.temporalCrystals != lastCrystals)
        {
            lastCrystals = GameManager.Instance.temporalCrystals;
            crystalsText.text = lastCrystals.ToString();
        }
        
        double actualProd = GameManager.Instance.manaPerSecond;
        if (actualProd != lastProd)
        {
            lastProd = actualProd;
            manaPerSecText.text = "+ " + FormatNumber(actualProd) + " / sec";
        }
    }

    public static string FormatNumber(double value)
    {
        if (double.IsNaN(value) || value < 0) return "0";
        if (double.IsInfinity(value)) return "MAX";
        if (value < 1000) return Math.Floor(value).ToString();

        int suffixIndex = (int)Math.Floor(Math.Log10(value) / 3);

        if (suffixIndex < 0) suffixIndex = 0;
        if (suffixIndex >= suffixes.Length) suffixIndex = suffixes.Length - 1;

        // Évite le Math.Pow(10, ...) très couteux à chaque frame
        double displayNum = value / pow10[suffixIndex];
        return displayNum.ToString("F2") + " " + suffixes[suffixIndex];
    }
}