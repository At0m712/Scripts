using UnityEngine;
using System;

[Serializable]
public class SaveData
{
    public long lastSaveTimestamp;
    public double manaCurrent;
    public double manaTotalProduced;
    public double temporalCrystals;
}

public class SaveManager : MonoBehaviour
{
    private string saveKey = "TourSorcierSave";
    public GameObject offlinePopup; // Fenêtre UI de retour
    public TMPro.TextMeshProUGUI offlineGainsText;

    private double offlineGains = 0;

    private void Start()
    {
        LoadGame();
        CalculateOfflineGains();
        InvokeRepeating(nameof(SaveGame), 30f, 30f); // Sauvegarde auto toutes les 30 sec
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    public void SaveGame()
    {
        SaveData data = new SaveData
        {
            lastSaveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            manaCurrent = GameManager.Instance.manaCurrent,
            manaTotalProduced = GameManager.Instance.manaTotalProduced,
            temporalCrystals = GameManager.Instance.temporalCrystals
        };

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(saveKey, json);
        PlayerPrefs.Save();
    }

    private void LoadGame()
    {
        if (PlayerPrefs.HasKey(saveKey))
        {
            string json = PlayerPrefs.GetString(saveKey);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            GameManager.Instance.manaCurrent = data.manaCurrent;
            GameManager.Instance.manaTotalProduced = data.manaTotalProduced;
            GameManager.Instance.temporalCrystals = data.temporalCrystals;
        }
    }

    private void CalculateOfflineGains()
    {
        if (!PlayerPrefs.HasKey(saveKey)) return;

        string json = PlayerPrefs.GetString(saveKey);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long timeAwaySeconds = currentTime - data.lastSaveTimestamp;

        // Limite de stockage : 2 heures maximum (7200 secondes)
        if (timeAwaySeconds > 7200) timeAwaySeconds = 7200;

        if (timeAwaySeconds > 60 && GameManager.Instance.manaPerSecond > 0) // Si parti plus de 1 minute
        {
            offlineGains = timeAwaySeconds * GameManager.Instance.manaPerSecond;
            
            ScoreUI scoreUI = FindObjectOfType<ScoreUI>();
            offlineGainsText.text = "Gains hors-ligne : \n" + scoreUI.FormatNumber(offlineGains) + " Mana";
            offlinePopup.SetActive(true);
        }
    }

    // À lier au bouton "Récupérer" basique
    public void CollectOfflineGains()
    {
        GameManager.Instance.AddMana(offlineGains);
        offlinePopup.SetActive(false);
    }

    // À lier au bouton "Vidéo Pub x3"
    public void CollectOfflineGainsAdBoost()
    {
        GameManager.Instance.AddMana(offlineGains * 3);
        offlinePopup.SetActive(false);
        // Appeler ici AdMobManager pour lancer la vidéo
    }
}