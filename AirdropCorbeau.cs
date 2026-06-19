using UnityEngine;
using UnityEngine.UI;

public class AirdropCorbeau : MonoBehaviour
{
    public float speed = 200f; 
    public float respawnTime = 180f; // Apparaît toutes les 3 minutes
    
    private float timer = 0f;
    private bool isActive = false;
    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        GetComponent<Button>().onClick.AddListener(OnClickCorbeau);
        ResetCorbeau();
    }

    void Update()
    {
        if (!isActive)
        {
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
        
        // Fait spawn le corbeau à gauche, à une hauteur aléatoire
        float randomY = UnityEngine.Random.Range(-500f, 500f);
        rectTransform.anchoredPosition = new Vector2(-600f, randomY);
        gameObject.SetActive(true);
    }

    private void ResetCorbeau()
    {
        isActive = false;
        gameObject.SetActive(false);
    }

    public void OnClickCorbeau()
    {
        // Gain = 15 minutes de production immédiate
        double reward = GameManager.Instance.manaPerSecond * 900;
        GameManager.Instance.AddMana(reward);
        
        // TODO: Jouer particule explosion de plumes
        ResetCorbeau();
    }
}