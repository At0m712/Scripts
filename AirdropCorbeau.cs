using UnityEngine;

public class AirdropCorbeau : MonoBehaviour
{
    [Header("UI")]
    public GameObject corbeauBouton; // L'objet visuel cliquable du corbeau

    [Header("Paramètres d'Apparition")]
    public float tempsApparitionMin = 60f;
    public float tempsApparitionMax = 180f;
    public float tempsDisparition = 15f; // Temps avant qu'il ne s'envole si on ne clique pas

    private void Start()
    {
        if (corbeauBouton != null) corbeauBouton.SetActive(false);
        ProgrammerProchaineApparition();
    }

    private void ProgrammerProchaineApparition()
    {
        float delai = Random.Range(tempsApparitionMin, tempsApparitionMax);
        Invoke(nameof(FaireApparaitreCorbeau), delai);
    }

    private void FaireApparaitreCorbeau()
    {
        if (corbeauBouton != null)
        {
            corbeauBouton.SetActive(true);
            // Le corbeau s'en va s'il n'est pas cliqué à temps
            Invoke(nameof(FaireDisparaitreCorbeau), tempsDisparition);
        }
    }

    private void FaireDisparaitreCorbeau()
    {
        if (corbeauBouton != null && corbeauBouton.activeSelf)
        {
            corbeauBouton.SetActive(false);
            ProgrammerProchaineApparition();
        }
    }

    // ==========================================
    // 🎁 RÉCOMPENSE AU CLIC
    // ==========================================
    public void OnClickCorbeau()
    {
        // 1. On annule la disparition automatique
        CancelInvoke(nameof(FaireDisparaitreCorbeau));

        // 2. Calcul de la récompense (15 minutes de production ou 500 minimum)
        if (GameManager.Instance != null)
        {
            double dpsActuel = GameManager.Instance.manaPerSecond;
            
            // 🛡️ SÉCURITÉ : On donne au moins 500 pour ne pas arnaquer un nouveau joueur
            double reward = (dpsActuel > 0) ? dpsActuel * 900 : 500; 

            GameManager.Instance.AddMana(reward);
            
            // On prévient le gestionnaire de sauvegarde qu'on a gagné de l'argent
            if (SaveManager.Instance != null) SaveManager.Instance.DemanderSauvegarde();
        }

        // 3. On cache le corbeau et on relance le cycle
        if (corbeauBouton != null) corbeauBouton.SetActive(false);
        ProgrammerProchaineApparition();
    }
}