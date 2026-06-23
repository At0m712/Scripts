using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Localization;
using System.Linq; // Indispensable pour trier les étages

public class BoutiqueCartesManager : MonoBehaviour
{
    [Header("Références UI")]
    public GameObject cartePrefab;
    public Transform contentPanel;
    public List<UpgradeShopUI> tousLesEtages;

    [Header("Génération Automatique")]
    public bool genererAutomatiquement = true;
    
    [Tooltip("Multiplicateur : La carte coûte X fois le prix de base de l'étage")]
    public float multiplicateurPrixProd = 50f;
    public float multiplicateurPrixCout = 500f;
    public float multiplicateurPrixNiveau = 5000f;

    [Header("Icônes par défaut des Cartes générées")]
    public Sprite iconeProdDefaut;
    public Sprite iconeCoutDefaut;
    public Sprite iconeNivDefaut;

    [Header("Liste des Cartes (Générée)")]
    public List<CarteDef> listeCartes;

    [Header("Localisation")]
    public LocalizedString texteAchatSuccess;

    void Start()
    {
        if (genererAutomatiquement)
        {
            GenererCartes();
        }
        
        RafraichirBoutique();
    }

    // ==========================================
    // ⚙️ GÉNÉRATION AUTOMATIQUE (PROGRESSION PARFAITE)
    // ==========================================
    private void GenererCartes()
    {
        listeCartes.Clear(); // On vide la liste pour repartir à zéro

        // Si la liste des étages est vide dans l'inspecteur, le script les trouve tout seul
        if (tousLesEtages == null || tousLesEtages.Count == 0)
        {
            tousLesEtages = new List<UpgradeShopUI>(FindObjectsOfType<UpgradeShopUI>());
        }

        // 🌟 LA MAGIE EST LÀ : On trie les étages du moins cher au plus cher. 
        // L'étage 1 passera toujours en premier, puis le 2, puis le 3... jusqu'au 8.
        tousLesEtages = tousLesEtages.OrderBy(e => e.myFloorData.baseCost).ToList();

        // 🟢 CYCLE 1 : Production x2 (Prix abordable, pour le début de partie)
        AjouterCycleDeCartes("Prod1_", "Prod. ", multiplicateurPrixProd, TypeBonusCarte.MultiplicateurProduction, 2f, iconeProdDefaut);

        // 🔵 CYCLE 2 : Réduction de Coût -10% (Plus cher, pour le milieu de partie)
        AjouterCycleDeCartes("Cost1_", "Éco. ", multiplicateurPrixCout, TypeBonusCarte.ReductionCout, 0.9f, iconeCoutDefaut);

        // 🟣 CYCLE 3 : Niveau de Départ +5 (Très cher, idéal après un ou deux Prestiges)
        AjouterCycleDeCartes("Level1_", "Héritage ", multiplicateurPrixNiveau, TypeBonusCarte.NiveauDepart, 5f, iconeNivDefaut);

        // 🔴 CYCLE 4 : Production x3 (Prix End-Game, x1000)
        AjouterCycleDeCartes("Prod2_", "Maitre ", multiplicateurPrixProd * 1000f, TypeBonusCarte.MultiplicateurProduction, 3f, iconeProdDefaut);

        // 🟡 CYCLE 5 : Réduction de Coût -15% (Prix Ultra End-Game)
        AjouterCycleDeCartes("Cost2_", "Radin ", multiplicateurPrixCout * 1000f, TypeBonusCarte.ReductionCout, 0.85f, iconeCoutDefaut);
        
        // Résultat final : 40 Cartes générées, rangées dans un ordre de difficulté parfait !
    }

    // Fonction qui crée les 8 cartes d'un cycle
    private void AjouterCycleDeCartes(string prefixeID, string prefixeNom, float multiplicateurPrix, TypeBonusCarte typeBonus, float valeurBonus, Sprite icone)
    {
        foreach (UpgradeShopUI etage in tousLesEtages)
        {
            if (etage == null || etage.myFloorData == null) continue;
            FloorData floor = etage.myFloorData;
            
            CarteDef carte = ScriptableObject.CreateInstance<CarteDef>();
            
            // ID fixe pour ne jamais perdre la sauvegarde du joueur
            carte.idUnique = prefixeID + floor.name; 
            carte.nomCarte = prefixeNom + floor.name;
            
            // Le prix est proportionnel au prix de l'étage
            carte.prixMana = floor.baseCost * multiplicateurPrix;
            if (carte.prixMana <= 0) carte.prixMana = 100 * multiplicateurPrix;

            carte.etageCible = floor;
            carte.typeBonus = typeBonus;
            carte.valeurBonus = valeurBonus;
            carte.iconeCarte = icone;

            listeCartes.Add(carte);
        }
    }

    // ==========================================
    // 🛒 GESTION DE LA BOUTIQUE
    // ==========================================
    public void RafraichirBoutique()
    {
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        foreach (CarteDef carte in listeCartes)
        {
            if (carte == null) continue; 

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
            
            AppliquerBonus(carte);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ActualiserTousLesEtages();
            }

            Destroy(uiObject);
            
            if (SaveManager.Instance != null) SaveManager.Instance.DemanderSauvegarde();
            else PlayerPrefs.Save();
            
            if (AudioManager.Instance != null && AudioManager.Instance.buySound != null)
                AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.buySound);
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
        else if (carte.typeBonus == TypeBonusCarte.NiveauDepart) 
        {
            int actuel = PlayerPrefs.GetInt("BonusLevels_" + nomEtage, 0);
            PlayerPrefs.SetInt("BonusLevels_" + nomEtage, actuel + (int)carte.valeurBonus);
        }
    }
}