using UnityEngine;

// La liste des types de bonus possibles pour tes cartes
public enum TypeBonusCarte
{
    MultiplicateurProduction,
    ReductionCout,
    NiveauxDepart
}

// Permet de créer les cartes facilement via un Clic Droit > IdleTower > Carte Sorcier dans tes dossiers
[CreateAssetMenu(fileName = "Nouvelle Carte", menuName = "IdleTower/Carte Sorcier")]
public class CarteDef : ScriptableObject
{
    [Header("Informations Générales")]
    public string idUnique = "carte_001"; // Doit être unique pour la sauvegarde (ex: "baguette_apprenti")
    public string nomCarte = "Nom de la carte";
    public Sprite icone;
    
    [Header("Prix")]
    public double prixMana = 1000;

    [Header("Effet de la Carte")]
    public FloorData etageCible; // L'étage affecté par la carte
    public TypeBonusCarte typeBonus;
    
    [Tooltip("Ex: 2 pour Production x2. 0.1 pour Coût -10%. 5 pour +5 Niveaux de départ.")]
    public float valeurBonus = 2f; 
}