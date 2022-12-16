using System;
using UnityEngine;
using UnityEngine.Events;

public class Hex : MonoBehaviour
{
    public int row { get => GetRow(); }
    public int column { get => GetColumn(); }

    public TileType tileType { get; private set; }

    public bool discovered { get; private set; }

    public bool isLand { get; private set; }

    public bool initialized { get; private set; } = false;

    private HexClickDelegateHandler clickDelegate;

    #region Initialization
    void Awake()
    {
        if (!initialized) {
            this.name = this.ToString();
            Discover(false);
        }
        else
        {
            throw new Exception($"{this.name} is already initialized.");
        }
    }

    public void Init(HexClickDelegateHandler clickDelegate)
    {
        this.clickDelegate = clickDelegate;
    }

    public void SetType(TileType tileType)
    {
        this.tileType = tileType;
        this.transform.localScale = new Vector3(1, tileType.height, 1);
        this.SetColor();
    }
    #endregion

    #region State Control
    public void Discover(bool val = true)
    {
        discovered = val;
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
            ((Token)obj).currentHex = this;
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
            child.gameObject.SetActive(val);
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
    #endregion


    #region User Interaction
    void OnMouseDown()
    {
        clickDelegate.onClick.Invoke(GetRow(), GetColumn());
    }

    private void OnMouseEnter()
    {
        SetColor(Color.clear);
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
