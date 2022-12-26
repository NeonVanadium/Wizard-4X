using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineManager : MonoBehaviour
{

    [SerializeField] Outline HexOutlinePrefab;

    private Color moveColor = Color.cyan; // color of the move markers
    private Color hostileColor = Color.red;

    private HashSet<Outline> activeMoveOutlines = new HashSet<Outline>();

    /// <summary>
    /// Creates an outline and returns it.
    /// Used to create hexes.
    /// </summary>
    private Outline CreateOutline()
    {
        return Instantiate(HexOutlinePrefab);
    }

    /// <summary>
    /// Turns the given hex's move outline on,
    /// coloring for the given player.
    /// 
    /// The player is relevant for determining 
    /// the outline color of tokens are present.
    /// 
    /// (Friendly, hostile, etc)
    /// </summary>
    /// <param name="hex">The hex for which to enable the outline.</param>
    /// <param name="p">The player for whom this is being toggled.</param>
    public void MarkMoveForPlayer(Hex hex, Player p)
    {
        // if the hex has no move outline, give it one.
        if (hex.moveOutline == null)
        {
            hex.moveOutline = CreateOutline();
            hex.SetObject(hex.moveOutline, 0.3f);
        }
        
        // is there a non-allied token on this hex?
        if (hex.HasTokenFromPlayerBesides(p))
        {
            hex.moveOutline.Show(Color.red);
        }
        else
        {
            hex.moveOutline.Show(moveColor);
        }
        
        activeMoveOutlines.Add(hex.moveOutline);
    }

    public void MarkTerritoryForPlayer(Hex hex, Player p)
    {
        // if the hex has no move outline, give it one.
        if (hex.territoryOutline == null)
        {
            hex.territoryOutline = CreateOutline();
            hex.SetObject(hex.territoryOutline, 0.2f);
        }

        hex.owner = p;
        hex.territoryOutline.Show(p.color);
    }

    /// <summary>
    /// Clears all of the move outlines in play.
    /// </summary>
    public void ClearMoveMarkers()
    {
        foreach (Outline o in activeMoveOutlines)
        {
            o.Hide();
        }
        activeMoveOutlines.Clear();
    }

}
