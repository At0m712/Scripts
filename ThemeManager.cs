using UnityEngine;
using UnityEngine.UI;

public class ThemeManager : MonoBehaviour
{
    [Header("Référence")]
    public Image backgroundImage;

    [Header("Les Différents Fonds")]
    public Sprite themeNormal;
    public Sprite themeFeu;
    public Sprite themeGlace;
    public Sprite themeNeant;

    void Start()
    {
        UpdateTheme();
    }

    public void UpdateTheme()
    {
        if (GameManager.Instance == null) return;

        int cristaux = GameManager.Instance.temporalCrystals;

        // Le fond change automatiquement à mesure que le joueur devient puissant
        if (cristaux >= 100)
            backgroundImage.sprite = themeNeant;
        else if (cristaux >= 50)
            backgroundImage.sprite = themeGlace;
        else if (cristaux >= 10)
            backgroundImage.sprite = themeFeu;
        else
            backgroundImage.sprite = themeNormal;
    }
}