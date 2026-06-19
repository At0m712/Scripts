using UnityEngine;

public class AutoDesactivateur : MonoBehaviour
{
    public float dureeDeVie = 1.5f; // Disparaît après 1.5 seconde

    void OnEnable()
    {
        Invoke("DesactiverObjet", dureeDeVie);
    }

    void DesactiverObjet()
    {
        gameObject.SetActive(false);
    }
    
    void OnDisable()
    {
        CancelInvoke();
    }
}