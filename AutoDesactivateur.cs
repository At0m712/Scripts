using UnityEngine;

public class AutoDesactivateur : MonoBehaviour
{
    public float timeToDisable = 1.5f;

    private void OnEnable()
    {
        Invoke(nameof(DisableObject), timeToDisable);
    }

    private void DisableObject()
    {
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }
}