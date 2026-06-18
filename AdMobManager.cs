using UnityEngine;
using System;

public class AdMobManager : MonoBehaviour
{
    public static AdMobManager Instance;
    
    // Le Callback qui s'exécute si le joueur a regardé la pub jusqu'au bout
    private Action onRewardEarnedCallback; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Fonction à appeler depuis vos boutons (Ex: le x3 Hors-ligne, l'Airdrop, le Boost 4h)
    public void ShowRewardedAd(Action rewardCallback)
    {
        onRewardEarnedCallback = rewardCallback;
        
        Debug.Log("[AdMob] Lancement de la vidéo publicitaire...");
        
        // --- VRAI CODE SDK À PLACER ICI PLUS TARD ---
        // if (rewardedAd.CanShowAd()) { rewardedAd.Show(...); }
        // --------------------------------------------

        // Simulation pour tester dans l'éditeur sans SDK :
        SimulateAdCompletion();
    }

    private void SimulateAdCompletion()
    {
        Debug.Log("[AdMob] Vidéo terminée. Distribution de la récompense.");
        if (onRewardEarnedCallback != null)
        {
            onRewardEarnedCallback.Invoke();
            onRewardEarnedCallback = null;
        }
    }
}