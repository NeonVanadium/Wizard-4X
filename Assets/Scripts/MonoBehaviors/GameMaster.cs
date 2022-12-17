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

    [SerializeField] Player playerPrefab;

    [SerializeField] Camera gameCamera;

    [SerializeField] int boardWidth;
    
    [SerializeField] int boardHeight;

    [SerializeField] int numPlayers;

    [SerializeField] int numberOfContinents;

    [SerializeField] int maxContinentWidth;

    [SerializeField] int minContinentWidth;

    private OutlineManager outlineManager;

    private PieceFactory pieceFactory;

    private HexClickDelegateHandler clickHandler;

    private Player[] players;

    private int turnIndex = -1;

    private Player activePlayer { get => players[turnIndex]; }

    private Player humanPlayer { get => players[0]; }

    private List<Hex> activePlayerAvailableHexes;

    private Board board;

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
        clickHandler = new HexClickDelegateHandler();
        clickHandler.onClick += ValidateAndMakeMove;
        outlineManager = this.GetComponent<OutlineManager>();
        pieceFactory = this.GetComponent<PieceFactory>();
    }
    private void SetupBoard()
    {
        board = Instantiate(boardPrefab, Vector3.zero, Quaternion.identity);
        board.Setup(clickHandler); // just gives the board the click handler
        // NOTE: A reasonable max width is Mathf.Min(boardWidth, boardHeight) / 3
        board.Generate(new MapSpecification(boardWidth, boardHeight, maxContinentWidth, minContinentWidth, numberOfContinents));
    }

    private void SetupPlayers()
    {
        players = new Player[numPlayers];
        for (int i = 0; i < numPlayers; i++)
        {
            players[i] = ScriptableObject.CreateInstance<Player>();
            players[i].color = UnityEngine.Random.ColorHSV();
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
            Unit activePiece = activePlayer.activePiece;

            if (activePlayer.interactionMode == InteractionMode.Move)
            {
                int cost = GetMinEnergyCostForMove(
                board.GetTileAt(activePiece.currentHex.row, activePiece.currentHex.column),
                board.GetTileAt(row, col));

                activePiece.remainingEnergy -= cost;


                PlaceUnit(row, col, activePiece);
            }
            else
            {
                int TEMP_PLACE_COST = 3;
                if (activePiece.remainingEnergy >= TEMP_PLACE_COST)
                {
                    board.PlaceToken(pieceFactory.Make(PieceType.TOWER), row, col);
                    foreach (Hex h in board.GetAdjacentTiles(row, col))
                    {
                        h.SetObject(outlineManager.CreateOutline(activePlayer.color), 0.5f);
                    }
                    activePiece.remainingEnergy -= TEMP_PLACE_COST; // TEMP for testing.
                }
                else
                {
                    print("Not enough energy to place that structure.");
                }
                
            }
            

            if (activePiece.remainingEnergy == 0)
            {
                NextTurn();
            }
            else if (activePiece.remainingEnergy < 0)
            {
                throw new Exception("Player ended round with less than zero energy.");
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
        board.DiscoveryBlorp(row, col, u.sight);
    }

    private bool IsTargetValidForPlayer(int row, int col, Player p)
    {
        // was the clicked hex a valid move for the current player?
        return p.mainPiece.currentHex == null || board.GetMovesBlorp(p.mainPiece).Contains(board.GetTileAt(row, col));
    }

    private void NextTurn()
    {
        turnIndex = (turnIndex + 1) % players.Length;

        // should be removed once tiles aren't disabled but filtered before discovery
        board.DiscoveryBlorp(activePlayer.mainPiece.currentHex.row, activePlayer.mainPiece.currentHex.column);

        activePlayer.StartTurn(); // Sets up this unit for their turn.
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
        if (activePlayerAvailableHexes != null)
        {
            outlineManager.ClearMoveMarkers();
        }
        activePlayerAvailableHexes = board.GetMovesBlorp(activePlayer.activePiece);
        if (activePlayerAvailableHexes.Count == 0)
        {
            NextTurn(); // no moves found, pass to next
        }
        else if (IsHumanPlayerTurn())
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

    #endregion

}
