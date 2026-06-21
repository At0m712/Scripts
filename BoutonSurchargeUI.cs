using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class BoutonSurchargeUI : MonoBehaviour
{
    [Header("Paramètres de la Compétence")]
    [Tooltip("Durée de la surcharge en secondes (ex: 30)")]
    public float dureeSurcharge = 30f; 
    [Tooltip("Temps d'attente avant réutilisation en secondes (ex: 300 pour 5 min)")]
    public float tempsRecharge = 300f; 

    [Header("Éléments UI")]
    public Button boutonSurcharge;
    public TextMeshProUGUI texteStatut; 
    
    [Tooltip("Optionnel : Une Image par-dessus le bouton (Type: Filled) pour assombrir pendant la recharge")]
    public Image imageAssombrie; 

    private float timerCooldown = 0f;

    void Start()
    {
        if (boutonSurcharge != null)
        {
            boutonSurcharge.onClick.RemoveAllListeners();
            boutonSurcharge.onClick.AddListener(ActiverSurcharge);
        }

        // On vérifie s'il y avait un temps de recharge en cours lors de la dernière session
        string dateFinString = PlayerPrefs.GetString("dateFinCooldownSurcharge", "");
        if (!string.IsNullOrEmpty(dateFinString) && DateTime.TryParse(dateFinString, out DateTime finCooldown))
        {
            if (finCooldown > DateTime.Now)
            {
                timerCooldown = (float)(finCooldown - DateTime.Now).TotalSeconds;
            }
        }
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        // ÉTAT 1 : La compétence est EN COURS d'utilisation !
        if (GameManager.Instance.IsRushActive)
        {
            boutonSurcharge.interactable = false;
            
            if (texteStatut != null)
                texteStatut.text = "ACTIF !";
                
            if (imageAssombrie != null)
                imageAssombrie.fillAmount = 0f;
        }
        // ÉTAT 2 : La compétence est en RECHARGE (Cooldown)
        else if (timerCooldown > 0)
        {
            timerCooldown -= Time.deltaTime;
            boutonSurcharge.interactable = false;

            if (texteStatut != null)
            {
                // Affiche les minutes et secondes restantes (ex: 04:59)
                TimeSpan temps = TimeSpan.FromSeconds(timerCooldown);
                texteStatut.text = string.Format("{0:D2}:{1:D2}", temps.Minutes, temps.Seconds);
            }

            // Remplit l'image noire au fur et à mesure
            if (imageAssombrie != null)
            {
                imageAssombrie.fillAmount = timerCooldown / tempsRecharge;
            }
        }
        // ÉTAT 3 : La compétence est PRÊTE
        else
        {
            timerCooldown = 0f;
            boutonSurcharge.interactable = true;

            if (texteStatut != null)
                texteStatut.text = "RUSH !";

            if (imageAssombrie != null)
                imageAssombrie.fillAmount = 0f;
        }
    }

    public void ActiverSurcharge()
    {
        if (GameManager.Instance == null) return;
        
        // Sécurité anti-spam
        if (timerCooldown > 0 || GameManager.Instance.IsRushActive) return;

        // 1. On active le x10 dans le moteur du jeu
        GameManager.Instance.ActivateRush(dureeSurcharge);

        // 2. On lance la punition d'attente
        timerCooldown = tempsRecharge;

        // 3. On sauvegarde la date de fin exacte pour éviter la triche en redémarrant le jeu
        DateTime finCooldownDateTime = DateTime.Now.AddSeconds(dureeSurcharge + tempsRecharge);
        PlayerPrefs.SetString("dateFinCooldownSurcharge", finCooldownDateTime.ToString());
        PlayerPrefs.Save();

        // 4. On joue le son
        if (AudioManager.Instance != null && AudioManager.Instance.buySound != null)
        {
            AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.buySound);
        }
    }
}   