using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;

public class MachineASousGacha : MonoBehaviour
{
    [Header("UI Elements")]
    public Button boutonTirage;
    public TextMeshProUGUI texteBoutonTirage;
    public int prixTirage = 100;

    [Header("Localisation")]
    public LocalizedString texteTiragePrix; // Clé ex: Ouvrir ({0}💎)
    public LocalizedString texteTirageGratuit; // Clé ex: GRATUIT !

    void Start()
    {
        if (boutonTirage != null)
        {
            boutonTirage.onClick.AddListener(FaireUnTirage);
        }
        MettreAJourBouton();
    }

    void Update()
    {
        MettreAJourBouton();
    }

    private void MettreAJourBouton()
    {
        if (GameManager.Instance == null || boutonTirage == null || texteBoutonTirage == null) return;

        // Si le tirage est payant en cristaux
        if (prixTirage > 0)
        {
            boutonTirage.interactable = (GameManager.Instance.temporalCrystals >= prixTirage);
            
            texteTiragePrix.Arguments = new object[] { prixTirage };
            texteBoutonTirage.text = texteTiragePrix.GetLocalizedString();
        }
        else
        {
            boutonTirage.interactable = true;
            texteBoutonTirage.text = texteTirageGratuit.GetLocalizedString();
        }
    }

    public void FaireUnTirage()
    {
        if (GameManager.Instance.temporalCrystals >= prixTirage)
        {
            GameManager.Instance.temporalCrystals -= prixTirage;
            
            if (AudioManager.Instance != null && AudioManager.Instance.cashSound != null)
                AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.cashSound);

            // --- INSÈRE ICI TA LOGIQUE D'OBTENTION DE CARTE ---
            Debug.Log("Tirage effectué !");
            
            PlayerPrefs.SetInt("temporalCrystals", GameManager.Instance.temporalCrystals);
            PlayerPrefs.Save();
            
            MettreAJourBouton();
        }
    }
}