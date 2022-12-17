using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineManager : MonoBehaviour
{

    [SerializeField] Token HexOutlinePrefab;

    private Color moveColor = Color.cyan; // color of the move markers

    private List<Token> outlines = new List<Token>();

    public Token CreateOutline(Color c)
    {
        Token newOutline = Instantiate(HexOutlinePrefab);
        LineRenderer rendy = newOutline.GetComponent<LineRenderer>();
        rendy.startColor = c;
        rendy.endColor = c;
        newOutline.color = c;
        outlines.Add(newOutline);
        return newOutline;
    }

    public Token CreateOutline()
    {
        return CreateOutline(moveColor);
    }

    public void ClearAllOutlines()
    {
        foreach (Token token in outlines)
        {
            Destroy(token.gameObject);
        }
        outlines.Clear();
    }

    public void ClearMoveMarkers()
    {
        System.Predicate<Token> pred = new System.Predicate<Token>((Token t) => t.color == moveColor);
        foreach (Token token in outlines)
        {
            if (pred(token))
                Destroy(token.gameObject);
        }
        outlines.RemoveAll(pred);
    }

}
