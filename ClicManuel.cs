using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ClicManuel : MonoBehaviour, IPointerDownHandler
{
    [Header("Paramètres de clic")]
    public double baseClickPower = 1;

    public void OnPointerDown(PointerEventData eventData)
    {
        // 1. Calcul du gain
        double gain = baseClickPower * GameManager.Instance.globalMultiplier;
        
        // 2. Ajout au GameManager
        GameManager.Instance.AddMana(gain);

        // 3. Effet Sonore
        if (AudioManager.Instance != null && AudioManager.Instance.clickSound != null)
        {
            AudioManager.Instance.sfxSource.pitch = Random.Range(0.95f, 1.05f);
            AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.clickSound);
        }

        // 4. Effet Visuel (Le texte flottant +X)
        if (ObjectPooler.Instance != null)
        {
            Vector3 randomOffset = new Vector3(Random.Range(-50f, 50f), Random.Range(-50f, 50f), 0);
            
            // CORRECTION : On utilise eventData.position au lieu de l'ancien Input.mousePosition
            Vector3 spawnPosition = new Vector3(eventData.position.x, eventData.position.y, 0) + randomOffset; 
            
            GameObject popup = ObjectPooler.Instance.SpawnFromPool(spawnPosition);
            if (popup != null)
            {
                TextMeshProUGUI textComp = popup.GetComponentInChildren<TextMeshProUGUI>();
                if (textComp != null)
                {
                    textComp.text = "+ " + ScoreUI.FormatNumber(gain);
                    textComp.color = new Color(0.2f, 0.8f, 1f); 
                }
            }
        }
    }
}