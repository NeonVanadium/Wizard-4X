using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A token is any game piece that is placed on a hex.
/// 
/// Units, structures, etc.
/// </summary>
public class Token : MonoBehaviour
{
    public Hex currentHex { get; set; }

    public Color color { get; private set; }

    public int sight { get => 2; } // how far this token can see

    private MeshRenderer rendy;

    public void Awake()
    {
        rendy = this.GetComponent<MeshRenderer>();

        Show(false);
    }

    public void SetColor(Color c)
    {
        color = c;
        rendy.material.color = color;
    }

    public void Show(bool val = true)
    {
        rendy.enabled = val;
    }


}
