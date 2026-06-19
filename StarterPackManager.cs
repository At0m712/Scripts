using UnityEngine;
using UnityEngine.UI;

public class StarterPackManager : MonoBehaviour
{
    [Header("UI Pack de Démarrage")]
    public GameObject starterPackButton; // Le bouton HUD avec animation PulseGlow
    public int levelRequiredToShow = 10;

    void Start()
    {
        CheckStarterPackVisibility();
    }

    public void CheckStarterPackVisibility()
    {
        // Si le pack a déjà été acheté, on le cache définitivement
        if (PlayerPrefs.GetInt("StarterPackBought", 0) == 1)
        {
            starterPackButton.SetActive(false);
            return;
        }

        // On vérifie le niveau de l'étage 1 (Apprenti Sorcier)
        int currentLevel = PlayerPrefs.GetInt("Tour_Apprenti Sorcier", 0);
        
        if (currentLevel >= levelRequiredToShow)
        {
            starterPackButton.SetActive(true);
        }
        else
        {
            starterPackButton.SetActive(false);
        }
    }

    // À lier au bouton d'achat en argent réel (Via Unity IAP)
    public void BuyStarterPackSuccessful()
    {
        // Récompense massive pour fidéliser le joueur
        GameManager.Instance.temporalCrystals += 50; 
        GameManager.Instance.AddMana(GameManager.Instance.manaPerSecond * 3600 * 24); // +24h de mana
        
        PlayerPrefs.SetInt("StarterPackBought", 1);
        PlayerPrefs.Save();
        
        starterPackButton.SetActive(false);

        // JUS VISUEL
        if (AudioManager.Instance != null && AudioManager.Instance.cashSound != null)
        {
            AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.cashSound);
        }
    }
}