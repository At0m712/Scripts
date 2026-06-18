using UnityEngine;

public class LiensLegaux : MonoBehaviour
{
    private string urlPrivacyPolicy = "https://votre-site.com/privacy-policy";
    private string urlTerms = "https://votre-site.com/terms";

    public void OpenPrivacyPolicy()
    {
        Application.OpenURL(urlPrivacyPolicy);
    }

    public void OpenTermsOfService()
    {
        Application.OpenURL(urlTerms);
    }
}