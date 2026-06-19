using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class MachineASousGacha : MonoBehaviour
{
    [Header("Pop-up de Récompense")]
    public GameObject rewardPopupPanel;
    public TextMeshProUGUI rewardPopupText;
    
    [Header("Boutons")]
    public Button freeSpinButton;
    public Button adSpinButton;
    public TextMeshProUGUI freeSpinStatusText;

    private string lastSpinDateKey = "Gacha_LastFreeSpin";

    void Start()
    {
        freeSpinButton.onClick.AddListener(() => Spin(false));
        adSpinButton.onClick.AddListener(() => Spin(true));
        CheckFreeSpinAvailability();
    }

    private void CheckFreeSpinAvailability()
    {
        string lastSpin = PlayerPrefs.GetString(lastSpinDateKey, "");
        string today = DateTime.Now.ToString("yyyyMMdd");

        if (lastSpin == today)
        {
            freeSpinButton.interactable = false;
            freeSpinStatusText.text = "Revenez demain !";
        }
        else
        {
            freeSpinButton.interactable = true;
            freeSpinStatusText.text = "Tirage gratuit prêt !";
        }
    }

    public void Spin(bool usesAd)
    {
        if (!usesAd)
        {
            PlayerPrefs.SetString(lastSpinDateKey, DateTime.Now.ToString("yyyyMMdd"));
            PlayerPrefs.Save();
            CheckFreeSpinAvailability();
            ExecuteSpin();
        }
        else
        {
            // TODO: Appeler ton AdMobManager ici. Si la pub est vue en entier :
            ExecuteSpin(); 
        }
    }

    private void ExecuteSpin()
    {
        int randomChance = UnityEngine.Random.Range(0, 100);
        
        if (randomChance < 50) 
        {
            // 50% de chance : 1H de production
            double reward = GameManager.Instance.manaPerSecond * 3600;
            GameManager.Instance.AddMana(reward);
            rewardPopupText.text = "GAIN : 1 HEURE DE PRODUCTION\n(+ " + ScoreUI.FormatNumber(reward) + " Mana)";
        }
        else if (randomChance < 80) 
        {
            // 30% de chance : Boost Vitesse (2 Heures)
            GameManager.Instance.adBoostTimer += 7200f; 
            rewardPopupText.text = "JACKPOT !\nProduction x2 pendant 2 heures !";
        }
        else 
        {
            // 20% de chance : Cristaux premium
            GameManager.Instance.temporalCrystals += 5;
            rewardPopupText.text = "EXCEPTIONNEL !\n+ 5 Cristaux Temporels.";
        }

        rewardPopupPanel.SetActive(true);
    }
    
    public void CloseRewardPopup()
    {
        rewardPopupPanel.SetActive(false);
    }
}