using UnityEngine;
using System.Collections.Generic;
using System;

public enum TypeBonusCarte { MultiplicateurProduction, ReductionCout, NiveauxDepart }

[Serializable]
public class CarteDef
{
    public string idUnique; 
    public string nomCarte;
    public string description;
    public Sprite icone;
    public FloorData etageCible;
    public TypeBonusCarte typeBonus;
    public float valeurBonus; 
    public double prixMana;
}

public class BoutiqueCartesManager : MonoBehaviour
{
    [Header("Configuration UI")]
    public GameObject cartePrefab; 
    public Transform contentPanel; 

    [Header("Base de données des Améliorations")]
    public List<CarteDef> listeCartes = new List<CarteDef>();

    [Header("=== OUTIL DE GÉNÉRATION ===")]
    public FloorData[] tousLesEtages;
    public Sprite iconeParDefaut;

    void OnEnable()
    {
        GenererBoutique();
    }

    public void GenererBoutique()
    {
        foreach (Transform child in contentPanel) Destroy(child.gameObject);

        foreach (CarteDef carte in listeCartes)
        {
            if (PlayerPrefs.GetInt("Achete_" + carte.idUnique, 0) == 0) 
            {
                GameObject nouvelleCarte = Instantiate(cartePrefab, contentPanel);
                CarteSorcierUI scriptCarte = nouvelleCarte.GetComponent<CarteSorcierUI>();
                if (scriptCarte != null) scriptCarte.Configurer(carte, this);
            }
        }
    }

