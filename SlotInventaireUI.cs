using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SlotInventaireUI : MonoBehaviour
{
    [Header("Éléments Visuels")]
    public Image iconeObjet;
    public TextMeshProUGUI nomText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI quantiteText;
    public Button utiliserBtn;

    private ItemDef monItem;
    private InventaireUI monInventaire;

    // Cette fonction est appelée par l'Inventaire pour remplir la case
    public void Configurer(ItemDef item, int quantite, InventaireUI inventaire)
    {
        monItem = item;
        monInventaire = inventaire;

        iconeObjet.sprite = item.icone;
        nomText.text = item.nom;
        quantiteText.text = "x" + quantite;

        // On calcule et on affiche les gains en temps réel !
        if (item.type == ItemType.PotionMana)
        {
            double gain = (GameManager.Instance != null) ? GameManager.Instance.manaPerSecond * 3600 * item.heures : 0;
            descriptionText.text = "Gain immédiat : <color=#00BFFF>" + ScoreUI.FormatNumber(gain) + " Mana</color>";
        }
        else if (item.type == ItemType.BoostVitesse)
        {
            descriptionText.text = "Vitesse x2 pendant <color=#FFD700>" + item.heures + " Heure(s)</color>";
        }

        // On assigne l'action au bouton
        utiliserBtn.onClick.RemoveAllListeners();
        utiliserBtn.onClick.AddListener(UtiliserObjet);
    }

    private void UtiliserObjet()
    {
        // On prévient l'inventaire principal qu'on veut consommer cet objet
        monInventaire.Utiliser(monItem);
    }
}