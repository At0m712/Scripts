using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Localization;
using System.Globalization;
using System.Collections; // Indispensable pour les animations de fondu

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
    public LocalizedString locExplication; // Clé ex: Le voyage temporel détruira la tour...
    public LocalizedString locGainCristaux; // Clé ex: + {0} Cristaux Temporels
    public LocalizedString locTropTot; // Clé ex: Il faut accumuler plus de Mana !

    private int cristauxGagnes;
    private double multiplicateurGagne;
    private bool isAnimating = false; // Empêche le joueur de spammer le bouton

    void Start()
    {
        // On s'assure que l'écran de fondu est bien invisible au lancement
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

        // Calcul mathématique sécurisé pour les nombres immenses
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

        // --- AFFICHAGE "AVANT / APRÈS" ---
        double multiActuel = GameManager.Instance.prestigeMultiplier;
        if (texteMultiplicateurActuel != null) 
            texteMultiplicateurActuel.text = "Bonus Actuel : x" + multiActuel.ToString("F2");

        if (multiplicateurGagne >= 4.0)
        {
            prestigeButton.interactable = true;

            // On additionne l'actuel et le gagné pour montrer la vraie puissance future
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

    // ==========================================
    // 🌌 LE RITUEL D'ASCENSION (ANIMATION)
    // ==========================================
    public void ValiderPrestige()
    {
        if (multiplicateurGagne < 4.0 || isAnimating) return;
        
        // On lance la séquence cinématique
        StartCoroutine(SequenceAnimationPrestige());
    }

    private IEnumerator SequenceAnimationPrestige()
    {
        isAnimating = true;

        // 1. Bloquer l'interface pour empêcher les bugs
        prestigeButton.interactable = false;
        if (closeButton != null) closeButton.interactable = false;

        // 2. Jouer un son très lourd/épique si tu as
        if (AudioManager.Instance != null && AudioManager.Instance.buySound != null)
        {
            AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.buySound);
        }

        // 3. Effet de voyage temporel (L'écran devient blanc de plus en plus fort)
        if (ecranFonduBlanc != null)
        {
            ecranFonduBlanc.gameObject.SetActive(true);
            float timer = 0f;
            float duration = 1.5f; // Le fondu dure 1.5 secondes

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, timer / duration);
                ecranFonduBlanc.color = new Color(1f, 1f, 1f, alpha);
                yield return null;
            }
            
            // Petite pause une fois l'écran 100% blanc pour l'effet dramatique
            yield return new WaitForSeconds(0.5f); 
        }

        // 4. LES SAUVEGARDES (Pendant que l'écran est blanc, on efface tout)
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

        // 5. Renaissance (On recharge la scène)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}