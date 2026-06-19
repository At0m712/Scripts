using UnityEngine;
using TMPro;
using System;

public class ScoreUI : MonoBehaviour
{
    public TextMeshProUGUI manaText;
    public TextMeshProUGUI crystalsText;
    public TextMeshProUGUI manaPerSecText;

    void Update()
    {
        if (GameManager.Instance == null) return;

        manaText.text = FormatNumber(GameManager.Instance.manaCurrent);
        crystalsText.text = GameManager.Instance.temporalCrystals.ToString();
        
        double actualProd = GameManager.Instance.manaPerSecond * GameManager.Instance.globalMultiplier * GameManager.Instance.adBoostMultiplier;
        manaPerSecText.text = "+ " + FormatNumber(actualProd) + " / sec";
    }

    // Le fameux formateur de nombres
    public static string FormatNumber(double value)
    {
        if (value < 1000) return Math.Floor(value).ToString();

        string[] suffixes = { "", "K", "M", "B", "T", "AA", "AB", "AC", "AD", "AE" };
        int suffixIndex = (int)Math.Floor(Math.Log10(value) / 3);
        
        if (suffixIndex >= suffixes.Length) suffixIndex = suffixes.Length - 1;

        double displayValue = value / Math.Pow(10, suffixIndex * 3);
        return displayValue.ToString("F2") + " " + suffixes[suffixIndex];
    }
}