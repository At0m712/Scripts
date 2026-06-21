using UnityEngine;
using System.Collections.Generic;
using System;

public enum ItemType { PotionMana, BoostVitesse }

// Cette classe définit ce qu'est un objet (nom, icone, type, durée...)
[Serializable]
public class ItemDef
{
    [Tooltip("L'ID unique pour la sauvegarde (ex: Inv_Potion1H)")]
    public string idPrefs; 
    public string nom;
    public Sprite icone;
    public ItemType type;
    [Tooltip("La puissance de l'objet en heures (1, 2, 4, etc.)")]
    public float heures; 
}

public class InventaireUI : MonoBehaviour
{
    [Header("Configuration")]
    public GameObject slotPrefab;      // Ton nouveau Prefab Carte_Potion
    public Transform contentPanel;     // Ton objet Content (qui a le GridLayout)

    [Header("Base de données des Objets")]
    public List<ItemDef> baseDeDonneesObjets = new List<ItemDef>();

    void OnEnable()
    {
        UpdateUI();
    }

    void Update()
    {
        // Optionnel : Actualiser les textes de description en direct si la production de mana augmente pendant qu'on regarde l'inventaire
        if (Time.frameCount % 60 == 0) // On le fait toutes les secondes pour optimiser
        {
            UpdateUI();
        }
    }

    public void UpdateUI()
    {
        // 1. On détruit toutes les cases actuellement affichées
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        // 2. On scanne la base de données
        foreach (ItemDef item in baseDeDonneesObjets)
        {
            int quantite = PlayerPrefs.GetInt(item.idPrefs, 0);
            
            // Si le joueur possède au moins 1 exemplaire, on fabrique la case !
            if (quantite > 0)
            {
                GameObject nouveauSlot = Instantiate(slotPrefab, contentPanel);
                SlotInventaireUI slotUI = nouveauSlot.GetComponent<SlotInventaireUI>();
                if (slotUI != null)
                {
                    slotUI.Configurer(item, quantite, this);
                }
            }
        }
    }

    public void Utiliser(ItemDef item)
    {
        int quantite = PlayerPrefs.GetInt(item.idPrefs, 0);
        if (quantite <= 0) return;

        // 1. Retirer 1 objet de l'inventaire
        PlayerPrefs.SetInt(item.idPrefs, quantite - 1);

        // 2. Appliquer les effets
        if (item.type == ItemType.PotionMana)
        {
            if (GameManager.Instance != null)
            {
                double reward = GameManager.Instance.manaPerSecond * 3600 * item.heures;
                GameManager.Instance.AddMana(reward);
            }
        }
        else if (item.type == ItemType.BoostVitesse)
        {
            if (GameManager.Instance != null)
            {
                float secondesAjoutees = item.heures * 3600f;
                DateTime finBonus;
                
                if (GameManager.Instance.adBoostTimer > 0) 
                    finBonus = DateTime.Now.AddSeconds(GameManager.Instance.adBoostTimer).AddSeconds(secondesAjoutees);
                else 
                    finBonus = DateTime.Now.AddSeconds(secondesAjoutees);

                GameManager.Instance.adBoostTimer += secondesAjoutees; 
                GameManager.Instance.adBoostMultiplier = 2.0; 

                PlayerPrefs.SetString("dateFinMultiplicateur", finBonus.ToString());
                PlayerPrefs.SetInt("multiplicateurArgentActuel", 2);
                GameManager.Instance.ActualiserTousLesEtages();
            }
        }

        PlayerPrefs.Save();

        // 3. Son de confirmation
        if (AudioManager.Instance != null && AudioManager.Instance.buySound != null)
            AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.buySound);

        // 4. On rafraîchit l'inventaire complet (ce qui fera disparaître la case s'il tombe à 0)
        UpdateUI(); 
    }

    // --- POUR LE GACHA ---
    // Cette fonction universelle remplace les anciennes !
    public static void AjouterObjet(string idPrefs, int quantite)
    {
        int current = PlayerPrefs.GetInt(idPrefs, 0);
        PlayerPrefs.SetInt(idPrefs, current + quantite);
        PlayerPrefs.Save();
    }
}