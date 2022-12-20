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

    [SerializeField] Camera gameCamera;

    [SerializeField] int boardWidth;
    
    [SerializeField] int boardHeight;

    [SerializeField] int numPlayers;

    [SerializeField] int numberOfContinents;

    [SerializeField] int maxContinentWidth;

    [SerializeField] int minContinentWidth;

    private Board board;

    private PieceFactory pieceFactory;

    private HexDelegates hexDelegates;

    [SerializeField] private UI uiManager;

    private Player[] players;

    private int turnIndex = -1;

    private Player activePlayer { get => players[turnIndex]; }

    private Player humanPlayer { get => players[0]; }

    private List<Hex> activePlayerAvailableHexes;

    #region Initialization

    private void Awake()
    {
        SetupHelperObjects();
        SetupBoard();
        SetupPlayers();
        SetupCamera();
    }

    private void SetupHelperObjects()
    {
        hexDelegates = new HexDelegates();
        hexDelegates.onClick += ValidateAndMakeMove;
        hexDelegates.playerSighted += MaybeMeetAndShowGreeting;
        pieceFactory = this.GetComponent<PieceFactory>();
    }
    private void SetupBoard()
    {
        board = GetComponent<Board>();
        board.Setup(hexDelegates); // just gives the board the delegate wrapper.
        // NOTE: A reasonable max width is Mathf.Min(boardWidth, boardHeight) / 3
        board.Generate(new MapSpecification(boardWidth, boardHeight, maxContinentWidth, minContinentWidth, numberOfContinents));
    }

    private void SetupPlayers()
    {
        players = new Player[numPlayers];
        for (int i = 0; i < numPlayers; i++)
        {
            players[i] = (i == 0) ? ScriptableObject.CreateInstance<Player>() : ScriptableObject.CreateInstance<AIPlayer>();
            players[i].Setup(i, numPlayers, UnityEngine.Random.ColorHSV());
            players[i].SetMainPiece((Unit) pieceFactory.Make(PieceType.WIZARD));
            board.PlaceToken(players[i].mainPiece, boardHeight / 2, i * (boardWidth / (numPlayers + 1)));
        }
        NextTurn();
    }

    private void SetupCamera()
    {
        gameCamera.transform.parent = players[0].mainPiece.transform;
        gameCamera.transform.localPosition = new Vector3(0, 10, -11);
    }
    #endregion

    #region Turns and Player Movement

    /// <summary>
    /// Returns true if it is the human player's turn.
    /// </summary>
    private bool IsHumanPlayerTurn()
    {
        return players[turnIndex] == humanPlayer;
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
            // Valid. Perform the action.
            if (activePlayer.interactionMode == InteractionMode.Move)
            {
                MoveTokenAction(row, col);
            }
            else
            {
                PlaceStructureAction(row, col);
            }

            // Move to next turn if no energy left, or remark possible moves.
            if (activePlayer.activePiece.remainingEnergy == 0)
            {
                NextTurn();
            }
            else if (activePlayer.activePiece.remainingEnergy < 0)
            {
                throw new Exception("Player ended round with less than zero energy.");
            }
            else
            {
                GetAndMarkAvailableMoves();
            }
        }
    }

    private void MoveTokenAction(int row, int col)
    {
        int cost = GetMinEnergyCostForMove(activePlayer.activePiece.currentHex, board.GetTileAt(row, col));

        activePlayer.activePiece.remainingEnergy -= cost;

        PlaceUnit(row, col, activePlayer.activePiece);
    }

    private void PlaceStructureAction(int row, int col)
    {
        // NOTE: Implementation still temporary. Probably 
        // shouldn't be in the manager anyway.
        int TEMP_PLACE_COST = 3;
        if (activePlayer.activePiece.remainingEnergy >= TEMP_PLACE_COST)
        {
            Token structure = pieceFactory.Make(PieceType.TOWER);
            activePlayer.AddPiece(structure);
            board.PlaceToken(structure, row, col);
            activePlayer.activePiece.remainingEnergy -= TEMP_PLACE_COST;
        }
        else
        {
            print("Not enough energy to place that structure.");
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
        board.VisionBlorp(activePlayer);
    }

    private bool IsTargetValidForPlayer(int row, int col, Player p)
    {
        // was the clicked hex a valid move for the current player?
        return p.mainPiece.currentHex == null || activePlayerAvailableHexes.Contains(board.GetTileAt(row, col));
    }

    private void NextTurn()
    {
        humanPlayer.ResetSeen();

        turnIndex = (turnIndex + 1) % players.Length;

        activePlayer.StartTurn(); // Sets up this unit for their turn.

        board.VisionBlorp(activePlayer);

        GetAndMarkAvailableMoves();

        if (!IsHumanPlayerTurn())
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
        if (IsHumanPlayerTurn())
        {
            NextTurn();
        }
    }

    private void GetAndMarkAvailableMoves()
    {
        activePlayerAvailableHexes = board.GetMovesBlorp(activePlayer);
        if (activePlayerAvailableHexes.Count == 0)
        {
            NextTurn(); // no moves found, pass to next
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
            throw new Exception("Tried to have the AI take a turn without setting its possible moves first.");
        }

        Hex destination = ((AIPlayer)activePlayer).makeMove(activePlayerAvailableHexes);
        PlaceUnit(destination.row, destination.column, activePlayer.activePiece);
        NextTurn();
    }

    #endregion

    #region UI Functionality

    /// <summary>
    /// Probably temporary until the UI functions
    /// have a better home.
    /// </summary>
    public void ChangePlayerInteractionMode()
    {
        humanPlayer.SwitchInteractionMode();
    }

    /// <summary>
    /// Checks if the activePlayer and 
    /// the seen player haven't met. If so,
    /// and one of them is the human player,
    /// shows the greeting dialogue.
    /// </summary>
    public void MaybeMeetAndShowGreeting(Player seen)
    {
        if (!activePlayer.HasMetPlayer(seen)) {
            activePlayer.MeetPlayer(seen);
            if (activePlayer.isHuman)
            {
                uiManager.ShowGreeting(seen);
            }
            else if (seen.isHuman)
            {
                uiManager.ShowGreeting(activePlayer);
            }
        }
    }

    #endregion

}
