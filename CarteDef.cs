using UnityEngine;

public enum TypeBonusCarte
{
    MultiplicateurProduction,
    ReductionCout,
    NiveauDepart
}

[CreateAssetMenu(fileName = "Nouvelle Carte", menuName = "IdleTower/Carte Sorcier")]
public class CarteDef : ScriptableObject
{
    [Header("Infos de la Carte")]
    public string idUnique; // IMPORTANT : Ne jamais changer une fois créé !
    public string nomCarte;
    [TextArea] public string descriptionCarte;
    public Sprite iconeCarte;

    [Header("Achat")]
    public int coutCristaux;

    [Header("Effet de la Carte")]
    public FloorData etageCible; // Si c'est vide, s'applique à tous les étages
    public TypeBonusCarte typeBonus;
    
    [Tooltip("EXEMPLES IMPORTANTS :\n- Mettre 2 pour une Production x2.\n- Mettre 0.9 pour une réduction de Coût de -10%.\n- Mettre 5 pour +5 Niveaux de départ.")]
    public float valeurBonus = 2f; 
}