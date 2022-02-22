using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class MazeGrid {
    private GemTile[,] _grid;
    private ColorManager _colorSet;

    private int _size;
    private int _s1;
    private int _s2;

    public MazeGrid (int size, int s1, int s2)
    {
        _grid = new GemTile[size, size];
        _colorSet = new ColorManager(s1, s2);

        _size = size;
        _s1 = s1;
        _s2 = s2;
    }

    public void SetTile(int x, int y, MeshRenderer renderer)
    {
        _grid[y, x] = new GemTile(renderer, _colorSet);
    }

    public void Initialise()
    {
        do
            Randomise();
        while (GetMatrix() == null);
    }

    public void Randomise()
    {
        for (int i = 0; i < _size; i++)
            for (int j = 0; j < _size; j++)
            {
                int v1 = Rnd.Range(0, _s1);
                int v2 = Rnd.Range(0, _s2);
                _grid[j, i].Set(v1, v2);
            }
    }

    public void Empty()
    {
        for (int i = 0; i < _size; i++)
            for (int j = 0; j < _size; j++)
                _grid[j, i].Set(0, 0);
    }

    public ColorManager GetColorSet()
    {
        return _colorSet;
    }

    public void Toggle()
    {
        for (int i = 0; i < _size; i++)
            for (int j = 0; j < _size; j++)
                _grid[j, i].Toggle();
    }

    public void Update(float lerp)
    {
        for (int i = 0; i < _size; i++)
            for (int j = 0; j < _size; j++)
                _grid[j, i].Update(lerp);
    }

    public BinaryMatrix GetMatrix()
    {
        BinaryMatrix m = new BinaryMatrix(_s1, _s2);
        FindPresent(m);

        while (true) {
            BinaryMatrix newM;

            newM = TrySingleFalse(m);
            if (newM != null)
            {
                m = newM;
                continue;
            }

            newM = TryLineFill(m);
            if (newM != null)
            {
                m = newM;
                continue;
            }

            newM = TryAllFalse(m);
            if (newM != null)
            {
                m = newM;
                break;
            }

            break;
        }

        if (IsValidMatrix(m))
        {
            return m;
        }
        return null;
    }

    private bool IsValidMatrix(BinaryMatrix m)
    {
        for (int i = 0; i < _s1; i++)
            for (int j = 0; j < _s2; j++)
                if (m.Get(i, j) == BinaryMatrix.State.Null)
                    return false;

        for (int i = 0; i < _s1; i++)
        {
            bool needsStop = true;
            for (int j = 0; j < _s2; j++)
                if (m.Get(i, j) == BinaryMatrix.State.Stop)
                    needsStop = false;
            if (needsStop)
                return false;
        }
        for (int i = 0; i < _s2; i++)
        {
            bool needsStop = true;
            for (int j = 0; j < _s1; j++)
                if (m.Get(j, i) == BinaryMatrix.State.Stop)
                    needsStop = false;
            if (needsStop)
                return false;
        }

        return IsNavigable(m);
    }

    private BinaryMatrix TryLineFill(BinaryMatrix m)
    {
        BinaryMatrix returnValue = new BinaryMatrix(m);
        for (int i = 0; i < _s1; i++)
        {
            bool needsStop = true;
            int nullCount = 0;
            for (int j = 0; j < _s2; j++)
                switch (m.Get(i, j))
                {
                    case BinaryMatrix.State.Null:
                        nullCount++;
                        break;
                    case BinaryMatrix.State.Stop:
                        needsStop = false;
                        break;
                }
            if (!needsStop)
                continue;
            if (nullCount == 1)
                for (int j = 0; j < _s2; j++)
                    if (m.Get(i, j) == BinaryMatrix.State.Null)
                    {
                        returnValue.Set(i, j, BinaryMatrix.State.Stop);
                        return returnValue;
                    }
        }

        for (int i = 0; i < _s2; i++)
        {
            bool needsStop = true;
            int nullCount = 0;
            for (int j = 0; j < _s1; j++)
                switch (m.Get(j, i))
                {
                    case BinaryMatrix.State.Null:
                        nullCount++;
                        break;
                    case BinaryMatrix.State.Stop:
                        needsStop = false;
                        break;
                }
            if (!needsStop)
                continue;
            if (nullCount == 1)
                for (int j = 0; j < _s1; j++)
                    if (m.Get(j, i) == BinaryMatrix.State.Null)
                    {
                        returnValue.Set(j, i, BinaryMatrix.State.Stop);
                        return returnValue;
                    }
        }

        return null;
    }

    private BinaryMatrix TryAllFalse(BinaryMatrix m)
    {
        BinaryMatrix attempt = new BinaryMatrix(m);
        FillNull(attempt, BinaryMatrix.State.Stop);
        if (IsNavigable(attempt))
            return attempt;
        return null;
    }

    private BinaryMatrix TrySingleFalse(BinaryMatrix m)
    {
        for (int i = 0; i < _s1; i++)
            for (int j = 0; j < _s2; j++)
                if (m.Get(i, j) == BinaryMatrix.State.Null)
                {
                    BinaryMatrix attempt = new BinaryMatrix(m);
                    attempt.Set(i, j, BinaryMatrix.State.Stop);
                    FillNull(attempt, BinaryMatrix.State.Pass);

                    if (!IsNavigable(attempt))
                    {
                        BinaryMatrix returnValue = new BinaryMatrix(m);
                        returnValue.Set(i, j, BinaryMatrix.State.Pass);
                        return returnValue;
                    }
                }
        return null;
    }

    private void FindPresent(BinaryMatrix m)
    {
        for (int x1 = 0; x1 < _size; x1++)
            for (int y1 = 0; y1 < _size; y1++)
                for (int x2 = 0; x2 < _size; x2++)
                    for (int y2 = 0; y2 < _size; y2++)
                        if (Mathf.Abs(x1 - x2) + Mathf.Abs(y1 - y2) == 1)
                            m.Set(_grid[y1, x1].Get(0), _grid[y2, x2].Get(1), BinaryMatrix.State.Null);
    }

    private bool IsNavigable(BinaryMatrix m)
    {
        bool[,] fromFirst = new bool[_size,_size];
        bool[,] toFirst = new bool[_size,_size];
        fromFirst[0, 0] = true;
        toFirst[0, 0] = true;

        bool hasUpdated;
        do
        {
            hasUpdated = false;


            //horizontals
            for (int x = 0; x < _size - 1; x++)
                for (int y = 0; y < _size; y++)
                {
                    //going right
                    if (IsPassable(x, y, x + 1, y, m))
                    {
                        hasUpdated |= fromFirst[y, x] && !fromFirst[y, x + 1];
                        fromFirst[y, x + 1] |= fromFirst[y, x];

                        hasUpdated |= toFirst[y, x + 1] && !toFirst[y, x];
                        toFirst[y, x] |= toFirst[y, x + 1];
                    }
                    //going left
                    if (IsPassable(x + 1, y, x, y, m))
                    {
                        hasUpdated |= fromFirst[y, x + 1] && !fromFirst[y, x];
                        fromFirst[y, x] |= fromFirst[y, x + 1];

                        hasUpdated |= toFirst[y, x] && !toFirst[y, x + 1];
                        toFirst[y, x + 1] |= toFirst[y, x];
                    }
                }

            for (int x = 0; x < _size; x++)
                for (int y = 0; y < _size - 1; y++)
                {
                    //going down
                    if (IsPassable(x, y, x, y + 1, m))
                    {
                        hasUpdated |= fromFirst[y, x] && !fromFirst[y + 1, x];
                        fromFirst[y + 1, x] |= fromFirst[y, x];

                        hasUpdated |= toFirst[y + 1, x] && !toFirst[y, x];
                        toFirst[y, x] |= toFirst[y + 1, x];
                    }
                    //going up
                    if (IsPassable(x, y + 1, x, y, m))
                    {
                        hasUpdated |= fromFirst[y + 1, x] && !fromFirst[y, x];
                        fromFirst[y, x] |= fromFirst[y + 1, x];

                        hasUpdated |= toFirst[y, x] && !toFirst[y + 1, x];
                        toFirst[y + 1, x] |= toFirst[y, x];
                    }
                }
        } while (hasUpdated);

        bool isNavigable = true;

        for (int x = 0; x < _size; x++)
            for (int y = 0; y < _size; y++)
                isNavigable &= fromFirst[y, x] && toFirst[y, x];

        return isNavigable;
    }

    private bool IsPassable(int x1, int y1, int x2, int y2, BinaryMatrix m)
    {
        return m.Get(_grid[y1, x1].Get(0), _grid[y2, x2].Get(1)) == BinaryMatrix.State.Pass;
    }

    private void FillNull(BinaryMatrix m, BinaryMatrix.State state)
    {
        for (int i = 0; i < _s1; i++)
            for (int j = 0; j < _s2; j++)
                if (m.Get(i, j) == BinaryMatrix.State.Null)
                    m.Set(i, j, state);
    }

    public override string ToString()
    {
        string returnValue = "";
        for (int i = 0; i < _size; i++)
        {
            for (int j = 0; j < _size; j++)
                returnValue += _grid[i, j].Get(0) + 1;

            if (i != _size - 1)
                returnValue += "/";
        }
        returnValue += ", ";
        for (int i = 0; i < _size; i++)
        {
            for (int j = 0; j < _size; j++)
                returnValue += _grid[i, j].Get(1) + 1;

            if (i != _size - 1)
                returnValue += "/";
        }
        return returnValue;
    }
}
