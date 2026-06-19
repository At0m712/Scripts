using UnityEngine;
using TMPro;
using System;

public class SaveManager : MonoBehaviour
{
    public GameObject offlinePopup;
    public TextMeshProUGUI offlineGainsText;
    private double pendingOfflineGains = 0;

    void Start()
    {
        LoadGame();
        InvokeRepeating("SaveGame", 30f, 30f); // Sauvegarde automatique toutes les 30s
    }

    public void SaveGame()
    {
        PlayerPrefs.SetString("manaCurrent", GameManager.Instance.manaCurrent.ToString());
        PlayerPrefs.SetString("manaTotalProduced", GameManager.Instance.manaTotalProduced.ToString());
        PlayerPrefs.SetInt("temporalCrystals", GameManager.Instance.temporalCrystals);
        PlayerPrefs.SetString("lastSaveTime", DateTime.Now.ToBinary().ToString());
        PlayerPrefs.Save();
    }

    private void LoadGame()
    {
        if (PlayerPrefs.HasKey("manaCurrent"))
        {
            GameManager.Instance.manaCurrent = double.Parse(PlayerPrefs.GetString("manaCurrent"));
            GameManager.Instance.manaTotalProduced = double.Parse(PlayerPrefs.GetString("manaTotalProduced"));
            GameManager.Instance.temporalCrystals = PlayerPrefs.GetInt("temporalCrystals");
            GameManager.Instance.RecalculateMultiplier();

            CalculateOfflineGains();
        }
    }

    private void CalculateOfflineGains()
    {
        long temp = Convert.ToInt64(PlayerPrefs.GetString("lastSaveTime"));
        DateTime lastSaveTime = DateTime.FromBinary(temp);
        TimeSpan timePassed = DateTime.Now - lastSaveTime;

        double secondsPassed = timePassed.TotalSeconds;
        
        // Limite à 24h max d'absence (86400 secondes)
        if (secondsPassed > 86400) secondsPassed = 86400;

        if (secondsPassed > 60 && GameManager.Instance.manaPerSecond > 0) // Si parti + d'1 minute
        {
            pendingOfflineGains = secondsPassed * (GameManager.Instance.manaPerSecond * GameManager.Instance.globalMultiplier);
            offlineGainsText.text = ScoreUI.FormatNumber(pendingOfflineGains) + " Mana !";
            offlinePopup.GetComponent<PopupAnimator>().Ouvrir();
        }
    }

    public void CollectOfflineGains()
    {
        GameManager.Instance.AddMana(pendingOfflineGains);
        offlinePopup.GetComponent<PopupAnimator>().Fermer();
    }

    public void CollectOfflineGainsAdBoost()
    {
        // TODO: Appeler l'API de pub ici. Si succès :
        GameManager.Instance.AddMana(pendingOfflineGains * 2); // Pub = x2
        offlinePopup.GetComponent<PopupAnimator>().Fermer();
    }

    void OnApplicationQuit() { SaveGame(); }
    void OnApplicationPause(bool pause) { if(pause) SaveGame(); }
}