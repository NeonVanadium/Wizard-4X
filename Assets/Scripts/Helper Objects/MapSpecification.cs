using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class that holds parameters for board generation.
/// </summary>
public class MapSpecification
{
    public int MAP_WIDTH { get; private set; }
    public int MAP_HEIGHT { get; private set; }
    public int MAX_CONTINENT_WIDTH { get; private set; }
    public int MIN_CONTINENT_WIDTH { get; private set; }
    public int NUM_CONTINENTS { get; private set; }

    public MapSpecification(int MAP_WIDTH, int MAP_HEIGHT, int MAX_CONTINENT_WIDTH, int MIN_CONTINENT_WIDTH, int NUM_CONTINENTS)
    {
        this.MAP_WIDTH = MAP_WIDTH;
        this.MAP_HEIGHT = MAP_HEIGHT;
        this.MAX_CONTINENT_WIDTH = MAX_CONTINENT_WIDTH;
        this.MIN_CONTINENT_WIDTH = MIN_CONTINENT_WIDTH;
        this.NUM_CONTINENTS = NUM_CONTINENTS;
    }
}
