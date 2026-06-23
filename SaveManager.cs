using UnityEngine;
using System;

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
        // OPTIMISATION MAX : Sauvegarde groupée toutes les 10 secondes au lieu de chaque frame
        InvokeRepeating(nameof(ForcerSauvegarde), 10f, 10f);
    }

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

    // ESSENTIEL SUR MOBILE : Sauvegarde si le joueur met l'application en arrière-plan
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) 
        {
            PlayerPrefs.Save();
            SauvegarderTempsDeconnexion();
            aDesModificationsNonSauvegardees = false;
        }
    }

    // Sauvegarde à la fermeture ferme du jeu
    void OnApplicationQuit()
    {
        PlayerPrefs.Save();
        SauvegarderTempsDeconnexion();
        aDesModificationsNonSauvegardees = false;
    }

    private void SauvegarderTempsDeconnexion()
    {
        // On sauvegarde l'heure exacte à laquelle le jeu est mis en pause/fermé
        PlayerPrefs.SetString("LastLogoutTime", DateTime.Now.ToString());
        PlayerPrefs.Save();
    }
}