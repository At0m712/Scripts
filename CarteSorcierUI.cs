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
    public LocalizedString texteDescProduction; // Clé ex: Production x{0}
    public LocalizedString texteDescCout;       // Clé ex: Prix -{0}%
    public LocalizedString texteDescNiveau;     // Clé ex: Démarre Niv. {0}
    public LocalizedString textePrixMana;       // Clé ex: {0} Mana

    private CarteDef maCarte;
    private BoutiqueCartesManager monManager;

    public void Configurer(CarteDef carte, BoutiqueCartesManager manager)
    {
        maCarte = carte;
        monManager = manager;

        if (iconeObjet != null && carte.icone != null) iconeObjet.sprite = carte.icone;
        
        // Le nom peut rester celui généré car c'est un nom propre
        if (titreText != null) titreText.text = carte.nomCarte;

        // --- MAGIE DE LA LOCALISATION DYNAMIQUE ---
        // On n'utilise plus la description en dur générée, on la recrée proprement dans la langue du joueur !
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
            else if (carte.typeBonus == TypeBonusCarte.NiveauxDepart)
            {
                texteDescNiveau.Arguments = new object[] { carte.valeurBonus };
                descriptionText.text = texteDescNiveau.GetLocalizedString();
            }
        }

        // Localisation du prix
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