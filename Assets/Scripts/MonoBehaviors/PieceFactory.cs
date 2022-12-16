using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class that holds references to the prefabs
/// of game piece tokens (structures, units, etc).
/// 
/// Has methods to create said tokens.
/// 
/// Serves to declutter the game master by 
/// moving prefabs and such to one class
/// focused solely on their creation.
/// </summary>
public class PieceFactory : MonoBehaviour
{
    [SerializeField] Unit wizardPrefab;

    [SerializeField] Token towerPrefab;

    public Token Make(PieceType type)
    {
        switch (type)
        {
            case PieceType.WIZARD:
                return Instantiate(wizardPrefab);
            case PieceType.TOWER:
                return Instantiate(towerPrefab);
            default:
                throw new System.Exception("Attempted to create a piece from a type that does not exist.");
        }

    }
}

public enum PieceType
{
    WIZARD, TOWER
}
