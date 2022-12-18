using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class represents a hex, also
/// called a Tile. It is one space on
/// the game board.
/// 
/// The hex knows its own position and 
/// holds references to the various 
/// objects that are "on" it.
/// 
/// The hex also serves as the point of control
/// for the visibility of all tokens on it.
/// </summary>
public class Hex : MonoBehaviour
{
    public int row { get => GetRow(); }
    public int column { get => GetColumn(); }

    public TileType tileType { get; private set; }

    private  List<Token> tokens = new List<Token>(); // the tokens on this tile.

    private bool inPlayerSight; // if this tile is actively visible by the human player. Used for rendering.

    public bool isLand { get => tileType.isLand; }

    private HexClickDelegateHandler clickDelegate;

    private Token moveOutline;

    #region Initialization
    void Awake()
    {
        this.name = this.ToString();
        Discover(false);
    }

    public void Init(HexClickDelegateHandler clickDelegate)
    {
        this.clickDelegate = clickDelegate;
    }

    public void SetType(TileType tileType)
    {
        this.tileType = tileType;
        transform.localScale = new Vector3(1, tileType.height, 1);
        SetColor();
    }
    #endregion

    #region State Control

    /// <summary>
    /// Turns the tile's renderer on. Should
    /// only be called when a human player sees the tile.
    /// </summary>
    /// <param name="val"></param>
    private void Discover(bool val = true)
    {
        Show(val);
    }

    /// <summary>
    /// Places the given game object "on" this tile in the gamespace.
    /// </summary>
    /// <param name="obj">The game object to place.</param>
    /// <param name="y">Optional, the y-value to set the object.</param>
    public void SetObject(MonoBehaviour obj, float y = 1.0f)
    {
        obj.transform.position = new Vector3(this.transform.position.x, y, this.transform.position.z);

        // is this a Token we're placing?
        if (obj is Token)
        {
            Token t = (Token)obj;
            if (t.currentHex)
            {
                t.currentHex.TakeOff(t);
            }
            t.currentHex = this;
            t.Show(inPlayerSight);
            this.tokens.Add(t);
        }
    }

    /// <summary>
    /// Sets the isActive flag of all the hex's
    /// children to the given value.
    /// </summary>
    private void Show(bool val)
    {
        foreach (Transform child in transform)
        {
            //child.gameObject.SetActive(val);
            child.GetComponent<Renderer>().enabled = val;
        }
    }

    public void SetInPlayerSight(bool val)
    {
        Discover();
        inPlayerSight = val;
        ShowHideTokens();
    }

    /// <summary>
    /// Shows or hides this hex's tokens, based on the visible flag.
    /// </summary>
    private void ShowHideTokens()
    {
        foreach (Token t in tokens)
        {
            t.Show(inPlayerSight);
        }
    }

    /// <summary>
    /// Sets the hex's color to the given value.
    /// </summary>
    /// <param name="c">The color to be changed to.</param>
    private void SetColor(Color c)
    {
        MeshRenderer curRendy;
        foreach (Transform child in transform)
        {
            curRendy = child.gameObject.GetComponent<MeshRenderer>();
            curRendy.material.color = c;
        }
    }

    /// <summary>
    /// Sets the hex's color based on its state and TileType.
    /// </summary>
    public void SetColor()
    {
        SetColor(tileType.color);
    }

    public void TakeOff(Token t)
    {
        if (tokens.Contains(t))
        {
            tokens.Remove(t);
        }
    }
    #endregion

    #region Mouse Interaction
    void OnMouseDown()
    {
        clickDelegate.onClick.Invoke(GetRow(), GetColumn());
    }

    private void OnMouseEnter()
    {
        SetColor(Color.white);
    }

    private void OnMouseExit()
    {
        SetColor();
    }
    #endregion

    #region Misc
    public override string ToString()
    {
        return $"Tile {GetRow()}, {GetColumn()}";
    }

    /// <summary>
    /// Determines this tile's row based on its transform position.
    /// </summary>
    /// <returns>A row index for the board.</returns>
    private int GetRow()
    {
        return (int) this.transform.position.z;
    }

    /// <summary>
    /// Determines this tile's column based on its transform position.
    /// </summary>
    /// <returns>A column index for the board.</returns>
    private int GetColumn()
    {
        float rawVal = this.transform.position.x;

        // due to the x-offset, we do some fun rounding

        if (GetRow() % 2 == 0)
        {
            return (int) rawVal; // even rows play nice
        }
        else
        {
            return (int) (rawVal - 0.5) + 1; // the 0th of an odd row would be at -0.5, so adjust
        }
    }
    #endregion
}
