using UnityEngine;
using UnityEngine.UI;

public class AnimationChevrons : MonoBehaviour
{
    [Header("Configuration")]
    public RawImage imageChevrons;
    
    [Tooltip("Vitesse de défilement (Positif = vers la droite, Négatif = vers la gauche)")]
    public float vitesseDefilement = 1f; 

    void Update()
    {
        if (imageChevrons != null)
        {
            // On récupère le rectangle de texture actuel
            Rect rect = imageChevrons.uvRect;
            
            // On le fait glisser vers la gauche (ce qui donne l'illusion que l'image va vers la droite)
            rect.x -= vitesseDefilement * Time.deltaTime;
            
            // On réapplique le rectangle
            imageChevrons.uvRect = rect;
        }
    }
}