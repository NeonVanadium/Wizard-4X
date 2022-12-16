using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class wraps a delegate that handles move processing when the player selects a hex.
/// 
/// Created by the GameMaster and passed down through the board to the hexes.
/// </summary>
public class HexClickDelegateHandler
{
    public delegate void HexClickDelegate(int row, int col);

    public HexClickDelegate onClick;
}
