using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The high-level manager that runs the game.
/// 
/// Handles setup, taking turns, and moving pieces.
/// </summary>
public class GameMaster : MonoBehaviour
{
    [SerializeField] Board boardPrefab;

    [SerializeField] Unit unitPrefab;

    [SerializeField] Camera camera;

    [SerializeField] int boardWidth;
    
    [SerializeField] int boardHeight;

    [SerializeField] int numPlayers;

    [SerializeField] int numberOfContinents;

    private OutlineManager outlineManager;

    private HexClickDelegateHandler clickHandler;

    private Unit[] players;

    private int turnIndex = -1;

    private Unit activePlayer { get => players[turnIndex]; }

    private List<Hex> activePlayerAvailableHexes;

    private Board board;

    #region Initialization

    private void Awake()
    {
        clickHandler = new HexClickDelegateHandler();
        clickHandler.onClick += ValidateAndMakeMove;
        outlineManager = this.GetComponent<OutlineManager>();
        SetupBoard();
        SetupPlayers();
        SetupCamera();
    }

    private void SetupBoard()
    {
        board = Instantiate(boardPrefab, Vector3.zero, Quaternion.identity);
        board.Setup(clickHandler); // just gives the board the click handler
        board.Generate(new MapSpecification(boardWidth, boardHeight, Mathf.Min(boardWidth, boardHeight) / 3, 1, numberOfContinents));
    }

    private void SetupPlayers()
    {
        players = new Unit[numPlayers];
        for (int i = 0; i < numPlayers; i++)
        {
            players[i] = Instantiate(unitPrefab, Vector3.zero, Quaternion.identity);
            players[i].color = UnityEngine.Random.ColorHSV();
            board.GetTileAt(boardHeight / 2, i * (boardWidth / (numPlayers + 1))).SetObject(players[i]);
        }
        NextTurn();
    }

    private void SetupCamera()
    {
        camera.transform.parent = players[0].transform;
        camera.transform.localPosition = new Vector3(0, 10, -11);
    }

    private void Start()
    {
        //camera.transform.position = new Vector3(boardWidth / 2, 10, -boardHeight);
        clickHandler.onClick(boardHeight / 2, boardWidth / 2);
    }
    #endregion

    #region Turns and Player Movement

    /// <summary>
    /// Returns true if it is the human player's turn.
    /// </summary>
    private bool IsPlayerTurn()
    {
        return turnIndex == 0;
    }

    /// <summary>
    /// Called when a human player clicks on a hex.
    /// Checks if the hex at those coordinates is a 
    /// valid move for the active token, and if so, places
    /// them there.
    /// </summary>
    private void ValidateAndMakeMove(int row, int col)
    {
        
        if (IsTargetValidForPlayer(row, col, activePlayer)) 
        { 
            
            int cost = GetMinEnergyCostForMove(
                board.GetTileAt(activePlayer.currentHex.row, activePlayer.currentHex.column),
                board.GetTileAt(row, col));

            activePlayer.remainingEnergy -= cost;

            PlaceUnit(row, col, activePlayer);

            if (activePlayer.remainingEnergy <= 0)
            {
                if (activePlayer.remainingEnergy < 0)
                    print("Active player had energy less than zero at round end. Was this intentional?");
                NextTurn();
            }
            else
            {
                GetAndMarkAvailableMoves();
            }
        }
    }

    // TODO: Atrocious, fix
    private int GetMinEnergyCostForMove(Hex cur, Hex end, Dictionary<Hex, int> acc = null, int min = 0)
    {
        if (cur == end)
        {
            return min;
        }

        if (acc == null)
        {
            acc = new Dictionary<Hex, int>();
        }

        acc[cur] = min;

        List<int> possibleValues = new List<int>();

        foreach (Hex hex in board.GetAdjacentTiles(cur))
        {
            int newMin = min + hex.tileType.cost;
            if (activePlayerAvailableHexes.Contains(hex) && (!acc.ContainsKey(hex) || (acc.ContainsKey(hex) && acc[hex] > newMin)))
            {
                possibleValues.Add(GetMinEnergyCostForMove(hex, end, acc, newMin));
            }
        }

        if (possibleValues.Count == 0)
        {
            return Int32.MaxValue;
        }
        else
        {
            possibleValues.Sort();
            acc[cur] = possibleValues[0];
            return possibleValues[0];
        }
        
    }

    private void PlaceUnit(int row, int col, Unit u)
    {
        board.PlaceToken(u, row, col);
        board.DiscoveryBlorp(row, col, activePlayer.sight);
    }

    private bool IsTargetValidForPlayer(int row, int col, Unit u)
    {
        // was the clicked hex a valid move for the current player?
        return activePlayer.currentHex == null || board.GetMovesBlorp(u).Contains(board.GetTileAt(row, col));
    }

    private void NextTurn()
    {
        turnIndex = (turnIndex + 1) % players.Length;

        // should be removed once tiles aren't disabled but filtered before discovery
        board.DiscoveryBlorp(activePlayer.currentHex.row, activePlayer.currentHex.column);

        activePlayer.StartTurn(); // Sets up this unit for their turn.
        GetAndMarkAvailableMoves();

        if (!IsPlayerTurn())
        {
            HaveAITakeTurn();
        }
    }

    /// <summary>
    /// For when a human player
    /// passes their turn via 
    /// a button.
    /// </summary>
    public void ManualEndTurn()
    {
        if (IsPlayerTurn())
        {
            NextTurn();
        }
    }

    private void GetAndMarkAvailableMoves()
    {
        if (activePlayerAvailableHexes != null)
        {
            outlineManager.ClearAllOutlines();
        }
        activePlayerAvailableHexes = board.GetMovesBlorp(activePlayer);
        if (activePlayerAvailableHexes.Count == 0)
        {
            NextTurn(); // no moves found, pass to next
        }
        else if (IsPlayerTurn())
        {
            // only mark on the player's turn.
            foreach (Hex h in activePlayerAvailableHexes)
            {
                h.SetObject(outlineManager.CreateOutline(), 0.3f);
            }
        }
    }

    /// <summary>
    /// I'll give you three tries to guess what this method does.
    /// 
    /// Must be called after GetAndMarkAvailableMoves.
    /// </summary>
    private void HaveAITakeTurn()
    {
        if (activePlayerAvailableHexes == null)
        {
            throw new System.Exception("Tried to have the AI take a turn without setting its possible moves first.");
        }

        Hex destination = AIPlayer.makeMove(activePlayerAvailableHexes);
        PlaceUnit(destination.row, destination.column, activePlayer);
        NextTurn();
    }

    #endregion

}
