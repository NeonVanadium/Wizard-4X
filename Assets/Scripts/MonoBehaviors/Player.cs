using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Describes a player. Will contain
/// things like resources, what board 
/// pieces belong to them, and a number 
/// of flags.
/// </summary>
public class Player : ScriptableObject
{
    public InteractionMode interactionMode { get; set; } = InteractionMode.Move;

    public List<Token> pieces = new List<Token>();

    public Unit mainPiece { get; private set; }

    public Unit activePiece { get => mainPiece; } // temporary, at some point players will have multiple pieces

    public void SwitchInteractionMode()
    {
        if (interactionMode == InteractionMode.Move) {
            interactionMode = InteractionMode.Place;
        }
        else
        {
            interactionMode = InteractionMode.Move;
        }
        Debug.Log($"Interaction mode changed to {interactionMode}");
    }

    public void SetMainPiece(Unit unit)
    {
        this.mainPiece = unit;
        this.pieces.Add(unit);
    }

    public void StartTurn()
    {
        foreach (Token piece in pieces)
        {
            if (piece is Unit)
            {
                ((Unit)piece).StartTurn();
            }
        }
    }
}

public enum InteractionMode
{
    Move = 0,
    Place = 1
}
