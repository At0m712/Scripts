using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class GestionTailleBoutons : MonoBehaviour
{
    private RectTransform rectTransform;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        AdjustSizeForScreen();
    }

    private void AdjustSizeForScreen()
    {
        // Calcul du ratio de l'écran du joueur
        float aspectRatio = (float)Screen.height / Screen.width;
        
        // Si l'écran est très allongé (ex: iPhone modernes, Samsung Galaxy S, ratio > 2.0)
        // On réduit légèrement la taille de cet élément pour éviter qu'il ne s'écrase sur les bords
        if (aspectRatio > 2f)
        {
            rectTransform.localScale = new Vector3(0.9f, 0.9f, 1f);
        }
    }
}