using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a moveable unit, such as the player characters.
/// 
/// Theoretically other minions may exist later.
/// </summary>
public class Unit : Token {

    public int maxEnergy { get => GetMaxEnergy(); }

    public int remainingEnergy { get; set; }

    private int GetMaxEnergy()
    {
        return 5;
    }

    public void StartTurn()
    {
        remainingEnergy = maxEnergy;
    }

}
