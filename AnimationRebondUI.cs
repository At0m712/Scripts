using UnityEngine;

public class AnimationRebondUI : MonoBehaviour
{
    [Header("Paramètres de Rebond")]
    public float vitesse = 5f;
    public float echelleMax = 1.1f;
    public float echelleMin = 0.9f;
    
    // OPTIMISATION : Mise en cache
    private RectTransform rectTransform;
    private Vector3 echelleInitiale;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        if (rectTransform != null) echelleInitiale = rectTransform.localScale;
    }

    void Update()
    {
        if (rectTransform == null) return;

        // Création d'une boucle mathématique fluide sans Instantiate ni calculs lourds
        float progression = Mathf.PingPong(Time.unscaledTime * vitesse, 1f);
        float scaleActuel = Mathf.Lerp(echelleMin, echelleMax, progression);
        
        rectTransform.localScale = echelleInitiale * scaleActuel;
    }
}