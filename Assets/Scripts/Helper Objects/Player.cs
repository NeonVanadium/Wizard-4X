using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Describes a player. Will contain
/// things like resources, what board 
/// pieces belong to them, and a number 
/// of flags.
/// </summary>
public class Player : MonoBehaviour
{
    public InteractionMode interactionMode { get; set; } = InteractionMode.Move;
}

public enum InteractionMode
{
    Move = 0,
    Place = 1
}
