using UnityEngine;
using TMPro;
using UnityEngine.UI;

public enum BuyMode { x1, x10, x100, MAX, NEXT }

public class BuyModeManager : MonoBehaviour
{
    public static BuyModeManager Instance;

    public BuyMode currentMode = BuyMode.x1;
    public Button toggleButton;
    public TextMeshProUGUI modeText;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (toggleButton != null) toggleButton.onClick.AddListener(ToggleMode);
        UpdateUI();
    }

    public void ToggleMode()
    {
        switch (currentMode)
        {
            case BuyMode.x1: currentMode = BuyMode.x10; break;
            case BuyMode.x10: currentMode = BuyMode.x100; break;
            case BuyMode.x100: currentMode = BuyMode.MAX; break;
            case BuyMode.MAX: currentMode = BuyMode.NEXT; break;
            case BuyMode.NEXT: currentMode = BuyMode.x1; break;
        }
        
        UpdateUI();
        
        // OPTIMISATION MAX : On utilise la liste pré-chargée en mémoire
        for (int i = 0; i < UpgradeShopUI.AllShops.Count; i++)
        {
            UpgradeShopUI.AllShops[i].RecalculerStats();
        }
    }

    private void UpdateUI()
    {
        if (modeText != null)
        {
            switch (currentMode)
            {
                case BuyMode.x1: modeText.text = "ACHAT x1"; break;
                case BuyMode.x10: modeText.text = "ACHAT x10"; break;
                case BuyMode.x100: modeText.text = "ACHAT x100"; break;
                case BuyMode.MAX: modeText.text = "ACHAT MAX"; break;
                case BuyMode.NEXT: modeText.text = "ACHAT NEXT"; break;
            }
        }
    }
}