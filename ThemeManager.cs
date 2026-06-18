using UnityEngine;
using UnityEngine.UI;

public class ThemeManager : MonoBehaviour
{
    public Image backgroundImage;
    
    [Header("Couleurs des Biomes (Prestige)")]
    public Color themeNormal;
    public Color themeFeu;
    public Color themeGlace;
    public Color themeNeant;

    private void Start()
    {
        ApplyTheme();
    }

    public void ApplyTheme()
    {
        // En fonction du nombre de prestiges, on change l'ambiance visuelle
        int prestigeCount = PlayerPrefs.GetInt("PrestigeCount", 0);

        if (prestigeCount == 0) backgroundImage.color = themeNormal;
        else if (prestigeCount == 1) backgroundImage.color = themeFeu;
        else if (prestigeCount == 2) backgroundImage.color = themeGlace;
        else backgroundImage.color = themeNeant;
    }
}