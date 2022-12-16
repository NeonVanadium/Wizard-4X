using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileType
{
    public string name { private set; get; }

    public Color color { private set; get; }

    public float height { private set; get; }

    public int cost { private set; get; } // the movement cost to pass this tile.

    public bool isLand { get => this != OCEAN; }

    public static TileType OCEAN { get; } = new TileType("Ocean", Color.blue, 0.1f, 2);

    public static TileType PLAINS { get; } = new TileType("Plains", Color.green, 0.2f, 5);

    public TileType(string name, Color color, float height, int cost = 1)
    {
        this.name = name;
        this.color = color;
        this.height = height;
        this.cost = cost;
    }
}
