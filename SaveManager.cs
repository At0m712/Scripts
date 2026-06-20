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
        InvokeRepeating("SaveGame", 30f, 30f); 
    }

    public void SaveGame()
    {
        if (GameManager.Instance == null) return;
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
            double.TryParse(PlayerPrefs.GetString("manaCurrent", "0"), out GameManager.Instance.manaCurrent);
            double.TryParse(PlayerPrefs.GetString("manaTotalProduced", "0"), out GameManager.Instance.manaTotalProduced);
            GameManager.Instance.temporalCrystals = PlayerPrefs.GetInt("temporalCrystals", 0);
            GameManager.Instance.RecalculateMultiplier();

            CalculateOfflineGains();
        }
    }

    private void CalculateOfflineGains()
    {
        string savedTimeStr = PlayerPrefs.GetString("lastSaveTime", "");
        
        // SÉCURITÉ ANTI-CRASH (Si la sauvegarde est vide ou corrompue)
        if (string.IsNullOrEmpty(savedTimeStr) || !long.TryParse(savedTimeStr, out long temp))
            return;

        DateTime lastSaveTime = DateTime.FromBinary(temp);
        TimeSpan timePassed = DateTime.Now - lastSaveTime;

        double secondsPassed = timePassed.TotalSeconds;
        
        if (secondsPassed > 86400) secondsPassed = 86400;

        if (secondsPassed > 60 && GameManager.Instance.manaPerSecond > 0) 
        {
            pendingOfflineGains = secondsPassed * (GameManager.Instance.manaPerSecond * GameManager.Instance.globalMultiplier);
            offlineGainsText.text = ScoreUI.FormatNumber(pendingOfflineGains) + " Mana !";
            if (offlinePopup != null) offlinePopup.GetComponent<PopupAnimator>().Ouvrir();
        }
    }

    public void CollectOfflineGains()
    {
        GameManager.Instance.AddMana(pendingOfflineGains);
        if (offlinePopup != null) offlinePopup.GetComponent<PopupAnimator>().Fermer();
        
        if (AudioManager.Instance != null && AudioManager.Instance.buySound != null)
            AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.buySound);
    }

    public void CollectOfflineGainsAdBoost()
    {
        if (AdMobManager.Instance != null)
        {
            AdMobManager.Instance.ShowRewardedAd(() => 
            {
                GameManager.Instance.AddMana(pendingOfflineGains * 2);
                if (offlinePopup != null) offlinePopup.GetComponent<PopupAnimator>().Fermer();
            });
        }
        else
        {
            GameManager.Instance.AddMana(pendingOfflineGains * 2); 
            if (offlinePopup != null) offlinePopup.GetComponent<PopupAnimator>().Fermer();
        }
    }

    void OnApplicationQuit() { SaveGame(); }
    void OnApplicationPause(bool pause) { if(pause) SaveGame(); }
}