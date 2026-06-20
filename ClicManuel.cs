using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System; // Obligatoire pour utiliser les formules mathématiques comme Math.Floor

public class ClicManuel : MonoBehaviour, IPointerDownHandler
{
    [Header("Paramètres de clic")]
    public double baseClickPower = 1;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (GameManager.Instance == null) return;

        // 1. Calcul de la puissance du clic
        double gain = baseClickPower * GameManager.Instance.globalMultiplier;
        GameManager.Instance.AddMana(gain);

        // 2. Audio (Avec précision UnityEngine.Random pour éviter les conflits avec System)
        if (AudioManager.Instance != null && AudioManager.Instance.clickSound != null)
        {
            AudioManager.Instance.sfxSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
            AudioManager.Instance.sfxSource.PlayOneShot(AudioManager.Instance.clickSound);
        }

        // 3. Feedback Visuel
        if (ObjectPooler.Instance != null)
        {
            Vector3 randomOffset = new Vector3(UnityEngine.Random.Range(-40f, 40f), UnityEngine.Random.Range(-40f, 40f), 0);
            Vector3 spawnPos = new Vector3(eventData.position.x, eventData.position.y, 0) + randomOffset; 
            
            GameObject popupText = ObjectPooler.Instance.SpawnFromPool(spawnPos);
            if (popupText != null)
            {
                TextMeshProUGUI textComp = popupText.GetComponentInChildren<TextMeshProUGUI>();
                if (textComp != null)
                {
                    textComp.text = "+ " + Math.Floor(gain).ToString();
                    textComp.color = new Color(0.2f, 0.8f, 1f); 
                }
            }
        }
    }
}