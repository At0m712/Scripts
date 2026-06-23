using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Localization;

public class CarteSorcierUI : MonoBehaviour
{
    [Header("Éléments Visuels")]
    public Image iconeObjet;
    public TextMeshProUGUI titreText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI prixText;
    public Button boutonAchat;

    [Header("Localisation des Descriptions")]
    public LocalizedString texteDescProduction; 
    public LocalizedString texteDescCout;       
    public LocalizedString texteDescNiveau;     
    public LocalizedString textePrixMana;       

    private CarteDef maCarte;
    private BoutiqueCartesManager monManager;

    public void Configurer(CarteDef carte, BoutiqueCartesManager manager)
    {
        maCarte = carte;
        monManager = manager;

        // <-- CORRIGÉ ICI (iconeCarte au lieu de icone)
        if (iconeObjet != null && carte.iconeCarte != null) iconeObjet.sprite = carte.iconeCarte;
        
        if (titreText != null) titreText.text = carte.nomCarte;

        if (descriptionText != null)
        {
            if (carte.typeBonus == TypeBonusCarte.MultiplicateurProduction)
            {
                texteDescProduction.Arguments = new object[] { carte.valeurBonus.ToString("F2") };
                descriptionText.text = texteDescProduction.GetLocalizedString();
            }
            else if (carte.typeBonus == TypeBonusCarte.ReductionCout)
            {
                int pourcentageSoustrait = Mathf.RoundToInt((1f - carte.valeurBonus) * 100f);
                texteDescCout.Arguments = new object[] { pourcentageSoustrait };
                descriptionText.text = texteDescCout.GetLocalizedString();
            }
            else if (carte.typeBonus == TypeBonusCarte.NiveauDepart) // <-- CORRIGÉ ICI (Singulier)
            {
                texteDescNiveau.Arguments = new object[] { carte.valeurBonus };
                descriptionText.text = texteDescNiveau.GetLocalizedString();
            }
        }

        if (prixText != null) 
        {
            textePrixMana.Arguments = new object[] { ScoreUI.FormatNumber(carte.prixMana) };
            prixText.text = textePrixMana.GetLocalizedString();
        }

        if (boutonAchat != null)
        {
            boutonAchat.onClick.RemoveAllListeners();
            boutonAchat.onClick.AddListener(Acheter);
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && boutonAchat != null)
        {
            boutonAchat.interactable = (GameManager.Instance.manaCurrent >= maCarte.prixMana);
        }
    }

    private void Acheter()
    {
        monManager.AcheterCarte(maCarte, this.gameObject);
    }
}