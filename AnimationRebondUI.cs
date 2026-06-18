using UnityEngine;

public class AnimationRebondUI : MonoBehaviour
{
    public float scaleSpeed = 5f;
    public float scaleAmount = 0.05f;

    private Vector3 startScale;

    private void Start()
    {
        startScale = transform.localScale;
    }

    private void Update()
    {
        // Crée un effet de battement de coeur doux et continu
        float bounce = Mathf.Sin(Time.time * scaleSpeed) * scaleAmount;
        transform.localScale = startScale + new Vector3(bounce, bounce, 0);
    }
}