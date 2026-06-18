using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    private void Start()
    {
        InitializeFirebase();
    }

    private void InitializeFirebase()
    {
        Debug.Log("[FirebaseManager] Initialisation des services Firebase (Analytics, Crashlytics)...");
        
        // --- À INTÉGRER PLUS TARD ---
        // Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => { ... });
        // Indispensable pour suivre la rétention de vos joueurs et équilibrer l'économie.
    }
}