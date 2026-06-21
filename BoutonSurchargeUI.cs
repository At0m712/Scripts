using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;

public class BoutonSurchargeUI : MonoBehaviour
{
    [Header("Configuration")]
    public float dureeSurcharge = 30f;
    public float tempsRecharge = 300f;
    
    [Header("UI Elements")]
    public Button boutonSurcharge;
    public Image iconeSurcharge;
    public Image jaugeRechargement;
    public TextMeshProUGUI texteStatut;
    public Image imageAssombrie;

    [Header("Localisation")]
    public LocalizedString textePret;  
    public LocalizedString texteActif; 

    // Variables internes pour gérer le temps localement
    private float timerSurcharge = 0f;
    private float timerRecharge = 0f;
    private bool isCooldown = false;

    void Start()
    {
        if (boutonSurcharge != null)
        {
            boutonSurcharge.onClick.AddListener(ActiverSurcharge);
        }
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        // --- 1. SI LA SURCHARGE EST EN COURS ---
        if (GameManager.Instance.IsRushActive)
        {
            timerSurcharge -= Time.deltaTime;
            
            boutonSurcharge.interactable = false;
            
            if(jaugeRechargement != null) 
                jaugeRechargement.fillAmount = timerSurcharge / dureeSurcharge;
                
            if(iconeSurcharge != null) 
                iconeSurcharge.color = Color.white;
            
            if(texteStatut != null)
            {
                texteStatut.text = texteActif.GetLocalizedString(); 
                texteStatut.color = Color.cyan;
            }

            // Fin de l'effet
            if (timerSurcharge <= 0f)
            {
                GameManager.Instance.IsRushActive = false;
                isCooldown = true;
                timerRecharge = tempsRecharge;
            }
        }
        // --- 2. SI LA COMPÉTENCE SE RECHARGE ---
        else if (isCooldown)
        {
            timerRecharge -= Time.deltaTime;
            
            boutonSurcharge.interactable = false;
            
            if(jaugeRechargement != null) 
                jaugeRechargement.fillAmount = 1f - (timerRecharge / tempsRecharge);
                
            if(iconeSurcharge != null) 
                iconeSurcharge.color = new Color(0.5f, 0.5f, 0.5f, 1f); // Grisé
                
            if(texteStatut != null) 
                texteStatut.text = ""; 

            // Fin du rechargement
            if (timerRecharge <= 0f)
            {
                isCooldown = false;
            }
        }
        // --- 3. SI LA COMPÉTENCE EST PRÊTE ---
        else
        {
            boutonSurcharge.interactable = true;
            
            if(jaugeRechargement != null) 
                jaugeRechargement.fillAmount = 1f;
                
            if(iconeSurcharge != null) 
                iconeSurcharge.color = Color.white;
            
            if(texteStatut != null)
            {
                texteStatut.text = textePret.GetLocalizedString(); 
                texteStatut.color = Color.yellow;
            }
        }
    }

    void ActiverSurcharge()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsRushActive && !isCooldown)
        {
            GameManager.Instance.IsRushActive = true;
            timerSurcharge = dureeSurcharge;
            
            if (AudioManager.Instance != null && AudioManager.Instance.buySound != null)
            {
                AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.buySound);
            }
        }
    }
}  
