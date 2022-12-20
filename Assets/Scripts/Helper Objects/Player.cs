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
    public int id { get; private set; }

    public InteractionMode interactionMode { get; set; } = InteractionMode.Move;

    public List<Token> pieces = new List<Token>(); // The pieces this player owns.

    public Unit mainPiece { get; private set; } // The player's main piece, "them" on the board.

    public Unit activePiece { get => mainPiece; } // temporary, at some point players will have multiple pieces

    public Color color { get; set; } // this player's identifying color, applied to its borders, etc

    private HashSet<Hex> discoveredHexes = new HashSet<Hex>(); // the hexes the player is aware of

    private HashSet<Hex> seenHexes = new HashSet<Hex>(); // the hexes the player can actively see

    public bool[] metPlayers;

    public bool isHuman { get => !(this is AIPlayer); }

    private Language language = new Language(); // will be part of a culture later, in theory.

    public void Setup(int myID, int numPlayers, Color color)
    {
        this.id = myID;
        metPlayers = new bool[numPlayers];
        metPlayers[id] = true;
        this.color = color;
        this.name = language.GenerateName();
        
    }

    public void MeetPlayer(Player other)
    {
        if (!metPlayers[other.id]) {
            metPlayers[other.id] = true;
            other.MeetPlayer(this);
        }
    }

    public bool HasMetPlayer(Player other)
    {
        return metPlayers[other.id];
    }
    
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
        mainPiece = unit;
        AddPiece(unit);
    }

    public void AddPiece(Token t)
    {
        t.AssignTo(this);
        pieces.Add(t);
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

    public bool HasDiscoveredHex(Hex hex)
    {
        return discoveredHexes.Contains(hex);
    }

    /// <summary>
    /// Adds the given token to this
    /// player's current sight.
    /// </summary>
    public void SeeHex(Hex hex)
    {
        discoveredHexes.Add(hex);
        seenHexes.Add(hex);
        if (isHuman)
        {
            hex.SetInPlayerSight(true);
        }
    }

    public bool CanSee(Hex hex)
    {
        return seenHexes.Contains(hex);
    }

    /// <summary>
    /// Clears this player's seen tiles.
    /// Should be called at the end of their turn.
    /// </summary>
    public void ResetSeen()
    {
        if (isHuman)
            foreach (Hex hex in seenHexes)
                hex.SetInPlayerSight(false);
        seenHexes.Clear();
    }
}

public enum InteractionMode
{
    Move = 0,
    Place = 1
}
