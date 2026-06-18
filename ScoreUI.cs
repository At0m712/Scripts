using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    public TextMeshProUGUI manaText;
    public TextMeshProUGUI crystalsText;
    public TextMeshProUGUI manaPerSecText;

    private readonly string[] suffixes = { "", " K", " M", " B", " T", " AA", " AB", " AC", " AD", " AE", " AF" };

    private void Update()
    {
        if (GameManager.Instance == null) return;

        manaText.text = FormatNumber(GameManager.Instance.manaCurrent) + " Mana";
        crystalsText.text = FormatNumber(GameManager.Instance.temporalCrystals);
        
        float totalMult = GameManager.Instance.globalMultiplier + (float)(GameManager.Instance.temporalCrystals * 0.02f);
        manaPerSecText.text = "+" + FormatNumber(GameManager.Instance.manaPerSecond * totalMult) + "/sec";
    }

    // Fonction mathématique pour formater les grands nombres (ex: 1500000 -> 1.50 M)
    public string FormatNumber(double value)
    {
        if (value < 1000d) return value.ToString("0");

        int suffixIndex = 0;
        while (value >= 1000d && suffixIndex < suffixes.Length - 1)
        {
            value /= 1000d;
            suffixIndex++;
        }

        return value.ToString("0.00") + suffixes[suffixIndex];
    }
}