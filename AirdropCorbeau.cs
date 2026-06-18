using UnityEngine;
using UnityEngine.UI;

public class AirdropCorbeau : MonoBehaviour
{
    [Header("Paramètres de l'Airdrop")]
    public float speed = 150f; // Vitesse de déplacement horizontal (pixels/seconde)
    public float respawnTime = 240f; // Apparaît toutes les 4 minutes (240 secondes)
    
    private float timer = 0f;
    private bool isActive = false;

    private RectTransform rectTransform;
    private Button button;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        button = GetComponent<Button>();
        
        // On écoute le clic du joueur sur l'airdrop
        button.onClick.AddListener(OnClickCorbeau);
        
        ResetCorbeau();
    }

    private void Update()
    {
        if (!isActive)
        {
            // Chronomètre en arrière-plan pour faire spawn le corbeau
            timer += Time.deltaTime;
            if (timer >= respawnTime)
            {
                SpawnCorbeau();
            }
        }
        else
        {
            // Déplacement fluide vers la droite de l'écran
            rectTransform.anchoredPosition += new Vector2(speed * Time.deltaTime, 0);

            // Si le corbeau sort complètement de l'écran (à ajuster selon la taille du Canvas)
            if (rectTransform.anchoredPosition.x > Screen.width + 200)
            {
                ResetCorbeau();
            }
        }
    }

    private void SpawnCorbeau()
    {
        isActive = true;
        timer = 0f;
        
        // Position de départ hors écran à gauche, avec une hauteur Y aléatoire pour varier
        float randomY = Random.Range(-400f, 400f);
        rectTransform.anchoredPosition = new Vector2(-200f, randomY);
        
        gameObject.SetActive(true);
    }

    private void ResetCorbeau()
    {
        isActive = false;
        gameObject.SetActive(false);
    }

    public void OnClickCorbeau()
    {
        // Dans une version finale, ce clic ouvre un Pop-up demandant de regarder une pub
        // et on appelle la récompense depuis le callback de AdMobManager.
        
        Debug.Log("Le Corbeau a été touché ! Gain : 30 minutes de production.");
        
        // Calcul de la récompense : 30 minutes = 1800 secondes
        double reward = GameManager.Instance.manaPerSecond * 1800;
        
        // On donne la récompense au joueur
        GameManager.Instance.AddMana(reward);
        
        // On fait disparaître le corbeau
        ResetCorbeau();
    }
}