using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemTile {
    private ColorManager _colorManager;
    private MeshRenderer _renderer;
    private int[] _values;
    private int _state = 0;

    private Color _lastColor = new Color(0.0625f, 0.0625f, 0.0625f);

    public GemTile(MeshRenderer renderer, ColorManager colorManager)
    {
        _renderer = renderer;
        _colorManager = colorManager;
    }

    public int State()
    {
        return _state;
    }

    public int Get(int state)
    {
        return _values[state];
    }

    public void Set(int v1, int v2)
    {
        _values = new int[] { v1, v2 };
    }

    public void Toggle()
    {
        _state = _state ^ 1;
    }

    public void Update(float lerp)
    {
        //do something to visualise
        _renderer.material.color = Color.Lerp(_lastColor, _colorManager.Get(_state, _values[_state]), lerp);

        if(lerp == 1)
            _lastColor = _colorManager.Get(_state, _values[_state]);
    }

    public override string ToString()
    {
        return _values[0] + "/" + _values[1];
    }
}
