using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubmitEntry {
    private int _v1;
    private int _v2;

    private int _answer;

    public SubmitEntry(int v1, int v2, int answer)
    {
        _v1 = v1;
        _v2 = v2;
        _answer = answer;
    }

    public int Get()
    {
        return _answer;
    }

    public int Get(int v)
    {
        return new int[] { _v1, _v2 }[v];
    }

    public void SetTile(GemTile tile)
    {
        tile.Set(_v1, _v2);
    }
}
