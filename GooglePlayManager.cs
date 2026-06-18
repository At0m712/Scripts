using UnityEngine;

public class GooglePlayManager : MonoBehaviour
{
    private void Start()
    {
        AuthenticateUser();
    }

    private void AuthenticateUser()
    {
        Debug.Log("[GooglePlayManager] Tentative de connexion à Google Play Games Services...");
        
        // --- À INTÉGRER PLUS TARD ---
        // PlayGamesPlatform.Activate();
        // Social.localUser.Authenticate((bool success) => { ... });
        // Utilisé pour les Leaderboards ou la sauvegarde Cloud.
    }
}