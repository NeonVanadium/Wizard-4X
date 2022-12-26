using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Board : MonoBehaviour
{

    [SerializeField] private Hex hexPrefab;

    private int width { get => mapSpec.MAP_WIDTH; }

    private int height { get => mapSpec.MAP_HEIGHT; }

    private MapSpecification mapSpec;

    private HexDelegates hexDelegates;

    private OutlineManager outlineManager; // manages the highlights and outlines of all hexes

    private int HexWidth; // the width of one individual hex.

    private Hex[][] board; // NOTE: As a Hex board, this array is jagged. Odd rows are width, evens are width - 1.

    #region Initialization

    public void Setup(HexDelegates hexDelegates) {
        outlineManager = GetComponentInParent<OutlineManager>();
        foreach (Transform child in hexPrefab.transform)
        {
            // there's for sure a better way to just get the first one

            HexWidth = (int)child.lossyScale.x;
            break;
        }
        this.hexDelegates = hexDelegates;
    }

    private void validateParameters()
    {
        if (width < 2) {
            throw new Exception("width must be at least 2.");
        }
        if (height < 1)
        {
            throw new Exception("height must be at least 1");
        }
    }

    /// <summary>
    /// Generates a board.
    /// </summary>

    #endregion

    #region Map Generation
    public void Generate(MapSpecification spec)
    {
        this.mapSpec = spec;
        validateParameters();
        board = new Hex[height][];
        InitializeTiles();
        Terrainify();
    }

    private void InitializeTiles()
    {
        bool rowIsOdd = false;

        // initialize all tiles
        for (int row = 0; row < height; row++)
        {
            int thisRowWidth = (rowIsOdd) ? width : width - 1; // even rows are one shorter
            board[row] = new Hex[thisRowWidth];

            for (int col = 0; col < thisRowWidth; col++)
            {
                float offset = (rowIsOdd) ? (0.5f * HexWidth) : 0; // for odd rows, offset by 1/2 of the tile width

                board[row][col] = Instantiate(hexPrefab, new Vector3(col - offset, 0, row * HexWidth), Quaternion.identity);

                board[row][col].Init(hexDelegates);
            }

            rowIsOdd = !rowIsOdd;
        }
    }

    private void Terrainify()
    {
        // the width of the board in which exists a single continent
        int continentSpaceWidth = (int)(width / mapSpec.NUM_CONTINENTS);

        for (int i = 0; i < mapSpec.NUM_CONTINENTS; i++)
        {
            ContinentBlorp(height / 2, (continentSpaceWidth * i) + (continentSpaceWidth / 2));
        }
    }

    private void ContinentBlorp(int row, int col, int cur = 0)
    {
        board[row][col].SetType(TileType.PLAINS);

        // rolls a number between the min and max distance from the start.
        int stopRoll = (int)UnityEngine.Random.Range(mapSpec.MIN_CONTINENT_WIDTH, mapSpec.MAX_CONTINENT_WIDTH);

        // if the number we rolled is bigger than cur, we keep going. Otherwise we stop.
        // this gives the generation an increasingly high chance of stopping
        // each iteration.
        if (stopRoll >= cur) {
            foreach (Hex hex in GetAdjacentTiles(row, col))
            {
                if (!hex.isLand) ContinentBlorp(hex.row, hex.column, cur + 1);
            }
        }
    }
    #endregion

    #region Things that do stuff to Hexes

    public void PlaceToken(Token t, int row, int col)
    {
        this.GetTileAt(row, col).SetObject(t);
        if (!(t is Unit)) { // if structure. Temp
            Hex curHex = GetTileAt(row, col);
            outlineManager.MarkTerritoryForPlayer(curHex, t.owner);
            foreach (Hex neighbor in GetAdjacentTiles(curHex))
            {
                outlineManager.MarkTerritoryForPlayer(neighbor, t.owner);
            }
        }
    }

    public void VisionBlorp(Player p)
    {
        Hex startHex;

        foreach (Token t in p.GetTokens())
        {
            startHex = t.currentHex;
            VisionBlorpHelper(startHex.row, startHex.column, t.sight, p);
        }
    }

    private void VisionBlorpHelper(int row, int col, int range, Player p)
    {
        Hex curHex = GetTileAt(row, col);
        //p.DiscoverHex(curHex);
        p.SeeHex(curHex);

        range -= 1;

        if (range >= 0)
        {
            foreach (Hex hex in GetAdjacentTiles(curHex))
            {
                VisionBlorpHelper(hex.row, hex.column, range, p);
            }
        }

    }
    #endregion

    #region Board Accessors

    public List<Hex> GetMovesBlorp(int row, int col, int energy, Player p)
    {
        outlineManager.ClearMoveMarkers();
        List<Hex> moves = new List<Hex>();

        Hex startHex = board[row][col];

        // cheeky way to stop the helper from marking the start tile:
        // add it before the recursion starts, and remove it before returning
        moves.Add(startHex);

        foreach (Hex hex in GetAdjacentTiles(row, col))
        {
            if (p.HasDiscoveredHex(hex))
                GetMovesBlorpHelper(hex.row, hex.column, energy, moves, p);
        }


        moves.Remove(startHex);

        return moves;

        //blorp(row, col, energy, (Hex h, int i) => true, (Hex h) => -1 * h.tileType.cost);
    }

    public List<Hex> GetMovesBlorp(Player p)
    {
        outlineManager.ClearMoveMarkers();
        Unit u = p.activePiece;
        return GetMovesBlorp(u.currentHex.row, u.currentHex.column, u.remainingEnergy, p);
    }

    private void GetMovesBlorpHelper(int row, int col, int energy, List<Hex> moves, Player p)
    {

        Hex curHex = GetTileAt(row, col);

        energy -= curHex.tileType.cost;

        if (energy >= 0)
        {
            moves.Add(curHex);
            if (p.isHuman)
            {
                outlineManager.MarkMoveForPlayer(curHex, p);
            }
        }

        if (energy > 0)
            foreach (Hex hex in GetAdjacentTiles(row, col))
            {
                if (!moves.Contains(hex) && p.HasDiscoveredHex(hex))
                    GetMovesBlorpHelper(hex.row, hex.column, energy, moves, p);
            }
    }

    public HashSet<Hex> blorp(int row, int col, int i, Func<Hex, int, bool> condition, Func<Hex, int> deltaI, HashSet<Hex> hexes = null)
    {
        if (hexes == null) {
            hexes = new HashSet<Hex>();
        }

        Hex curHex = GetTileAt(row, col);
        hexes.Add(curHex);
        i += deltaI(curHex);

        foreach (Hex hex in GetAdjacentTiles(row, col))
        {
            if (condition(hex, i))
            {
                blorp(hex.row, hex.column, i, condition, deltaI, hexes);
            }
        }

        return hexes;
    }



    /// <param name="row">The row in the board</param>
    /// <param name="col">The column in the board</param>
    /// <returns>A list of Hexes adjacent to the point described by the arguments</returns>
    public List<Hex> GetAdjacentTiles(int row, int col)
    {
        List<Hex> result = new List<Hex>();
        bool rowIsEven = row % 2 == 0;


        AddIfInbounds(row + 1, col);
        AddIfInbounds(row + 1, (rowIsEven) ? col + 1 : col - 1);

        AddIfInbounds(row, col - 1);
        AddIfInbounds(row, col + 1);

        AddIfInbounds(row - 1, col);
        AddIfInbounds(row - 1, (rowIsEven) ? col + 1 : col - 1);
        
        return result;

        void AddIfInbounds(int r, int c)
        {
            if (CoordinateInBounds(r, c)) result.Add(board[r][c]);
        }
    }

    /// <summary>
    /// Returns a list of hexes adjacent in the board to the provided hex.
    /// </summary>
    public List<Hex> GetAdjacentTiles(Hex hex)
    {
        return GetAdjacentTiles(hex.row, hex.column);
    }

    public Hex GetTileAt(int row, int col)
    {
        return CoordinateInBounds(row, col) ? board[row][col] : throw new Exception($"{row}, {col} is out of bounds.");
    }

    /// <summary>
    /// Returns True iff the provided row, col pair is in-bounds on the current game board.
    /// </summary>
    private bool CoordinateInBounds(int row, int col)
    {
        return !(row < 0 || col < 0 || row >= board.Length || col >= board[row].Length);
    }
    #endregion
}
