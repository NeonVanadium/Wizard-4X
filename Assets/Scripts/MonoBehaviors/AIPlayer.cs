using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayer
{
    /// <summary>
    /// Presumably temporary. Takes in a list of valid hexes,
    /// then selects one as its move.
    /// </summary>
    /// <param name="options"></param>
    /// <returns>The hex chosen as its destination.</returns>
    public static Hex makeMove(List<Hex> options) 
        // temporarily static
    {
        int choice = Random.Range(0, options.Count);

        return options[choice];
    }
}
