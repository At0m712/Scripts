using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections;

public class GachaManager : MonoBehaviour
{
    [Header("UI - Textes")]
    public TextMeshProUGUI texteNiveau;
    public TextMeshProUGUI texteTiragesRestants;
    public TextMeshProUGUI texteRecompenseObtenue; 
    public TextMeshProUGUI texteDropRates;

    [Header("UI - Bouton Unique")]
    public Button boutonActionPub; // LE bouton unique
    public TextMeshProUGUI texteBoutonAction; // Le texte à l'intérieur du bouton
    public Button boutonFermer;

    [Header("Animation - Éléments de la Scène")]
    public GameObject machineConteneur; 
    public GameObject zoneAnimationCapsule; 
    public Image iconeRecompense; 
    
    [Header("Sprites")]
    public Sprite imagePotion;
    public Sprite imageCristaux;
    public Sprite imageBoost;

    [Header("Paramètres")]
    private int niveauActuel = 1;
    private int tiragesRestants = 15;
    private const int MAX_TIRAGES = 15;
    private bool isAnimating = false; 

    void Start()
    {
        ChargerSauvegardes();
        VerifierResetQuotidien();
        MettreAJourUI();
        
        if(zoneAnimationCapsule != null) zoneAnimationCapsule.SetActive(false);
    }

    private void ChargerSauvegardes()
    {
        niveauActuel = PlayerPrefs.GetInt("GachaLevel", 1);
        tiragesRestants = PlayerPrefs.GetInt("GachaTirages", MAX_TIRAGES);
    }

    private void VerifierResetQuotidien()
    {
        string derniereDate = PlayerPrefs.GetString("GachaDate", "");
        string dateAujourdhui = DateTime.Now.ToString("yyyy-MM-dd");

        if (derniereDate != dateAujourdhui)
        {
            tiragesRestants = MAX_TIRAGES;
            PlayerPrefs.SetInt("GachaTirages", tiragesRestants);
            PlayerPrefs.SetString("GachaDate", dateAujourdhui);
            PlayerPrefs.Save();
        }
    }

    private void MettreAJourUI()
    {
        if (texteNiveau != null) texteNiveau.text = "Gacha - Niveau " + niveauActuel;
        if (texteTiragesRestants != null) texteTiragesRestants.text = "Tirages : " + tiragesRestants + " / " + MAX_TIRAGES;
        if (texteDropRates != null) texteDropRates.text = "Potions : 30% | Cristaux : 69.9% | Ultime : 0.1%";

        if (!isAnimating)
        {
            if (boutonFermer != null) boutonFermer.interactable = true;

            // LE BOUTON INTELLIGENT
            if (boutonActionPub != null)
            {
                if (niveauActuel < 5)
                {
                    boutonActionPub.interactable = true;
                    if (texteBoutonAction != null) texteBoutonAction.text = "Améliorer\n(Pub)";
                }
                else
                {
                    boutonActionPub.interactable = (tiragesRestants > 0);
                    if (texteBoutonAction != null) texteBoutonAction.text = "Invoquer\n(Pub)";
                }
            }
        }
    }

    // ==========================================
    // 📺 CLIC SUR LE BOUTON UNIQUE
    // ==========================================
    public void OnClickBoutonAction()
    {
        if (isAnimating) return;
        if (niveauActuel >= 5 && tiragesRestants <= 0) return;

        // C'EST ICI QUE TU LANCERAS TA PUB ADMOB PLUS TARD.
        // Exemple : AdMobManager.Instance.ShowRewardedAd(OnPubTerminee);
        
        // Pour l'instant, on simule que la pub a été vue avec succès :
        OnPubTerminee();
    }

    // Fonction appelée QUAND LA PUB EST FINIE
    public void OnPubTerminee()
    {
        if (niveauActuel < 5)
        {
            StartCoroutine(SequenceAmelioration());
        }
        else
        {
            tiragesRestants--;
            PlayerPrefs.SetInt("GachaTirages", tiragesRestants);
            PlayerPrefs.Save();
            StartCoroutine(SequenceTirageGacha());
        }
    }

    // ==========================================
    // 🎬 ANIMATION 1 : AMÉLIORATION MACHINE
    // ==========================================
    private IEnumerator SequenceAmelioration()
    {
        isAnimating = true;
        VerrouillerInterface();
        texteRecompenseObtenue.text = "Amélioration en cours...";
        zoneAnimationCapsule.SetActive(false);

        // Tremblement
        Vector3 positionInitiale = machineConteneur.transform.localPosition;
        float tempsTremblement = 0f;
        while (tempsTremblement < 1f)
        {
            machineConteneur.transform.localPosition = positionInitiale + (Vector3)UnityEngine.Random.insideUnitCircle * 10f;
            tempsTremblement += Time.deltaTime;
            yield return null;
        }
        machineConteneur.transform.localPosition = positionInitiale; 

        yield return new WaitForSeconds(0.2f);

        // Gain du niveau
        niveauActuel++;
        PlayerPrefs.SetInt("GachaLevel", niveauActuel);
        PlayerPrefs.Save();

        texteRecompenseObtenue.text = "Machine améliorée au Niveau " + niveauActuel + " !";
        
        TerminerAnimation();
    }

