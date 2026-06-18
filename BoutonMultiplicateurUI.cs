using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BoutonMultiplicateurUI : MonoBehaviour
{
    public Button boostButton;
    public TextMeshProUGUI timerText; // Optionnel : pour afficher le temps restant
    
    private float boostDuration = 14400f; // 4 heures en secondes

    private void Start()
    {
        boostButton.onClick.AddListener(OnBoostButtonClicked);
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;

        // Met à jour l'affichage du temps s'il y a un boost actif
        if (GameManager.Instance.adBoostTimer > 0)
        {
            if (timerText != null)
            {
                int hours = Mathf.FloorToInt(GameManager.Instance.adBoostTimer / 3600);
                int minutes = Mathf.FloorToInt((GameManager.Instance.adBoostTimer % 3600) / 60);
                timerText.text = string.Format("{0:00}:{1:00}", hours, minutes);
            }
        }
        else
        {
            if (timerText != null) timerText.text = "BOOST x2";
        }
    }

    private void OnBoostButtonClicked()
    {
        // Ici, vous appellerez AdMobManager.Instance.ShowRewardedAd(ActivateBoost);
        // Pour le moment, on active le boost directement :
        ActivateBoost();
    }

    public void ActivateBoost()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.adBoostMultiplier = 2f;
            // On peut cumuler le temps si le joueur regarde plusieurs pubs
            GameManager.Instance.adBoostTimer += boostDuration; 
            
            if (AudioManager.Instance != null) AudioManager.Instance.PlayCash();
        }
    }
}