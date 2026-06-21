using UnityEngine;
using TMPro;
using UnityEngine.Localization; 

public enum BuyMode { x1, x10, x100, MAX, NEXT }

public class BuyModeManager : MonoBehaviour
{
    public static BuyModeManager Instance;
    public BuyMode currentMode = BuyMode.x1;
    
    public TextMeshProUGUI modeText;

    [Header("Localisation")]
    public LocalizedString texteAchatX1;
    public LocalizedString texteAchatX10;
    public LocalizedString texteAchatX100;
    public LocalizedString texteAchatMax;
    public LocalizedString texteAchatSuivant;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        MettreAJourTexte();
    }

    public void ToggleMode()
    {
        if (currentMode == BuyMode.x1) currentMode = BuyMode.x10;
        else if (currentMode == BuyMode.x10) currentMode = BuyMode.x100;
        else if (currentMode == BuyMode.x100) currentMode = BuyMode.MAX;
        else if (currentMode == BuyMode.MAX) currentMode = BuyMode.NEXT;
        else currentMode = BuyMode.x1;

        MettreAJourTexte();
        
        if (GameManager.Instance != null) GameManager.Instance.ActualiserTousLesEtages();
    }

    private void MettreAJourTexte()
    {
        if (modeText == null) return;

        if (currentMode == BuyMode.x1) modeText.text = texteAchatX1.GetLocalizedString();
        else if (currentMode == BuyMode.x10) modeText.text = texteAchatX10.GetLocalizedString();
        else if (currentMode == BuyMode.x100) modeText.text = texteAchatX100.GetLocalizedString();
        else if (currentMode == BuyMode.MAX) modeText.text = texteAchatMax.GetLocalizedString();
        else if (currentMode == BuyMode.NEXT) modeText.text = texteAchatSuivant.GetLocalizedString();
    }
}