    // ==========================================
    // 🎬 ANIMATION 2 : LE VRAI GACHA
    // ==========================================
    private IEnumerator SequenceTirageGacha()
    {
        isAnimating = true;
        VerrouillerInterface();
        texteRecompenseObtenue.text = "Invocation en cours...";
        zoneAnimationCapsule.SetActive(false);

        // Tremblement
        Vector3 positionInitiale = machineConteneur.transform.localPosition;
        float tempsTremblement = 0f;
        while (tempsTremblement < 1f)
        {
            machineConteneur.transform.localPosition = positionInitiale + (Vector3)UnityEngine.Random.insideUnitCircle * 10f;
            tempsTremblement += Time.deltaTime;
            yield return null; 
        }
        machineConteneur.transform.localPosition = positionInitiale; 

        yield return new WaitForSeconds(0.2f);

        // Tirage des %
        EffectuerTirage();

        // Apparition de la capsule (Pop)
        zoneAnimationCapsule.SetActive(true);
        zoneAnimationCapsule.transform.localScale = Vector3.zero; 
        
        float tempsPop = 0f;
        while (tempsPop < 0.3f)
        {
            tempsPop += Time.deltaTime;
            float scale = Mathf.Lerp(0f, 1f, tempsPop / 0.3f);
            zoneAnimationCapsule.transform.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        TerminerAnimation();
    }

    private void VerrouillerInterface()
    {
        if (boutonActionPub != null) boutonActionPub.gameObject.SetActive(false);
        if (boutonFermer != null) boutonFermer.interactable = false;
    }

    private void TerminerAnimation()
    {
        isAnimating = false;
        if (boutonActionPub != null) boutonActionPub.gameObject.SetActive(true);
        MettreAJourUI();
    }

    // ==========================================
    // 🎲 MOTEUR DE PROBABILITÉS
    // ==========================================
    private void EffectuerTirage()
    {
        float jetDeDe = UnityEngine.Random.Range(0f, 100f);

        if (jetDeDe <= 0.1f) DonnerBoostPermanent();
        else if (jetDeDe <= 30.1f) DonnerPotion();
        else DonnerCristaux();
    }

    private void DonnerCristaux()
    {
        int min = 1, max = 5; 
        if (niveauActuel == 2) { min = 10; max = 15; }
        else if (niveauActuel == 3) { min = 20; max = 25; }
        else if (niveauActuel == 4) { min = 30; max = 35; }
        else if (niveauActuel == 5) { min = 40; max = 45; }

        int gain = UnityEngine.Random.Range(min, max + 1); 
        GameManager.Instance.temporalCrystals += gain;
        PlayerPrefs.SetInt("temporalCrystals", GameManager.Instance.temporalCrystals);
        
        texteRecompenseObtenue.text = "+" + gain + " Cristaux !";
        if(iconeRecompense != null) iconeRecompense.sprite = imageCristaux;
    }

    private void DonnerPotion()
    {
        float secondes = 1800f; 
        if (niveauActuel == 2) secondes = 3600f; 
        else if (niveauActuel == 3) secondes = 7200f; 
        else if (niveauActuel == 4) secondes = UnityEngine.Random.Range(7200f, 18000f); 
        else if (niveauActuel == 5) secondes = UnityEngine.Random.Range(18000f, 36000f); 

        double gainMana = GameManager.Instance.manaPerSecond * secondes;
        GameManager.Instance.AddMana(gainMana);
        
        int heuresFormatees = Mathf.RoundToInt(secondes / 3600f);
        texteRecompenseObtenue.text = "Potion " + heuresFormatees + "H !";
        if(iconeRecompense != null) iconeRecompense.sprite = imagePotion;
    }

    private void DonnerBoostPermanent()
    {
        float boost = 2f; 
        if (niveauActuel == 2) boost = 5f;
        else if (niveauActuel == 3) boost = 10f;
        else if (niveauActuel == 4) boost = 15f;
        else if (niveauActuel == 5) boost = 20f;

        float boostActuel = PlayerPrefs.GetFloat("GachaPermanentBoost", 1f);
        PlayerPrefs.SetFloat("GachaPermanentBoost", boostActuel * boost);
        PlayerPrefs.Save();
        GameManager.Instance.RecalculateMultiplier();

        texteRecompenseObtenue.text = "JACKPOT ! Boost x" + boost + " !";
        if(iconeRecompense != null) iconeRecompense.sprite = imageBoost;
    }
}