    public void AcheterCarte(CarteDef carte, GameObject objetUI)
    {
        if (GameManager.Instance.SpendMana(carte.prixMana))
        {
            PlayerPrefs.SetInt("Achete_" + carte.idUnique, 1);
            string nomEtage = carte.etageCible.name;
            
            if (carte.typeBonus == TypeBonusCarte.MultiplicateurProduction)
            {
                float currentMulti = PlayerPrefs.GetFloat("BonusProd_" + nomEtage, 1f);
                float bonusAAjouter = carte.valeurBonus - 1f; 
                PlayerPrefs.SetFloat("BonusProd_" + nomEtage, currentMulti + bonusAAjouter);
            }
            else if (carte.typeBonus == TypeBonusCarte.ReductionCout)
            {
                float currentReduc = PlayerPrefs.GetFloat("BonusCost_" + nomEtage, 1f);
                PlayerPrefs.SetFloat("BonusCost_" + nomEtage, currentReduc * carte.valeurBonus);
            }
            else if (carte.typeBonus == TypeBonusCarte.NiveauxDepart)
            {
                int currentLevels = PlayerPrefs.GetInt("BonusLevels_" + nomEtage, 0);
                PlayerPrefs.SetInt("BonusLevels_" + nomEtage, currentLevels + (int)carte.valeurBonus);
            }

            PlayerPrefs.Save();

            if (AudioManager.Instance != null && AudioManager.Instance.buySound != null)
                AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.buySound);

            Destroy(objetUI); 

            // OPTIMISATION MAX : Informe l'étage qu'il doit recharger son PlayerPrefs mis en cache
            for(int i = 0; i < UpgradeShopUI.AllShops.Count; i++)
            {
                if(UpgradeShopUI.AllShops[i].myFloorData == carte.etageCible)
                {
                    UpgradeShopUI.AllShops[i].ChargerBonus();
                }
            }
            
            GameManager.Instance.ActualiserTousLesEtages(); 
        }
    }

    // Le générateur automatique (outil Unity) est conservé parfaitement identique.
    // ... Garder votre fonction existante : [ContextMenu("Générer les Cartes Automatiquement")]

    // ==============================================================================
    // OUTIL DE GÉNÉRATION DES VAGUES (10 PALIERS - 208 CARTES)
    // ==============================================================================
    [ContextMenu("Générer les Cartes Automatiquement")]
    public void GenererToutesLesCartes()
    {
        if (tousLesEtages == null || tousLesEtages.Length == 0)
        {
            Debug.LogError("Attention : Tu dois d'abord glisser tes 8 FloorData dans le tableau 'Tous Les Etages' !");
            return;
        }

        listeCartes.Clear();

        int nbPaliers = 10;

        // --- NOMS DES CARTES (10 mots différents pour marquer la puissance) ---
        string[] motsProd = { "Baguette", "Parchemin", "Livre", "Grimoire", "Artefact", "Relique", "Aura", "Savoir", "Omniscience", "Transcendance" };
        string[] motsCout = { "Négociation", "Accord", "Contrat", "Pacte", "Alliance", "Syndicat", "Monopole", "Emprise", "Domination", "Maîtrise" };
        string[] motsStart = { "", "", "", "", "Éveil", "Héritage", "Privilège", "Vétéran", "Légende", "Ascension" };

        // --- VALEURS DES BONUS ---
        // Diversité demandée : beaucoup de x1.25, x1.5, x2...
        float[] valProd = { 1.25f, 1.25f, 1.50f, 1.50f, 1.75f, 1.75f, 2.00f, 2.00f, 2.50f, 3.00f }; 
        // Coût bridé strictement à -20% max (donc 0.80 du prix)
        float[] valCout = { 0.90f, 0.90f, 0.85f, 0.85f, 0.80f, 0.80f, 0.80f, 0.80f, 0.80f, 0.80f }; 
        // Niveaux de départ : 0 au début, s'active au Palier 4 (le 5ème palier)
        float[] valStart = { 0f, 0f, 0f, 0f, 5f, 10f, 15f, 20f, 25f, 50f }; 

        // --- COURBE DE PRIX (Croissance exponentielle) ---
        // Multiplicateur massif pour que le Palier 9 coûte des millions de milliards
        double[] multiplicateurTier = { 
            1d, 500d, 250000d, 125000000d, 62500000000d, 
            3.1e13, 1.5e16, 7.5e18, 3.7e21, 1.8e24 
        };

        // ON BOUCLE SUR LES 10 PALIERS (Pour générer dans le bon ordre d'affichage)
        for (int tier = 0; tier < nbPaliers; tier++)
        {
            // VAGUE 1 : PRODUCTION (Fait le tour des étages 1 à 8)
            for (int e = 0; e < tousLesEtages.Length; e++)
            {
                FloorData etage = tousLesEtages[e];
                double prixBaseEtage = 100 * Math.Pow(10, e); 

                CarteDef carteProd = new CarteDef();
                carteProd.idUnique = "Amelio_Prod_" + etage.name.Replace(" ", "") + "_T" + tier;
                carteProd.nomCarte = motsProd[tier] + " (" + etage.name + ")";
                carteProd.description = "Production : x" + valProd[tier].ToString("F2");
                carteProd.etageCible = etage;
                carteProd.typeBonus = TypeBonusCarte.MultiplicateurProduction;
                carteProd.valeurBonus = valProd[tier];
                carteProd.prixMana = prixBaseEtage * 5 * multiplicateurTier[tier];
                carteProd.icone = iconeParDefaut;
                listeCartes.Add(carteProd);
            }

            // VAGUE 2 : RÉDUCTION DE COÛT (Fait le tour des étages 1 à 8)
            for (int e = 0; e < tousLesEtages.Length; e++)
            {
                FloorData etage = tousLesEtages[e];
                double prixBaseEtage = 100 * Math.Pow(10, e);

                CarteDef carteCout = new CarteDef();
                carteCout.idUnique = "Amelio_Cout_" + etage.name.Replace(" ", "") + "_T" + tier;
                carteCout.nomCarte = motsCout[tier] + " (" + etage.name + ")";
                carteCout.description = "Coût d'achat : -" + Math.Round((1f - valCout[tier]) * 100f) + "%";
                carteCout.etageCible = etage;
                carteCout.typeBonus = TypeBonusCarte.ReductionCout;
                carteCout.valeurBonus = valCout[tier];
                carteCout.prixMana = prixBaseEtage * 15 * multiplicateurTier[tier];
                carteCout.icone = iconeParDefaut;
                listeCartes.Add(carteCout);
            }

            // VAGUE 3 : NIVEAUX DE DÉPART (Uniquement si le Palier actuel l'autorise)
            if (valStart[tier] > 0)
            {
                for (int e = 0; e < tousLesEtages.Length; e++)
                {
                    FloorData etage = tousLesEtages[e];
                    double prixBaseEtage = 100 * Math.Pow(10, e);

                    CarteDef carteStart = new CarteDef();
                    carteStart.idUnique = "Amelio_Start_" + etage.name.Replace(" ", "") + "_T" + tier;
                    carteStart.nomCarte = motsStart[tier] + " (" + etage.name + ")";
                    carteStart.description = "Prestige : Démarre Niv. " + valStart[tier];
                    carteStart.etageCible = etage;
                    carteStart.typeBonus = TypeBonusCarte.NiveauxDepart;
                    carteStart.valeurBonus = valStart[tier];
                    carteStart.prixMana = prixBaseEtage * 500 * multiplicateurTier[tier]; // Très cher !
                    carteStart.icone = iconeParDefaut;
                    listeCartes.Add(carteStart);
                }
            }
        }

        Debug.Log("✅ SUCCÈS : " + listeCartes.Count + " Cartes d'amélioration ont été générées ! N'oubliez pas de sauvegarder.");
    }   
}