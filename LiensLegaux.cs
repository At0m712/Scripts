using UnityEngine;

public class LiensLegaux : MonoBehaviour
{
    public string urlCGU = "https://votre-site.com/cgu";
    public string urlConfidentialite = "https://votre-site.com/privacy";

    public void OuvrirConditions()
    {
        Application.OpenURL(urlCGU);
    }

    public void OuvrirPolitique()
    {
        Application.OpenURL(urlConfidentialite);
    }
}