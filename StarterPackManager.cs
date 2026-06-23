using UnityEngine;
using UnityEngine.UI;

public class StarterPackManager : MonoBehaviour
{
    [Header("UI")]
    public Button starterPackButton;
    public int levelRequiredToShow = 10;

    private void Start()
    {
        VerifierAchat();
    }

    private void VerifierAchat()
    {
        // Si le pack a déjà été acheté, on cache le bouton définitivement
        if (PlayerPrefs.GetInt("StarterPackAchete", 0) == 1)
        {
            if (starterPackButton != null) starterPackButton.gameObject.SetActive(false);
        }
        else
        {
            if (starterPackButton != null) starterPackButton.gameObject.SetActive(true);
        }
    }

    // ==========================================
    // 💳 SUCCÈS DE L'ACHAT (À lier à ton IAP Button)
    // ==========================================
    public void BuyStarterPackSuccessful()
    {
        if (GameManager.Instance == null) return;

        // 1. Gain des Cristaux
        GameManager.Instance.temporalCrystals += 50; 
        PlayerPrefs.SetInt("temporalCrystals", GameManager.Instance.temporalCrystals);

        // 2. Gain du Mana (24h de production, ou 50 000 minimum)
        double dpsActuel = GameManager.Instance.manaPerSecond;
        
        // 🛡️ SÉCURITÉ : Évite le bug du gain = 0 si le joueur achète le pack dès la première seconde
        double manaGagne = (dpsActuel > 0) ? dpsActuel * 86400 : 50000; // 86400 = 24 heures en secondes
        
        GameManager.Instance.AddMana(manaGagne); 

        // 3. Enregistrement définitif de l'achat
        PlayerPrefs.SetInt("StarterPackAchete", 1);
        PlayerPrefs.Save();

        // 4. On cache le bouton
        if (starterPackButton != null) starterPackButton.gameObject.SetActive(false);

        // 5. On demande la sauvegarde générale
        if (SaveManager.Instance != null) SaveManager.Instance.DemanderSauvegarde();
    }
}