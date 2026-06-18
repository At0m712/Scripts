using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MultiplicateurUI : MonoBehaviour
{
    public GameObject prestigePanel;
    public Button prestigeButton;
    public TextMeshProUGUI rewardText;

    private double pendingCrystals = 0;

    private void Update()
    {
        // Déblocage uniquement si le mana total généré dans la partie dépasse 1 Trillion (1 000 000 000 000)
        if (GameManager.Instance.manaTotalProduced >= 1000000000000d)
        {
            prestigeButton.interactable = true;
            CalculatePendingCrystals();
        }
        else
        {
            prestigeButton.interactable = false;
        }
    }

    private void CalculatePendingCrystals()
    {
        // Formule du GDD : Racine Cubique du (Mana Total / 1 Milliard)
        double baseValue = GameManager.Instance.manaTotalProduced / 1000000000d;
        pendingCrystals = System.Math.Pow(baseValue, 1f / 3f);
        
        ScoreUI scoreUI = FindObjectOfType<ScoreUI>();
        rewardText.text = "Détruire la tour pour gagner\n" + scoreUI.FormatNumber(pendingCrystals) + " Cristaux";
    }

    public void TriggerPrestige()
    {
        // Ajout des cristaux
        GameManager.Instance.temporalCrystals += System.Math.Floor(pendingCrystals);

        // Reset des valeurs monétaires
        GameManager.Instance.manaCurrent = 0;
        GameManager.Instance.manaTotalProduced = 0;
        GameManager.Instance.manaPerSecond = 0;

        // Reset des étages : Détruit et recrée ou remet à niveau 0
        UpgradeShopUI[] allFloors = FindObjectsOfType<UpgradeShopUI>();
        foreach (var floor in allFloors)
        {
            floor.currentLevel = 0;
            // La logique complète nécessiterait de re-vérrouiller les étages 2 à 10
        }

        FindObjectOfType<SaveManager>().SaveGame();
        prestigePanel.SetActive(false);
    }
}