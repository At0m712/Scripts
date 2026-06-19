using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System;

public class MultiplicateurUI : MonoBehaviour
{
    public TextMeshProUGUI rewardText;
    private int crystalsToGet = 0;
    [Header("Effets Visuels")]
    public ParticleSystem prestigeParticles;

    void OnEnable()
    {
        if (GameManager.Instance == null) return;

        // Racine cubique de (Mana Total / 1 Million)
        double rawCrystals = Math.Pow(GameManager.Instance.manaTotalProduced / 1000000.0, 1.0 / 3.0);
        crystalsToGet = (int)Math.Floor(rawCrystals);

        rewardText.text = "+ " + crystalsToGet + " Cristaux";
    }

    public void TriggerPrestige()
    {
        if (crystalsToGet <= 0) return;

        // On sauvegarde l'argent premium et les arbres
        int totalCrystals = GameManager.Instance.temporalCrystals + crystalsToGet;
        PlayerPrefs.SetInt("temporalCrystals", totalCrystals);

        // On nettoie la progression de la tour
        PlayerPrefs.DeleteKey("manaCurrent");
        PlayerPrefs.DeleteKey("manaTotalProduced");
        // Optionnel : PlayerPrefs.DeleteAll() si on veut TOUT reset sauf les cristaux.

        // EFFET VISUEL COMPLÉTÉ
        if (prestigeParticles != null)
        {
            prestigeParticles.Play();
        }

        // On attend 1.5 secondes pour laisser l'explosion se jouer, puis on recharge
        Invoke("ReloadScene", 1.5f);
    }

    private void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}