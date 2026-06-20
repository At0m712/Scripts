using UnityEngine;

[CreateAssetMenu(fileName = "NouvelEtage", menuName = "Tour/Donnees Etage")]
public class FloorData : ScriptableObject
{
    [Header("Paramètres de base")]
    public double baseCost = 10;
    public double baseProduction = 1;
    public double costMultiplier = 1.15;

    [Header("Temps de Production")]
    [Tooltip("Temps en secondes avant de générer l'argent (ex: 1, 3, 7200 pour 2h)")]
    public float baseProductionTime = 1f;
}