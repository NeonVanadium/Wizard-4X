using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Outline : MonoBehaviour
{
    public Color color { get; private set; }

    private LineRenderer rendy;

    private void Awake()
    {
        rendy = GetComponent<LineRenderer>();
        Hide();
    }

    private void SetColor(Color c)
    {
        color = c;
        rendy.startColor = color;
        rendy.endColor = color;
    }

    public void Show(Color c)
    {
        rendy.enabled = true;
        SetColor(c);
    }

    public void Hide()
    {
        rendy.enabled = false;
    }
}
