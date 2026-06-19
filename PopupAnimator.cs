using UnityEngine;

public class PopupAnimator : MonoBehaviour
{
    [Header("Paramètres d'Animation")]
    public float animationSpeed = 10f;
    
    private Vector3 targetScale = Vector3.zero;

    void Awake()
    {
        // Au démarrage, on s'assure que le popup est invisible (taille 0)
        transform.localScale = Vector3.zero;
    }

    void Update()
    {
        // L'animation fluide de la taille vers la cible (0 ou 1)
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
    }

    // --- FONCTIONS À APPELER DEPUIS LES BOUTONS ---

    public void Ouvrir()
    {
        // On active l'objet et on lui dit de grandir jusqu'à sa taille normale (1)
        gameObject.SetActive(true);
        targetScale = Vector3.one;
    }

    public void Fermer()
    {
        // On lui dit de rétrécir jusqu'à 0
        targetScale = Vector3.zero;
        
        // On attend une fraction de seconde pour que l'animation se termine, puis on désactive l'objet
        Invoke("DesactiverTotalement", 0.3f);
    }

    private void DesactiverTotalement()
    {
        // Sécurité : on s'assure qu'on ne désactive pas l'objet si le joueur a cliqué sur "Ouvrir" entre-temps
        if (targetScale == Vector3.zero)
        {
            gameObject.SetActive(false);
        }
    }
}