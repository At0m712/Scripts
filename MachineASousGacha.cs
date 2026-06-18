using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class MachineASousGacha : MonoBehaviour
{
    [Header("Fenêtre des Gains (Pop-up)")]
    public GameObject rewardPopupPanel;
    public TextMeshProUGUI rewardPopupText;
    
    [Header("Boutons de la Machine")]
    public Button freeSpinButton;
    public Button adSpinButton;
    public TextMeshProUGUI freeSpinStatusText;

    private string lastSpinDateKey = "LastFreeSpinDate_Gacha";

    private void Start()
    {
        freeSpinButton.onClick.AddListener(() => Spin(false));
        adSpinButton.onClick.AddListener(() => Spin(true));
        CheckFreeSpinAvailability();
    }

    private void CheckFreeSpinAvailability()
    {
        // On récupère la date sauvegardée sous format "AnnéeMoisJour" (ex: 20260614)
        string lastSpin = PlayerPrefs.GetString(lastSpinDateKey, "");
        string today = DateTime.Now.ToString("yyyyMMdd");

        // Si la date sauvegardée correspond à la date du jour, le tour gratuit a déjà été utilisé
        if (lastSpin == today)
        {
            freeSpinButton.interactable = false;
            freeSpinStatusText.text = "Prochain tirage gratuit demain";
        }
        else
        {
            freeSpinButton.interactable = true;
            freeSpinStatusText.text = "Tirage gratuit disponible !";
        }
    }

    public void Spin(bool usesAd)
    {
        if (!usesAd)
        {
            // Marquer le tirage gratuit comme utilisé aujourd'hui
            PlayerPrefs.SetString(lastSpinDateKey, DateTime.Now.ToString("yyyyMMdd"));
            PlayerPrefs.Save();
            
            freeSpinButton.interactable = false;
            freeSpinStatusText.text = "Prochain tirage gratuit demain";
            
            ExecuteSpin();
        }
        else
        {
            // Si le joueur clique sur le bouton "Tirage via Pub"
            Debug.Log("Lancement de la vidéo publicitaire (Call AdMobManager ici)...");
            
            // Une fois la vidéo terminée avec succès, on exécute le tirage :
            // AdMobManager.Instance.ShowRewardedAd(ExecuteSpin);
            ExecuteSpin(); 
        }
    }

    private void ExecuteSpin()
    {
        // Logique des probabilités du Gacha
        int randomChance = UnityEngine.Random.Range(0, 100); // Tire un chiffre entre 0 et 99
        
        if (randomChance < 50) 
        {
            // 50% de chance : Le joueur gagne l'équivalent d'une heure de production de Mana
            double manaReward = GameManager.Instance.manaPerSecond * 3600;
            GameManager.Instance.AddMana(manaReward);
            
            ScoreUI scoreUI = FindObjectOfType<ScoreUI>();
            rewardPopupText.text = "Félicitations !\nVous gagnez 1 Heure de production de Mana (" + scoreUI.FormatNumber(manaReward) + ").";
        }
        else if (randomChance < 85) 
        {
            // 35% de chance : Le joueur gagne une poignée de Cristaux Temporels
            double crystalReward = 50;
            GameManager.Instance.temporalCrystals += crystalReward;
            
            rewardPopupText.text = "Exceptionnel !\nVous gagnez " + crystalReward + " Cristaux Temporels.";
        }
        else 
        {
            // 15% de chance (Jackpot) : Le joueur gagne un Boost Vitesse de 2 heures
            GameManager.Instance.adBoostMultiplier = 2f;
            GameManager.Instance.adBoostTimer += 7200f; // Ajoute 7200 secondes
            
            rewardPopupText.text = "JACKPOT !\nVotre Tour produit 2x plus vite pendant les 2 prochaines heures !";
        }

        // On affiche le pop-up de récompense
        rewardPopupPanel.SetActive(true);
    }
    
    public void CloseRewardPopup()
    {
        rewardPopupPanel.SetActive(false);
    }
}