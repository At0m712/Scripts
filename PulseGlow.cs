using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class PulseGlow : MonoBehaviour
{
    public float pulseSpeed = 3f;
    public float minAlpha = 0.4f;
    public float maxAlpha = 1f;

    private CanvasGroup canvasGroup;

    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Update()
    {
        // Alterne l'opacité entre minAlpha et maxAlpha
        float pingPong = Mathf.PingPong(Time.time * pulseSpeed, 1f);
        canvasGroup.alpha = Mathf.Lerp(minAlpha, maxAlpha, pingPong);
    }
}