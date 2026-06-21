using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Localization;

public class BoutiqueCartesManager : MonoBehaviour
{
    public GameObject cartePrefab;
    public Transform contentPanel;
    public List<CarteDef> listeCartes;
    public List<UpgradeShopUI> tousLesEtages;

    [Header("Localisation")]
    public LocalizedString texteAchatSuccess;

    void Start()
    {
        RafraichirBoutique();
    }

    public void RafraichirBoutique()
    {
        // Nettoyage des cartes existantes
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        // Création des nouvelles cartes
        foreach (CarteDef carte in listeCartes)
        {
            if (!PlayerPrefs.HasKey("Achete_" + carte.idUnique))
            {
                GameObject obj = Instantiate(cartePrefab, contentPanel);
                CarteSorcierUI ui = obj.GetComponent<CarteSorcierUI>();
                ui.Configurer(carte, this);
            }
        }
    }

    public void AcheterCarte(CarteDef carte, GameObject uiObject)
    {
        if (GameManager.Instance.manaCurrent >= carte.prixMana)
        {
            GameManager.Instance.SpendMana(carte.prixMana);
            PlayerPrefs.SetInt("Achete_" + carte.idUnique, 1);
            
            // Appliquer le bonus selon le type
            AppliquerBonus(carte);

            // CORRECTION : On demande au GameManager de mettre à jour tous les étages
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ActualiserTousLesEtages();
            }

            Destroy(uiObject);
            PlayerPrefs.Save();
        }
    }

    private void AppliquerBonus(CarteDef carte)
    {
        string nomEtage = carte.etageCible.name;
        
        if (carte.typeBonus == TypeBonusCarte.MultiplicateurProduction)
        {
            float actuel = PlayerPrefs.GetFloat("BonusProd_" + nomEtage, 1f);
            PlayerPrefs.SetFloat("BonusProd_" + nomEtage, actuel * carte.valeurBonus);
        }
        else if (carte.typeBonus == TypeBonusCarte.ReductionCout)
        {
            float actuel = PlayerPrefs.GetFloat("BonusCost_" + nomEtage, 1f);
            PlayerPrefs.SetFloat("BonusCost_" + nomEtage, actuel * carte.valeurBonus);
        }
        else if (carte.typeBonus == TypeBonusCarte.NiveauxDepart)
        {
            int actuel = PlayerPrefs.GetInt("BonusLevels_" + nomEtage, 0);
            PlayerPrefs.SetInt("BonusLevels_" + nomEtage, actuel + (int)carte.valeurBonus);
        }
    }
}