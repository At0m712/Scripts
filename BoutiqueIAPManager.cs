using UnityEngine;

public class BoutiqueIAPManager : MonoBehaviour
{
    public const string NO_ADS_ID = "com.tonnom.latourdusorcier.noads";
    public const string STARTER_PACK_ID = "com.tonnom.latourdusorcier.starterpack";

    public void BuyNoAds()
    {
        Debug.Log("[IAP] Tentative d'achat de NoAds...");
        // Logique Unity IAP ici
        // En cas de succès : UnlockNoAds();
    }

    private void UnlockNoAds()
    {
        PlayerPrefs.SetInt("HasNoAds", 1);
        PlayerPrefs.Save();
        Debug.Log("Les publicités forcées sont désactivées !");
    }

    public bool HasNoAds()
    {
        return PlayerPrefs.GetInt("HasNoAds", 0) == 1;
    }
}