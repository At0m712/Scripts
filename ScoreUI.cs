using UnityEngine;
using TMPro;
using UnityEngine.Localization;

public class ScoreUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI manaText;
    public TextMeshProUGUI crystalsText;
    public TextMeshProUGUI manaPerSecText;

    [Header("Localisation")]
    public LocalizedString texteCristaux; // Clé ex: Cristaux : {0}
    public LocalizedString texteManaParSec; // Clé ex: + {0} / sec

    void Update()
    {
        if (GameManager.Instance == null) return;

        // 1. Affichage du Mana (Pas besoin de traduction, ce sont juste des chiffres et des lettres)
        if (manaText != null)
        {
            manaText.text = FormatNumber(GameManager.Instance.manaCurrent);
        }

        // 2. Localisation des Cristaux
        if (crystalsText != null)
        {
            texteCristaux.Arguments = new object[] { GameManager.Instance.temporalCrystals };
            crystalsText.text = texteCristaux.GetLocalizedString();
        }

        // 3. Localisation du Mana par Seconde
        if (manaPerSecText != null)
        {
            double dps = GameManager.Instance.manaPerSecond;
            texteManaParSec.Arguments = new object[] { FormatNumber(dps) };
            manaPerSecText.text = texteManaParSec.GetLocalizedString();
        }
    }

    // Fonction universelle pour formater les grands nombres (1.5k, 2.3 AA, etc.)
    public static string FormatNumber(double value)
    {
        if (value < 1000) return System.Math.Floor(value).ToString();
        
        string[] suffixes = { "", "k", "M", "B", "T", "AA", "AB", "AC", "AD", "AE", "AF", "AG", "AH", "AI", "AJ" };
        int suffixIndex = 0;
        
        while (value >= 1000 && suffixIndex < suffixes.Length - 1)
        {
            value /= 1000;
            suffixIndex++;
        }
        
        return value.ToString("F2") + " " + suffixes[suffixIndex];
    }
}