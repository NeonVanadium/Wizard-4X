using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineManager : MonoBehaviour
{

    [SerializeField] Outline HexOutlinePrefab;

    private Color moveColor = Color.cyan; // color of the move markers

    private List<Outline> outlines = new List<Outline>();

    public Outline CreateOutline(Color c)
    {
        Outline newOutline = Instantiate(HexOutlinePrefab);
        newOutline.SetColor(c);
        outlines.Add(newOutline);
        return newOutline;
    }

    public Outline CreateOutline()
    {
        return CreateOutline(moveColor);
    }

    public void ClearAllOutlines()
    {
        foreach (Outline o in outlines)
        {
            Destroy(o.gameObject);
        }
        outlines.Clear();
    }

    public void ClearMoveMarkers()
    {
        System.Predicate<Outline> pred = new System.Predicate<Outline>((Outline o) => o.color == moveColor);
        foreach (Outline o in outlines)
        {
            if (pred(o))
                Destroy(o.gameObject);
        }
        outlines.RemoveAll(pred);
    }

}
