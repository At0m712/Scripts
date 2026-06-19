using UnityEngine;

[CreateAssetMenu(fileName = "NouvelEtage", menuName = "IdleTower/Donnees Etage")]
public class FloorData : ScriptableObject
{
    public string floorName;
    public double baseCost;
    public float baseCostMultiplier = 1.15f;
    public double baseProduction;
    public Sprite iconEtage;
}
