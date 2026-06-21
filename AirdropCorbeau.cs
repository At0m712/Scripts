using UnityEngine;
using UnityEngine.UI;
using TMPro; // 1. AJOUT OBLIGATOIRE pour utiliser TextMeshProUGUI

public class AirdropCorbeau : MonoBehaviour
{
    public float speed = 200f; 
    public float respawnTime = 180f; // Apparaît toutes les 3 minutes
    
    private float timer = 0f;
    private bool isActive = false;
    private RectTransform rectTransform;
    
    // 2. Ajout de ces variables pour masquer l'oiseau plutôt que de le désactiver
    private Image corbeauImage;
    private Button corbeauButton;

    [Header("Effets Visuels")]
    public ParticleSystem explosionPlumes;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        corbeauImage = GetComponent<Image>();
        corbeauButton = GetComponent<Button>();
        
        corbeauButton.onClick.AddListener(OnClickCorbeau);
        
        ResetCorbeau();
    }

    void Update()
    {
        if (!isActive)
        {
            // Le timer peut maintenant tourner car le GameObject reste actif !
            timer += Time.deltaTime;
            if (timer >= respawnTime) SpawnCorbeau();
        }
        else
        {
            // Fait voler le corbeau vers la droite
            rectTransform.anchoredPosition += new Vector2(speed * Time.deltaTime, 0);

            // S'il sort de l'écran par la droite, on le cache
            if (rectTransform.anchoredPosition.x > 1500f) ResetCorbeau();
        }
    }

    private void SpawnCorbeau()
    {
        isActive = true;
        timer = 0f;
        
        // 3. On rend l'image visible et le bouton cliquable
        corbeauImage.enabled = true;
        corbeauButton.interactable = true;

        // Fait spawn le corbeau à gauche, à une hauteur aléatoire
        float randomY = UnityEngine.Random.Range(-500f, 500f);
        rectTransform.anchoredPosition = new Vector2(-600f, randomY);
    }

    private void ResetCorbeau()
    {
        isActive = false;
        
        // 4. On masque l'image et on bloque les clics (le script continue de tourner en fond)
        corbeauImage.enabled = false;
        corbeauButton.interactable = false;
    }

    public void OnClickCorbeau()
    {
        // Gain = 15 minutes de production immédiate
        double reward = GameManager.Instance.manaPerSecond * 900;
        GameManager.Instance.AddMana(reward);
        
        // EFFET VISUEL
        if (explosionPlumes != null)
        {
            explosionPlumes.transform.position = transform.position;
            explosionPlumes.Play();
        }

        if (ObjectPooler.Instance != null)
        {
            // CORRECTION ICI : On donne le Tag (string), la Position (Vector3), et la Rotation (Quaternion.identity = aucune rotation)
            // ⚠️ ATTENTION : Remplace "PopupText" par le VRAI TAG que tu as configuré dans la liste de ton ObjectPooler !
            GameObject popup = ObjectPooler.Instance.SpawnFromPool("PopupText", transform.position, Quaternion.identity);
            
            if (popup != null)
            {
                TextMeshProUGUI textComp = popup.GetComponentInChildren<TextMeshProUGUI>();
                if (textComp != null)
                {
                    textComp.text = "+ " + ScoreUI.FormatNumber(reward);
                    textComp.color = Color.yellow;
                }
            }
        }

        ResetCorbeau();
    }
}