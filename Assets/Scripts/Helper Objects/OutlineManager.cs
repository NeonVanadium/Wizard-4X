using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineManager : MonoBehaviour
{

    [SerializeField] Token HexOutlinePrefab;

    private List<Token> outlines = new List<Token>();

    public Token CreateOutline()
    {
        Token newOutline = Instantiate(HexOutlinePrefab);
        outlines.Add(newOutline);
        return newOutline;
    }

    public void ClearAllOutlines()
    {
        foreach (Token token in outlines)
        {
            Destroy(token.gameObject);
        }
        outlines.Clear();
    }

}
