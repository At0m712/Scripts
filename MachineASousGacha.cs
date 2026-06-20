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
            if (AdMobManager.Instance != null)
            {
                AdMobManager.Instance.ShowRewardedAd(() => 
                {
                    ExecuteSpin();
                });
            }
            else 
            {
                ExecuteSpin(); 
            }
        }
    }

    private void ExecuteSpin()
    {
        // Son de roulement
        if (AudioManager.Instance != null && AudioManager.Instance.clickSound != null)
        {
            AudioManager.Instance.sfxSource.pitch = 1.5f;
            AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.clickSound);
            AudioManager.Instance.sfxSource.pitch = 1.0f;
        }

        int randomChance = UnityEngine.Random.Range(0, 100);
        
        if (randomChance < 50) 
        {
            // 50% de chance : FIOLLE DE MANA (Inventaire)
            InventaireUI.AjouterPotionMana(1);
            rewardPopupText.text = "GAIN : FIOLLE DE MANA\n(Ajouté à votre inventaire !)";
        }
        else if (randomChance < 80) 
        {
            // 30% de chance : BOOST DE VITESSE (Inventaire)
            InventaireUI.AjouterBoostVitesse(1);
            rewardPopupText.text = "JACKPOT !\nBOOST DE VITESSE (2H)\n(Ajouté à votre inventaire !)";
        }
        else 
        {
            // 20% de chance : Cristaux premium (Reste immédiat car c'est une monnaie)
            GameManager.Instance.temporalCrystals += 5;
            rewardPopupText.text = "EXCEPTIONNEL !\n+ 5 Cristaux Temporels.";
        }

        // Son de Victoire
        if (AudioManager.Instance != null && AudioManager.Instance.buySound != null)
        {
            AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.buySound);
        }

        rewardPopupPanel.SetActive(true);
    }
    
    public void CloseRewardPopup()
    {
        rewardPopupPanel.SetActive(false);
    }
}