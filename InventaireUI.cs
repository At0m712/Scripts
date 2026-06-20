using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventaireUI : MonoBehaviour
{
    [Header("Potion de Mana (1H de Prod)")]
    public TextMeshProUGUI potionManaCountText;
    public Button utiliserPotionManaBtn;

    [Header("Boost de Vitesse (2H x2)")]
    public TextMeshProUGUI boostVitesseCountText;
    public Button utiliserBoostVitesseBtn;

    void Start()
    {
        if (utiliserPotionManaBtn != null) utiliserPotionManaBtn.onClick.AddListener(UtiliserPotionMana);
        if (utiliserBoostVitesseBtn != null) utiliserBoostVitesseBtn.onClick.AddListener(UtiliserBoostVitesse);
    }

    // OnEnable se déclenche à chaque fois que la fenêtre d'inventaire s'ouvre !
    void OnEnable()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        int potionsMana = PlayerPrefs.GetInt("Inv_PotionMana", 0);
        int boostsVitesse = PlayerPrefs.GetInt("Inv_BoostVitesse", 0);

        if (potionManaCountText != null) potionManaCountText.text = "x" + potionsMana;
        if (boostVitesseCountText != null) boostVitesseCountText.text = "x" + boostsVitesse;

        // On grise le bouton si le joueur n'a pas l'objet
        if (utiliserPotionManaBtn != null) utiliserPotionManaBtn.interactable = (potionsMana > 0);
        if (utiliserBoostVitesseBtn != null) utiliserBoostVitesseBtn.interactable = (boostsVitesse > 0);
    }

    public void UtiliserPotionMana()
    {
        int potions = PlayerPrefs.GetInt("Inv_PotionMana", 0);
        if (potions > 0 && GameManager.Instance != null)
        {
            // 1. On retire l'objet
            potions--;
            PlayerPrefs.SetInt("Inv_PotionMana", potions);
            PlayerPrefs.Save();
            
            // 2. On donne la récompense (1 heure de prod)
            double reward = GameManager.Instance.manaPerSecond * 3600;
            GameManager.Instance.AddMana(reward);

            // 3. Effets
            if (AudioManager.Instance != null && AudioManager.Instance.buySound != null)
                AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.buySound);

            UpdateUI();
        }
    }

    public void UtiliserBoostVitesse()
    {
        int boosts = PlayerPrefs.GetInt("Inv_BoostVitesse", 0);
        if (boosts > 0 && GameManager.Instance != null)
        {
            // 1. On retire l'objet
            boosts--;
            PlayerPrefs.SetInt("Inv_BoostVitesse", boosts);
            PlayerPrefs.Save();
            
            // 2. On donne la récompense (2 heures de x2)
            GameManager.Instance.adBoostTimer += 7200f; 

            // 3. Effets
            if (AudioManager.Instance != null && AudioManager.Instance.buySound != null)
                AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.buySound);

            UpdateUI();
        }
    }
    
    // --- FONCTIONS STATIQUES POUR LE GACHA ---
    // Ces fonctions permettent au Gacha d'ajouter des objets à l'inventaire facilement
    public static void AjouterPotionMana(int quantite)
    {
        int current = PlayerPrefs.GetInt("Inv_PotionMana", 0);
        PlayerPrefs.SetInt("Inv_PotionMana", current + quantite);
        PlayerPrefs.Save();
    }

    public static void AjouterBoostVitesse(int quantite)
    {
        int current = PlayerPrefs.GetInt("Inv_BoostVitesse", 0);
        PlayerPrefs.SetInt("Inv_BoostVitesse", current + quantite);
        PlayerPrefs.Save();
    }
}