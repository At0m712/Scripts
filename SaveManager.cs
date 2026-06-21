using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private bool aDesModificationsNonSauvegardees = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // OPTIMISATION MAX : Sauvegarde groupée toutes les 10 secondes au lieu de chaque frame ou chaque achat
        InvokeRepeating(nameof(ForcerSauvegarde), 10f, 10f);
    }

    // À appeler par les autres scripts (ex: GameManager) au lieu de PlayerPrefs.Save()
    public void DemanderSauvegarde()
    {
        aDesModificationsNonSauvegardees = true;
    }

    private void ForcerSauvegarde()
    {
        if (aDesModificationsNonSauvegardees)
        {
            PlayerPrefs.Save();
            aDesModificationsNonSauvegardees = false;
        }
    }

    // ESSENTIEL SUR MOBILE : Sauvegarde si le joueur met l'application en arrière-plan (Home button, appel reçu...)
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) 
        {
            PlayerPrefs.Save();
            aDesModificationsNonSauvegardees = false;
        }
    }

    // Sauvegarde à la fermeture ferme du jeu
    void OnApplicationQuit()
    {
        PlayerPrefs.Save();
        aDesModificationsNonSauvegardees = false;
    }
}