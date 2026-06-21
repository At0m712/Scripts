using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CarteSorcierUI : MonoBehaviour
{
    [Header("Éléments Visuels")]
    public Image iconeObjet;
    public TextMeshProUGUI titreText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI prixText;
    public Button boutonAchat;

    private CarteDef maCarte;
    private BoutiqueCartesManager monManager;

    // Rempli automatiquement par le Manager
    public void Configurer(CarteDef carte, BoutiqueCartesManager manager)
    {
        maCarte = carte;
        monManager = manager;

        if (iconeObjet != null && carte.icone != null) iconeObjet.sprite = carte.icone;
        if (titreText != null) titreText.text = carte.nomCarte;
        if (descriptionText != null) descriptionText.text = carte.description;
        if (prixText != null) prixText.text = ScoreUI.FormatNumber(carte.prixMana) + " Mana";

        // Nettoie et connecte le bouton
        if (boutonAchat != null)
        {
            boutonAchat.onClick.RemoveAllListeners();
            boutonAchat.onClick.AddListener(Acheter);
        }
    }

    void Update()
    {
        // Grise le bouton si pas assez de Mana
        if (GameManager.Instance != null && boutonAchat != null)
        {
            boutonAchat.interactable = (GameManager.Instance.manaCurrent >= maCarte.prixMana);
        }
    }

    private void Acheter()
    {
        // Envoie l'ordre d'achat au grand Manager
        monManager.AcheterCarte(maCarte, this.gameObject);
    }
}