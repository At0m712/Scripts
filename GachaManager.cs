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
    public Button boutonActionPub; 
    public TextMeshProUGUI texteBoutonAction; 
    public Button boutonFermer;

    [Header("Animation - Éléments de la Scène")]
    public GameObject machineConteneur; 
    public GameObject zoneAnimationCapsule; 
    public Image iconeRecompense; 

    [Header("Animation - Détails Capsule")]
    public GameObject capsuleFermee;
    public GameObject capsuleOuverteHaut;
    public GameObject capsuleOuverteBas;
    public GameObject effetLumiereVFX;
    public float hauteurOuverture = 150f; // De combien de pixels le haut de la capsule monte
    
    [Header("Sprites")]
    public Sprite imagePotion;
    public Sprite imageCristaux;
    public Sprite imageBoost;

    [Header("Paramètres")]
    private int niveauActuel = 1;
    private int tiragesRestants = 15;
    private const int MAX_TIRAGES = 15;
    private bool isAnimating = false; 

    // Mémoire de la position initiale pour ne pas que la capsule s'envole à l'infini entre 2 tirages
    private Vector3 positionInitialeCapsuleHaut;

    void Start()
    {
        ChargerSauvegardes();
        VerifierResetQuotidien();
        MettreAJourUI();
        
        if (zoneAnimationCapsule != null) zoneAnimationCapsule.SetActive(false);
        
        // On mémorise la position de base du "chapeau" de la capsule
        if (capsuleOuverteHaut != null) positionInitialeCapsuleHaut = capsuleOuverteHaut.transform.localPosition;
    }

    void Update()
    {
        // Fait tourner l'effet de lumière sur lui-même en continu quand il est affiché
        if (effetLumiereVFX != null && effetLumiereVFX.activeSelf)
        {
            effetLumiereVFX.transform.Rotate(0f, 0f, -100f * Time.deltaTime);
        }
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

            if (boutonActionPub != null)
            {
                boutonActionPub.interactable = (tiragesRestants > 0);
                
                if (niveauActuel < 5)
                {
                    if (texteBoutonAction != null) texteBoutonAction.text = "Invoquer & Améliorer\n(Pub)";
                }
                else
                {
                    if (texteBoutonAction != null) texteBoutonAction.text = "Invoquer\n(Pub)";
                }
            }
        }
    }

    public void OnClickBoutonAction()
    {
        if (isAnimating || tiragesRestants <= 0) return;

        // C'EST ICI QUE TU LANCERAS TA PUB ADMOB PLUS TARD.
        OnPubTerminee();
    }

    public void OnPubTerminee()
    {
        tiragesRestants--;
        PlayerPrefs.SetInt("GachaTirages", tiragesRestants);
        PlayerPrefs.Save();
        
        StartCoroutine(SequenceTirageGacha());
    }

    // ==========================================
    // 🎬 ANIMATION CINÉMATOGRAPHIQUE
    // ==========================================
    private IEnumerator SequenceTirageGacha()
    {
        isAnimating = true;
        VerrouillerInterface();
        texteRecompenseObtenue.text = "Invocation en cours...";

        // --- PRÉPARATION : On cache et réinitialise tout ---
        zoneAnimationCapsule.SetActive(false);
        if(capsuleFermee != null) capsuleFermee.SetActive(true);
        if(capsuleOuverteHaut != null) 
        {
            capsuleOuverteHaut.SetActive(false);
            capsuleOuverteHaut.transform.localPosition = positionInitialeCapsuleHaut;
        }
        if(capsuleOuverteBas != null) capsuleOuverteBas.SetActive(false);
        if(effetLumiereVFX != null) effetLumiereVFX.SetActive(false);
        
        iconeRecompense.transform.localScale = Vector3.zero; 
        texteRecompenseObtenue.gameObject.SetActive(false);

        // --- ÉTAPE 1 : Tremblement de la machine ---
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

        // Amélioration de la machine si besoin
        if (niveauActuel < 5)
        {
            niveauActuel++;
            PlayerPrefs.SetInt("GachaLevel", niveauActuel);
            PlayerPrefs.Save();
        }

        // On détermine mathématiquement ce que le joueur gagne
        EffectuerTirage();

        // --- ÉTAPE 2 : La capsule fermée s'approche (Grossit) ---
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

        yield return new WaitForSeconds(0.2f); // Petite pause de suspense

        // --- ÉTAPE 3 : La capsule s'ouvre ---
        if(capsuleFermee != null) capsuleFermee.SetActive(false);
        if(capsuleOuverteHaut != null) capsuleOuverteHaut.SetActive(true);
        if(capsuleOuverteBas != null) capsuleOuverteBas.SetActive(true);

        Vector3 cibleHaut = positionInitialeCapsuleHaut + new Vector3(0f, hauteurOuverture, 0f);

        float tempsOuverture = 0f;
        while (tempsOuverture < 0.3f)
        {
            tempsOuverture += Time.deltaTime;
            if(capsuleOuverteHaut != null)
                capsuleOuverteHaut.transform.localPosition = Vector3.Lerp(positionInitialeCapsuleHaut, cibleHaut, tempsOuverture / 0.3f);
            yield return null;
        }

        // --- ÉTAPE 4 : Apparition de la récompense et du VFX ---
        if(effetLumiereVFX != null) effetLumiereVFX.SetActive(true);
        texteRecompenseObtenue.gameObject.SetActive(true);

        float tempsRecompense = 0f;
        while (tempsRecompense < 0.4f)
        {
            tempsRecompense += Time.deltaTime;
            // On utilise un effet "Overshoot" mathématique pour que l'icône rebondisse légèrement (facultatif mais cool)
            float scale = Mathf.Lerp(0f, 1f, tempsRecompense / 0.4f); 
            iconeRecompense.transform.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        // --- FIN DE L'ANIMATION ---
        isAnimating = false;
        if (boutonActionPub != null) boutonActionPub.gameObject.SetActive(true);
        MettreAJourUI();
    }

    private void VerrouillerInterface()
    {
        if (boutonActionPub != null) boutonActionPub.gameObject.SetActive(false);
        if (boutonFermer != null) boutonFermer.interactable = false;
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
        string idPotion = "potion_1h";
        string nomPotion = "Potion 1H";

        if (niveauActuel == 1) { idPotion = "potion_1h"; nomPotion = "Potion 1H"; }
        else if (niveauActuel == 2) { idPotion = "potion_2h"; nomPotion = "Potion 2H"; }
        else if (niveauActuel == 3) { idPotion = "potion_4h"; nomPotion = "Potion 4H"; }
        else if (niveauActuel == 4) { idPotion = "potion_8h"; nomPotion = "Potion 8H"; }
        else if (niveauActuel == 5) { idPotion = "potion_12h"; nomPotion = "Potion 12H"; }

        InventaireUI.AjouterObjet(idPotion, 1);
        
        texteRecompenseObtenue.text = nomPotion + "\n(Ajoutée à l'Inventaire) !";
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