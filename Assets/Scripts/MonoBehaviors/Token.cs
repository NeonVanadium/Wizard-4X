using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A token is anything that is placed on a hex.
/// 
/// Units, structures, tile outlines, etc.
/// </summary>
public class Token : MonoBehaviour
{
    public Hex currentHex { get; set; }

    public Color color { get; set; }

    public void Start()
    {
        MeshRenderer rendy = this.GetComponent<MeshRenderer>();

        if (rendy != null)
            rendy.material.color = color;

    }
}
