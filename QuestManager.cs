using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("Quête 1 : Améliorations")]
    public int q1Goal = 5; // Acheter 5 niveaux
    public int q1Progress = 0;
    public bool q1Claimed = false;
    public TextMeshProUGUI q1Text;
    public Button q1Btn;

    [Header("Quête 2 : Utiliser la Surcharge")]
    public int q2Goal = 3; // Utiliser le sort de Rush 3 fois
    public int q2Progress = 0;
    public bool q2Claimed = false;
    public TextMeshProUGUI q2Text;
    public Button q2Btn;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        CheckNewDay();
        UpdateUI();
        
        q1Btn.onClick.AddListener(ClaimQuest1);
        q2Btn.onClick.AddListener(ClaimQuest2);
    }

    private void CheckNewDay()
    {
        string today = DateTime.Now.ToString("yyyyMMdd");
        if (PlayerPrefs.GetString("Quest_LastDay", "") != today)
        {
            PlayerPrefs.SetString("Quest_LastDay", today);
            ResetQuests();
        }
        else
        {
            LoadQuests();
        }
    }

    private void ResetQuests()
    {
        q1Progress = 0; q1Claimed = false;
        q2Progress = 0; q2Claimed = false;
        SaveQuests();
    }

    // --- FONCTIONS POUR FAIRE PROGRESSER LES QUÊTES ---
    
    // À appeler dans UpgradeShopUI.cs dans la fonction BuyUpgrade() : QuestManager.Instance.AddUpgradeProgress();
    public void AddUpgradeProgress()
    {
        if (!q1Claimed && q1Progress < q1Goal)
        {
            q1Progress++;
            SaveQuests();
            UpdateUI();
        }
    }

    // À appeler dans GameManager.cs dans la fonction ActivateRush() : QuestManager.Instance.AddRushProgress();
    public void AddRushProgress()
    {
        if (!q2Claimed && q2Progress < q2Goal)
        {
            q2Progress++;
            SaveQuests();
            UpdateUI();
        }
    }

    // --- RÉCOMPENSES ---

    public void ClaimQuest1()
    {
        GameManager.Instance.temporalCrystals += 2; // Récompense premium
        q1Claimed = true;
        SaveQuests();
        UpdateUI();
        // TODO: Jouer son de validation
    }

    public void ClaimQuest2()
    {
        GameManager.Instance.adBoostTimer += 3600f; // +1h de boost
        q2Claimed = true;
        SaveQuests();
        UpdateUI();
    }

    // --- SAUVEGARDE ET UI ---

    private void SaveQuests()
    {
        PlayerPrefs.SetInt("Quest_Q1Prog", q1Progress);
        PlayerPrefs.SetInt("Quest_Q1Claim", q1Claimed ? 1 : 0);
        PlayerPrefs.SetInt("Quest_Q2Prog", q2Progress);
        PlayerPrefs.SetInt("Quest_Q2Claim", q2Claimed ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadQuests()
    {
        q1Progress = PlayerPrefs.GetInt("Quest_Q1Prog", 0);
        q1Claimed = PlayerPrefs.GetInt("Quest_Q1Claim", 0) == 1;
        q2Progress = PlayerPrefs.GetInt("Quest_Q2Prog", 0);
        q2Claimed = PlayerPrefs.GetInt("Quest_Q2Claim", 0) == 1;
    }

    public void UpdateUI()
    {
        q1Text.text = $"Améliorer la tour ({q1Progress}/{q1Goal})";
        q1Btn.interactable = (q1Progress >= q1Goal && !q1Claimed);
        q1Btn.GetComponentInChildren<TextMeshProUGUI>().text = q1Claimed ? "RÉCUPÉRÉ" : "RÉCUPÉRER";

        q2Text.text = $"Utiliser Surcharge ({q2Progress}/{q2Goal})";
        q2Btn.interactable = (q2Progress >= q2Goal && !q2Claimed);
        q2Btn.GetComponentInChildren<TextMeshProUGUI>().text = q2Claimed ? "RÉCUPÉRÉ" : "RÉCUPÉRER";
    }
}