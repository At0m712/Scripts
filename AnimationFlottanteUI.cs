using UnityEngine;

public class AnimationFlottanteUI : MonoBehaviour
{
    public float amplitude = 10f; // Hauteur du mouvement
    public float speed = 2f;      // Vitesse du mouvement
    
    private RectTransform rectTransform;
    private Vector2 startPos;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            startPos = rectTransform.anchoredPosition;
        }
    }

    private void Update()
    {
        if (rectTransform != null)
        {
            // Utilise un Sinus mathématique pour un mouvement fluide sans aucun lien avec un autre script
            float newY = startPos.y + Mathf.Sin(Time.time * speed) * amplitude;
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, newY);
        }
    }
}