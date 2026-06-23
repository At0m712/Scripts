using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Localization;
using System.Globalization;
using System.Collections; 

public class PrestigeUI : MonoBehaviour
{
    [Header("UI - Textes")]
    public TextMeshProUGUI texteExplication;
    public TextMeshProUGUI texteMultiplicateurActuel;
    public TextMeshProUGUI texteMultiplicateurFutur;
    public TextMeshProUGUI cristauxGagnesText;

    [Header("UI - Boutons & Effets")]
    public Button prestigeButton;
    public Button closeButton;
    [Tooltip("Une image blanche qui prend tout l'écran, invisible au départ.")]
    public Image ecranFonduBlanc; 

    [Header("Localisation")]
    public LocalizedString locExplication; 
    public LocalizedString locGainCristaux; 
    public LocalizedString locTropTot; 

    private int cristauxGagnes;
    private double multiplicateurGagne;
    private bool isAnimating = false; 

    void Start()
    {
        if (ecranFonduBlanc != null)
        {
            ecranFonduBlanc.gameObject.SetActive(false);
            ecranFonduBlanc.color = new Color(1f, 1f, 1f, 0f); 
        }
    }

    void Update()
    {
        if (GameManager.Instance == null || isAnimating) return;

        double totalMana = GameManager.Instance.manaTotalProduced;

        if (totalMana > 1000000)
        {
            cristauxGagnes = (int)(System.Math.Pow(totalMana / 1000000.0, 0.33));
            multiplicateurGagne = cristauxGagnes * 0.1; 
        }
        else
        {
            cristauxGagnes = 0;
            multiplicateurGagne = 0;
        }

        double multiActuel = GameManager.Instance.prestigeMultiplier;
        if (texteMultiplicateurActuel != null) 
            texteMultiplicateurActuel.text = "Bonus Actuel : x" + multiActuel.ToString("F2");

        // 🛡️ CORRECTION : On débloque le prestige dès 0.5 (Environ 130 Millions de Mana généré)
        if (multiplicateurGagne >= 0.1) 
        {
            prestigeButton.interactable = true;

            double multiFutur = multiActuel + multiplicateurGagne;
            if (texteMultiplicateurFutur != null) 
                texteMultiplicateurFutur.text = "Renaissance : <color=#FFD700>x" + multiFutur.ToString("F2") + "</color>";

            locGainCristaux.Arguments = new object[] { ScoreUI.FormatNumber(cristauxGagnes) };
            if (cristauxGagnesText != null) cristauxGagnesText.text = locGainCristaux.GetLocalizedString();
            
            if (texteExplication != null) texteExplication.text = locExplication.GetLocalizedString();
        }
        else
        {
            prestigeButton.interactable = false;
            
            if (cristauxGagnesText != null) cristauxGagnesText.text = locTropTot.GetLocalizedString();
            if (texteMultiplicateurFutur != null) texteMultiplicateurFutur.text = "Renaissance : ???";
            if (texteExplication != null) texteExplication.text = locExplication.GetLocalizedString();
        }
    }

    public void ValiderPrestige()
    {
        // 🛡️ CORRECTION : Autorisé dès 0.5
        if (multiplicateurGagne < 0.5 || isAnimating) return; 
        
        StartCoroutine(SequenceAnimationPrestige());
    }

    private IEnumerator SequenceAnimationPrestige()
    {
        isAnimating = true;

        prestigeButton.interactable = false;
        if (closeButton != null) closeButton.interactable = false;

        if (AudioManager.Instance != null && AudioManager.Instance.buySound != null)
        {
            AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.buySound);
        }

        if (ecranFonduBlanc != null)
        {
            ecranFonduBlanc.gameObject.SetActive(true);
            float timer = 0f;
            float duration = 1.5f; 

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, timer / duration);
                ecranFonduBlanc.color = new Color(1f, 1f, 1f, alpha);
                yield return null;
            }
            
            yield return new WaitForSeconds(0.5f); 
        }

        GameManager.Instance.prestigeMultiplier += multiplicateurGagne;
        GameManager.Instance.temporalCrystals += cristauxGagnes;
        PlayerPrefs.SetString("prestigeMultiplier", GameManager.Instance.prestigeMultiplier.ToString(CultureInfo.InvariantCulture));

        GameManager.Instance.manaCurrent = 0;
        GameManager.Instance.manaTotalProduced = 0;
        GameManager.Instance.manaPerSecond = 0;

        UpgradeShopUI[] tousLesEtages = FindObjectsOfType<UpgradeShopUI>();
        foreach (var etage in tousLesEtages)
        {
            string nomEtage = etage.myFloorData.name;
            int minLevels = PlayerPrefs.GetInt("BonusLevels_" + nomEtage, 0);
            etage.currentLevel = minLevels;
            PlayerPrefs.SetInt("FloorLevel_" + nomEtage, minLevels);
            PlayerPrefs.SetFloat("FloorTimer_" + nomEtage, 0f);
        }

        PlayerPrefs.SetString("manaCurrent", "0");
        PlayerPrefs.SetString("manaTotalProduced", "0");
        PlayerPrefs.SetInt("temporalCrystals", GameManager.Instance.temporalCrystals);

        if (SaveManager.Instance != null) SaveManager.Instance.DemanderSauvegarde();
        else PlayerPrefs.Save();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}