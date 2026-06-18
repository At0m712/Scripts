using UnityEngine;

public class ConsentManager : MonoBehaviour
{
    private void Start()
    {
        // Initialisation de base pour la collecte de consentement publicitaire
        RequestConsent();
    }

    private void RequestConsent()
    {
        Debug.Log("[ConsentManager] Vérification du consentement utilisateur pour les publicités personnalisées...");
        
        // --- À INTÉGRER PLUS TARD ---
        // C'est ici que vous appellerez le SDK Google UMP (User Messaging Platform) 
        // ou Unity Ads Consent API pour être en règle avec les lois sur la vie privée.
    }
}