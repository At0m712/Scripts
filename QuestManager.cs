using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("Configuration Quête")]
    public int targetClicks = 100;
    public int currentClicks = 0;

    [Header("UI Elements")]
    public TextMeshProUGUI questText;
    public TextMeshProUGUI progressText;
    public Image progressBar;
    public Button claimButton;
    public TextMeshProUGUI claimButtonText;

    [Header("Localisation")]
    public LocalizedString texteTitreQuete; 
    public LocalizedString texteBoutonRecuperer; 
    public LocalizedString texteBoutonEnCours; 

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        UpdateUI();
    }

    public void AddUpgradeProgress()
    {
        if (currentClicks < targetClicks)
        {
            currentClicks++;
            UpdateUI();
        }
    }

    public void ClaimReward()
    {
        if (currentClicks >= targetClicks)
        {
            // Reset et passe à l'objectif suivant
            currentClicks = 0;
            targetClicks += 50; 
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (questText != null)
        {
            texteTitreQuete.Arguments = new object[] { targetClicks };
            questText.text = texteTitreQuete.GetLocalizedString();
        }

        if (progressText != null)
        {
            progressText.text = currentClicks + " / " + targetClicks;
        }

        if (progressBar != null)
        {
            progressBar.fillAmount = (float)currentClicks / targetClicks;
        }

        if (claimButton != null && claimButtonText != null)
        {
            if (currentClicks >= targetClicks)
            {
                claimButton.interactable = true;
                claimButtonText.text = texteBoutonRecuperer.GetLocalizedString();
            }
            else
            {
                claimButton.interactable = false;
                claimButtonText.text = texteBoutonEnCours.GetLocalizedString();
            }
        }
    }
}