using UnityEngine;

public class AutoDesactivateur : MonoBehaviour
{
    [Header("Configuration")]
    public float tempsAvantDesactivation = 2f;
    private float chrono;

    void OnEnable()
    {
        // On réinitialise le chrono à chaque fois que l'objet sort du pool
        chrono = tempsAvantDesactivation;
    }

    void Update()
    {
        chrono -= Time.deltaTime;
        if (chrono <= 0)
        {
            // OPTIMISATION : On désactive l'objet pour qu'il retourne dans le Pool, on NE LE DÉTRUIT PAS.
            gameObject.SetActive(false);
        }
    }
}