using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System;

public class MultiplicateurUI : MonoBehaviour
{
    public TextMeshProUGUI rewardText;
    private int crystalsToGet = 0;

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
        // Attention : il faut aussi effacer les clés "Tour_..." ici ou via un script dédié.

        // Redémarre le jeu à zéro
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}