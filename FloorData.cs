using UnityEngine;

[CreateAssetMenu(fileName = "NouvelEtage", menuName = "Tour/Donnees Etage")]
public class FloorData : ScriptableObject
{
    [Header("Paramètres de base")]
    [Tooltip("Le prix du niveau 1")]
    public double baseCost = 10;
    
    [Tooltip("La production donnée par 1 niveau")]
    public double baseProduction = 1;
    
    [Header("Évolution (Important)")]
    [Tooltip("Multiplicateur de prix à chaque niveau (ex: 1.15 pour +15% à chaque achat)")]
    public double costMultiplier = 1.15;
}