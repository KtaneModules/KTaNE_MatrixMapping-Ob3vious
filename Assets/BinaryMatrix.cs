using System.Collections.Generic;

public class BinaryMatrix {
    public enum State
    {
        Null,
        Pass,
        Stop,
        Absent
    }

    private State[,] _grid;

    private int _s1;
    private int _s2;

    public BinaryMatrix(int s1, int s2)
    {
        _s1 = s1;
        _s2 = s2;

        _grid = new State[_s1, _s2];
        for (int i = 0; i < _s1; i++)
            for (int j = 0; j < _s2; j++)
                _grid[i, j] = State.Absent;
    }

    public BinaryMatrix(BinaryMatrix toCopy)
    {
        _s1 = toCopy._s1;
        _s2 = toCopy._s2;

        _grid = new State[_s1, _s2];
        for (int i = 0; i < _s1; i++)
            for (int j = 0; j < _s2; j++)
                _grid[i, j] = toCopy.Get(i, j);
    }

    public State Get(int fromType, int toType)
    {
        return _grid[fromType, toType];
    }

    public void Set(int fromType, int toType, State state)
    {
        _grid[fromType, toType] = state;
    }

    public List<SubmitEntry> GetEntries()
    {
        List<SubmitEntry> entries = new List<SubmitEntry>();
        for (int i = 0; i < _s1; i++)
            for (int j = 0; j < _s2; j++)
                if (Get(i, j) != State.Absent)
                    entries.Add(new SubmitEntry(i, j, Get(i, j) == State.Pass ? 1 : 0));
        return entries;
    }

    public override string ToString()
    {
        string returnValue = "";
        for (int i = 0; i < _s1; i++)
        {
            for (int j = 0; j < _s2; j++)
            {
                returnValue += "?10-"[(int)_grid[i, j]];
            }
            if (i != _s1 - 1)
                returnValue += "/";
        }
        return returnValue;
    }
}
