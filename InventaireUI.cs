using UnityEngine;
using System.Collections.Generic;
using System;

public enum ItemType { PotionMana, BoostVitesse }

[Serializable]
public class ItemDef
{
    public string idPrefs; 
    public string nom;
    public Sprite icone;
    public ItemType type;
    public float heures; 
}

public class InventaireUI : MonoBehaviour
{
    [Header("Configuration")]
    public GameObject slotPrefab;
    public Transform contentPanel;

    [Header("Base de données des Objets")]
    public List<ItemDef> baseDeDonneesObjets = new List<ItemDef>();

    // OPTIMISATION : Stockage des cases pour éviter de les détruire/créer
    private List<SlotInventaireUI> slotsInstancies = new List<SlotInventaireUI>();
    private bool estInitialise = false;

    void Start()
    {
        InitialiserPool();
    }

    void OnEnable()
    {
        if (estInitialise) UpdateUI();
    }

    private void InitialiserPool()
    {
        // On supprime les vieux éléments du panel (s'il y en a)
        foreach (Transform child in contentPanel) Destroy(child.gameObject);

        // On crée toutes les cases possibles de la base de données, cachées au début
        foreach (ItemDef item in baseDeDonneesObjets)
        {
            GameObject nouveauSlot = Instantiate(slotPrefab, contentPanel);
            SlotInventaireUI slotUI = nouveauSlot.GetComponent<SlotInventaireUI>();
            nouveauSlot.SetActive(false); // Caché par défaut
            slotsInstancies.Add(slotUI);
        }
        estInitialise = true;
        UpdateUI();
    }

    void Update()
    {
        // Moins gourmand qu'un Update total : 1 fois par seconde max
        if (Time.frameCount % 60 == 0) UpdateUI();
    }

    public void UpdateUI()
    {
        if (!estInitialise || slotsInstancies.Count == 0) return;

        for (int i = 0; i < baseDeDonneesObjets.Count; i++)
        {
            ItemDef item = baseDeDonneesObjets[i];
            int quantite = PlayerPrefs.GetInt(item.idPrefs, 0);
            
            if (quantite > 0)
            {
                if (!slotsInstancies[i].gameObject.activeSelf) slotsInstancies[i].gameObject.SetActive(true);
                slotsInstancies[i].Configurer(item, quantite, this);
            }
            else
            {
                if (slotsInstancies[i].gameObject.activeSelf) slotsInstancies[i].gameObject.SetActive(false);
            }
        }
    }

    public void Utiliser(ItemDef item)
    {
        int quantite = PlayerPrefs.GetInt(item.idPrefs, 0);
        if (quantite <= 0) return;

        PlayerPrefs.SetInt(item.idPrefs, quantite - 1);

        if (item.type == ItemType.PotionMana && GameManager.Instance != null)
        {
            double reward = GameManager.Instance.manaPerSecond * 3600 * item.heures;
            GameManager.Instance.AddMana(reward);
        }
        else if (item.type == ItemType.BoostVitesse && GameManager.Instance != null)
        {
            float secondesAjoutees = item.heures * 3600f;
            DateTime finBonus;
            
            if (GameManager.Instance.adBoostTimer > 0) finBonus = DateTime.Now.AddSeconds(GameManager.Instance.adBoostTimer).AddSeconds(secondesAjoutees);
            else finBonus = DateTime.Now.AddSeconds(secondesAjoutees);

            GameManager.Instance.adBoostTimer += secondesAjoutees; 
            GameManager.Instance.adBoostMultiplier = 2.0; 

            PlayerPrefs.SetString("dateFinMultiplicateur", finBonus.ToString());
            PlayerPrefs.SetInt("multiplicateurArgentActuel", 2);
            GameManager.Instance.ActualiserTousLesEtages();
        }

        PlayerPrefs.Save();

        if (AudioManager.Instance != null && AudioManager.Instance.buySound != null)
            AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.buySound);

        UpdateUI(); 
    }

    public static void AjouterObjet(string idPrefs, int quantite)
    {
        int current = PlayerPrefs.GetInt(idPrefs, 0);
        PlayerPrefs.SetInt(idPrefs, current + quantite);
        PlayerPrefs.Save();
    }
}