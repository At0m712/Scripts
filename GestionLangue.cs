using UnityEngine;

public class GestionLangue : MonoBehaviour
{
    public static GestionLangue Instance;
    
    [Header("Paramètres")]
    public string currentLanguage = "FR"; // "FR" ou "EN"

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        // Charge la langue sauvegardée ou utilise le français par défaut
        currentLanguage = PlayerPrefs.GetString("Language", "FR");
    }

    public void SwitchLanguage()
    {
        currentLanguage = (currentLanguage == "FR") ? "EN" : "FR";
        PlayerPrefs.SetString("Language", currentLanguage);
        PlayerPrefs.Save();
        
        Debug.Log("[GestionLangue] Langue changée en : " + currentLanguage);
        
        // Dans une version finale, vous déclencherez ici un Action/Event 
        // pour demander à tous les textes de l'UI de se recharger.
    }
}