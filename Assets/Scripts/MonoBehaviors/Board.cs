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

    private HexClickDelegateHandler clickDelegate;

    private int HexWidth; // the width of one individual hex.

    private Hex[][] board; // NOTE: As a Hex board, this array is jagged. Odd rows are width, evens are width - 1.

    #region Initialization

    public void Setup(HexClickDelegateHandler clickDelegate) {
        foreach (Transform child in hexPrefab.transform)
        {
            // there's for sure a better way to just get the first one

            HexWidth = (int)child.lossyScale.x;
            break;
        }
        this.clickDelegate = clickDelegate;
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

                board[row][col] = Instantiate(hexPrefab, new Vector3(col - offset, 0, row), Quaternion.identity);

                board[row][col].Init(clickDelegate);

                board[row][col].SetType(TileType.OCEAN);
            }

            rowIsOdd = !rowIsOdd;
        }
    }

    private void Terrainify()
    {
        // the width of the board in which exists a single continent
        int continentSpaceWidth = (int) (width / mapSpec.NUM_CONTINENTS);

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
    }

    public void DiscoveryBlorp(int row, int col, int range = 2)
    {
        board[row][col].Discover();
        if (range > 0)
        {
            foreach (Hex hex in GetAdjacentTiles(row, col))
            {
                DiscoveryBlorp(hex.row, hex.column, range - 1);
            }
        }
    }
    #endregion

    #region Board Accessors

    public List<Hex> GetMovesBlorp(int row, int col, int energy)
    {
        List<Hex> moves = new List<Hex>();

        Hex startHex = board[row][col];

        // cheeky way to stop the helper from marking the start tile:
        // add it before the recursion starts, and remove it before returning
        moves.Add(startHex);

        foreach (Hex hex in GetAdjacentTiles(row, col))
        {
            if (hex.discovered)
                GetMovesBlorpHelper(hex.row, hex.column, energy, moves);
        }


        moves.Remove(startHex);

        return moves;
    }

    public List<Hex> GetMovesBlorp(Unit u)
    {
        return GetMovesBlorp(u.currentHex.row, u.currentHex.column, u.remainingEnergy);
    }

    private void GetMovesBlorpHelper(int row, int col, int energy, List<Hex> moves)
    {
        
        energy -= board[row][col].tileType.cost;

        if (energy >= 0)
            moves.Add(board[row][col]);

        if (energy > 0)
            foreach (Hex hex in GetAdjacentTiles(row, col))
            {
                if (!moves.Contains(hex) && hex.discovered)
                    GetMovesBlorpHelper(hex.row, hex.column, energy, moves);
            }
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
