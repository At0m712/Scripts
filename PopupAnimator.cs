using UnityEngine;

public class PopupAnimator : MonoBehaviour
{
    public float animationSpeed = 10f;
    private Vector3 targetScale = Vector3.one;

    private void OnEnable()
    {
        // À chaque fois que le popup est activé, il part de 0
        transform.localScale = Vector3.zero;
    }

    private void Update()
    {
        // Agrandit le popup jusqu'à sa taille normale de manière fluide
        if (transform.localScale != targetScale)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
        }
    }
    
    public void ClosePopup()
    {
        gameObject.SetActive(false);
    }
}